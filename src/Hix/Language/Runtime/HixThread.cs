using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Hix.Runtime;

/// <summary>A wakeable event loop which interleaves independently owned coroutine activations.</summary>
public sealed class HixThread {
    [ThreadStatic]
    private static HixThread current;
    private readonly Stack<HixValueDictionary> localPool = new();
    private readonly ConcurrentQueue<Work> queue = new();
    private readonly ConcurrentQueue<Action> events = new();
    private readonly ConcurrentQueue<Work> stealable = new();
    private readonly ConcurrentQueue<Work> submissions;
    private readonly HixThread[] peers;
    private readonly AutoResetEvent wake = new(false);
    private readonly CancellationToken stopping;
    private int driving;
    private readonly object lifecycle = new();
    private int loopThread;
    private int ownerThread;
    private bool preferPinned = true;
    private bool preferSubmission = true;
    private bool preferEvent = true;
    private Thread nativeThread;

    public HixThread() {
    }

    private HixThread(int ownerThread) {
        this.ownerThread = ownerThread;
    }

    internal HixThread(ConcurrentQueue<Work> submissions, HixThread[] peers, CancellationToken stopping) {
        this.submissions = submissions;
        this.peers = peers;
        this.stopping = stopping;
    }

    public static HixThread Current => current ??= new HixThread(Thread.CurrentThread.ManagedThreadId);
    internal bool IsIdle => Volatile.Read(ref driving) == 0;

    public HixTask<HixInvocationResult> Enqueue(HixCoroutine coroutine) {
        if (coroutine == null)
            throw new ArgumentNullException(nameof(coroutine));
        return Enqueue(() => coroutine);
    }

    public void Post(Action action) {
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        events.Enqueue(action);
        Notify();
    }

    /// <summary>Execute one bounded event immediately. A manual loop may call this repeatedly.</summary>
    public bool Tick(int instructionBudget = 1000) {
        if (instructionBudget <= 0)
            throw new ArgumentOutOfRangeException(nameof(instructionBudget));
        var caller = Thread.CurrentThread.ManagedThreadId;
        Interlocked.CompareExchange(ref ownerThread, caller, 0);
        if (Volatile.Read(ref ownerThread) != caller)
            throw new InvalidOperationException("This worker belongs to another OS thread");
        var owner = Volatile.Read(ref loopThread);
        if (owner != 0 && owner != Thread.CurrentThread.ManagedThreadId)
            throw new InvalidOperationException("The event loop belongs to another OS thread");
        if (Interlocked.CompareExchange(ref driving, 1, 0) != 0)
            throw new InvalidOperationException("A worker cannot be driven concurrently");
        var previous = current;
        current = this;
        try {
            if (preferEvent && events.TryDequeue(out var action)) {
                preferEvent = false;
                action();
                return true;
            }

            if (TryTake(out var work)) {
                preferEvent = true;
                Execute(work, instructionBudget);
                return true;
            }

            if (!events.TryDequeue(out action))
                return false;
            action();
            return true;
        } finally {
            current = previous;
            Volatile.Write(ref driving, 0);
        }
    }

    public void RunUntilIdle(int instructionBudget = 1000) {
        while (Tick(instructionBudget)) {
        }
    }

    /// <summary>Drive this worker until a completion is ready, sleeping when no event is runnable.</summary>
    public T RunUntil<T>(HixTask<T> completion, int instructionBudget = 1000) {
        if (completion == null)
            throw new ArgumentNullException(nameof(completion));
        if (instructionBudget <= 0)
            throw new ArgumentOutOfRangeException(nameof(instructionBudget));
        completion.OnCompleted(Notify);
        while (!completion.IsCompleted) {
            if (!Tick(instructionBudget) && !completion.IsCompleted)
                wake.WaitOne();
        }
        return completion.GetResult();
    }

    /// <summary>Run an event loop on this OS thread, sleeping when no coroutine is runnable.</summary>
    public void Run(CancellationToken cancellationToken = default) {
        if (Interlocked.CompareExchange(ref loopThread, Thread.CurrentThread.ManagedThreadId, 0) != 0)
            throw new InvalidOperationException("The worker event loop is already running");
        var handles = new[] {
            wake,
            cancellationToken.WaitHandle
        };
        try {
            while (!cancellationToken.IsCancellationRequested) {
                if (!Tick())
                    WaitHandle.WaitAny(handles);
            }
        } finally {
            Volatile.Write(ref loopThread, 0);
        }
    }

    internal HixTask<HixInvocationResult> Enqueue(Func<HixCoroutine> create) {
        var work = new Work(create);
        queue.Enqueue(work);
        Notify();
        return work.Completion;
    }

    internal HixValueDictionary RentLocals() => localPool.Count == 0 ? new HixValueDictionary(mutable: true) : localPool.Pop();

    internal void ReturnLocals(HixValueDictionary locals) {
        locals.Clear();
        if (localPool.Count < 128)
            localPool.Push(locals);
    }

    internal void Notify() {
        if (stopping.IsCancellationRequested)
            return;
        wake.Set();
    }

    internal void Start() {
        lock (lifecycle) {
            if (nativeThread == null && !stopping.IsCancellationRequested) {
                nativeThread = new Thread(() => Run(stopping)) {
                    IsBackground = true,
                    Name = "Hix worker"
                };
                nativeThread.Start();
            }
        }
    }

    internal void Join() {
        Thread thread;
        lock (lifecycle)
            thread = nativeThread;
        if (thread != null && thread != Thread.CurrentThread)
            thread.Join();
    }

    private void Execute(Work work, int instructionBudget) {
        try {
            work.Coroutine ??= work.Create();
            if (work.Coroutine.Resume(instructionBudget))
                work.Completion.TrySetResult(work.Coroutine.Result);
            else if (work.Coroutine.PendingOperation is { } pending) {
                pending.OnCompleted(() => {
                    if (stopping.IsCancellationRequested)
                        return;
                    Requeue(work);
                    Notify();
                });
            } else
                Requeue(work);
        } catch (OperationCanceledException) {
            work.Completion.TrySetCanceled();
        } catch (Exception error) {
            work.Completion.TrySetException(error);
        }
    }

    private bool TryTake(out Work work) {
        if (preferSubmission && submissions != null && submissions.TryDequeue(out work)) {
            preferSubmission = false;
            return true;
        }

        if (TryLocal(out work)) {
            preferSubmission = true;
            return true;
        }

        if (submissions != null && submissions.TryDequeue(out work))
            return true;
        if (peers != null)
            foreach (var peer in peers)
                if (peer != null && !ReferenceEquals(peer, this) && peer.stealable.TryDequeue(out work))
                    return true;
        work = null;
        return false;
    }

    private void Requeue(Work work) => (work.CanSteal ? stealable : queue).Enqueue(work);

    private bool TryLocal(out Work work) {
        if (preferPinned && queue.TryDequeue(out work)) {
            preferPinned = false;
            return true;
        }

        if (stealable.TryDequeue(out work)) {
            preferPinned = true;
            return true;
        }

        return queue.TryDequeue(out work);
    }

    internal sealed class Work {
        internal readonly Func<HixCoroutine> Create;
        internal readonly bool CanSteal;
        internal HixCoroutine Coroutine;
        internal readonly HixTask<HixInvocationResult> Completion = new();

        internal Work(Func<HixCoroutine> create, bool canSteal = false) {
            Create = create;
            CanSteal = canSteal;
        }
    }
}

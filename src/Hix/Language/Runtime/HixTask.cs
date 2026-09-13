using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Hix.Runtime;

/// <summary>Hix-owned completion state. Completion callbacks only notify event queues.</summary>
public sealed class HixTask<T> {
    private readonly object gate = new();
    private Action continuations;
    private T result;
    private ExceptionDispatchInfo failure;
    private int state;

    public bool IsCompleted => Volatile.Read(ref state) != 0;
    public bool IsCompletedSuccessfully => Volatile.Read(ref state) == 1;
    public bool IsFaulted => Volatile.Read(ref state) == 2;
    public bool IsCanceled => Volatile.Read(ref state) == 3;
    public T Result => GetResult();

    public bool TrySetResult(T value) => Complete(1, value, null);

    public bool TrySetException(Exception exception) => Complete(2, default, ExceptionDispatchInfo.Capture(exception ?? throw new ArgumentNullException(nameof(exception))));

    public bool TrySetCanceled() => Complete(3, default, null);

    public bool Wait(TimeSpan timeout) {
        if (timeout < TimeSpan.Zero && timeout != Timeout.InfiniteTimeSpan)
            throw new ArgumentOutOfRangeException(nameof(timeout));
        var started = Stopwatch.StartNew();
        lock (gate) {
            while (state == 0) {
                if (timeout == Timeout.InfiniteTimeSpan)
                    Monitor.Wait(gate);
                else {
                    var remaining = timeout - started.Elapsed;
                    if (remaining <= TimeSpan.Zero)
                        return false;
                    Monitor.Wait(gate, remaining);
                }
            }

            return true;
        }
    }

    public T GetResult() {
        lock (gate) {
            while (state == 0)
                Monitor.Wait(gate);
            failure?.Throw();
            if (state == 3)
                throw new OperationCanceledException("Hix task was cancelled");
            return result;
        }
    }

    public Awaiter GetAwaiter() => new(this, HixThread.Current);

    /// <summary>Register a completion notification. The callback runs on the completing thread; use the awaiter to queue user code on its worker.</summary>
    public void OnCompleted(Action continuation) {
        if (continuation == null)
            throw new ArgumentNullException(nameof(continuation));
        lock (gate) {
            if (state == 0) {
                continuations += continuation;
                return;
            }
        }

        continuation();
    }

    private bool Complete(int completedState, T value, ExceptionDispatchInfo exception) {
        Action ready;
        lock (gate) {
            if (state != 0)
                return false;
            result = value;
            failure = exception;
            ready = continuations;
            continuations = null;
            Volatile.Write(ref state, completedState);
            Monitor.PulseAll(gate);
        }

        ready?.Invoke();
        return true;
    }

    public readonly struct Awaiter : INotifyCompletion {
        private readonly HixTask<T> task;
        private readonly HixThread worker;

        internal Awaiter(HixTask<T> task, HixThread worker) {
            this.task = task;
            this.worker = worker;
        }

        public bool IsCompleted => task.IsCompleted;

        public T GetResult() => task.GetResult();

        public void OnCompleted(Action continuation) {
            if (continuation == null)
                throw new ArgumentNullException(nameof(continuation));
            var target = worker;
            task.OnCompleted(() => target.Post(continuation));
        }
    }
}

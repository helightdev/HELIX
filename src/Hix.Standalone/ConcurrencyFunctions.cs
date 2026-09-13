using System;
using System.Collections.Generic;
using System.Threading;
using Hix.Compiler;
using Hix.Runtime;
using K = Hix.HixValueKind;

namespace Hix.Standalone;

internal static class ConcurrencyFunctions {
  public static void Register(FunctionSignatureRegistryBuilder functions) {
    functions.Add(
      Sync("spawn", new(K.Symbol, [K.Function, K.Tuple, K.Bool],
        ArgumentNames: ["function", "arguments", "useThreadPool"],
        ArgumentDefaults: [null, new TupleExpressionIr([]), new BooleanExpressionIr(false)]), Spawn,
        "Queues a Hix function or lambda on the current worker, or explicitly on the VM pool. Arguments are positional."),
      Async("awaitTask", new(K.Any, [K.Symbol]), (thread, args) => Handle<TaskHandle>(args[0], "task").Completion,
        "Suspends until a task finishes and returns its value, propagating errors."),
      Async("awaitAll", new(K.Tuple, [K.Tuple]), (thread, args) => Combine(thread, (TupleHixValue)args[0], false),
        "Waits for every task and returns results in input order. An empty tuple returns an empty tuple."),
      Async("awaitAny", new(K.Table, [K.Tuple]), (thread, args) => Combine(thread, (TupleHixValue)args[0], true),
        "Returns {index, value} for the first completed task. Empty input is an error; other tasks keep running."),
      Sync("taskDone", new(K.Bool, [K.Symbol]), (thread, args) => BooleanHixValue.From(Handle<TaskHandle>(args[0], "task").Completion.IsCompleted),
        "Tests completion without waiting."),
      Async("yield", new(K.Null, []), (thread, args) => {
        RequireSuspendable(thread);
        var completion = new HixTask<IHixValue>();
        HixThread.Current.Post(() => completion.TrySetResult(NullHixValue.Instance));
        return completion;
      }, "Yields to other runnable events on the current worker."),
      Async("sleep", new(K.Null, [K.Number], ArgumentNames: ["milliseconds"]), Sleep,
        "Suspends for a non-negative number of milliseconds without blocking the worker."),
      Sync("workerId", new(K.Number, []), (thread, args) => new NumberHixValue(Thread.CurrentThread.ManagedThreadId),
        "Returns the current worker's OS thread ID."),
      Sync("channel", new(K.Symbol, [K.Number], ArgumentNames: ["capacity"], ArgumentDefaults: [new NumberExpressionIr(0)]),
        (thread, args) => new ObjectHixValue(new Channel(Count(args[0], "capacity"))),
        "Creates a FIFO channel. Capacity zero is a rendezvous channel; positive capacity buffers messages."),
      Async("send", new(K.Null, [K.Symbol, K.Any]), (thread, args) => Handle<Channel>(args[0], "channel").Send(thread, thread.DetachValue(args[1])),
        "Sends a value, suspending when the channel has no space."),
      Async("receive", new(K.Any, [K.Symbol]), (thread, args) => Handle<Channel>(args[0], "channel").Receive(thread),
        "Receives the next value, suspending when the channel is empty. Closed and drained channels return an error."),
      Sync("closeChannel", new(K.Bool, [K.Symbol]), (thread, args) => BooleanHixValue.From(Handle<Channel>(args[0], "channel").Close()),
        "Closes a channel, wakes waiting senders/receivers, and preserves buffered messages. Returns false if already closed."),
      Sync("semaphore", new(K.Symbol, [K.Number, K.Number], ArgumentNames: ["permits", "maximum"],
        ArgumentDefaults: [new NumberExpressionIr(1), new NumberExpressionIr(int.MaxValue)]), (thread, args) =>
          new ObjectHixValue(new Semaphore(Count(args[0], "permits"), Count(args[1], "maximum"))),
        "Creates a FIFO semaphore with non-negative initial permits and a positive maximum. One permit can be used as a mutex."),
      Async("acquire", new(K.Null, [K.Symbol]), (thread, args) => Handle<Semaphore>(args[0], "semaphore").Acquire(thread),
        "Acquires one permit, suspending when none is available."),
      Sync("release", new(K.Null, [K.Symbol]), (thread, args) => {
        Handle<Semaphore>(args[0], "semaphore").Release();
        return NullHixValue.Instance;
      }, "Releases one permit and wakes the oldest waiter. Exceeding the maximum is an error.")
    );
  }

  private static FunctionDefinition Sync(string name, FunctionSignature signature, InlineFunction action, string documentation) =>
    new SimpleFunction(name, [signature], action, effects: true, documentation: documentation);

  private static FunctionDefinition Async(string name, FunctionSignature signature,
    Func<HixCoroutine, IHixValue[], HixTask<IHixValue>> action, string documentation) => new SuspendingFunction(name, signature, action, documentation);

  private static T Handle<T>(IHixValue value, string name) where T : class =>
    value is ObjectHixValue { Value: T handle } ? handle : throw new ArgumentException("expected a " + name + " handle");

  private static int Count(IHixValue value, string name) {
    var number = ((NumberHixValue)value).Value;
    if (double.IsNaN(number) || number < 0 || number > int.MaxValue || number != Math.Truncate(number))
      throw new ArgumentException(name + " must be a non-negative integer");
    return (int)number;
  }

  private static void RequireSuspendable(HixCoroutine thread) {
    if (!thread.CanSuspend)
      throw new ArgumentException("this operation cannot suspend inside a synchronous host callback");
  }

  private static HixTask<IHixValue> Ready(IHixValue value) {
    var completion = new HixTask<IHixValue>();
    completion.TrySetResult(value);
    return completion;
  }

  private static IHixValue Spawn(HixCoroutine thread, IHixValue[] args) {
    if (args[0] is ResolvedFunctionHixValue { Backend: not null })
      return thread.Error("spawn requires a Hix function or lambda, not a host function");
    var values = ((TupleHixValue)args[1]).Values;
    var arguments = new IHixValue[values.Count];
    for (var index = 0; index < arguments.Length; index++)
      arguments[index] = thread.DetachValue(values[index]);
    HixTask<HixInvocationResult> source;
    try {
      source = thread.Machine.InvokeAsync(thread.Program, args[0], useThreadPool: ((BooleanHixValue)args[2]).Value, arguments: arguments);
    } catch (InvalidOperationException error) {
      return thread.Error(error.Message);
    }
    var handle = new TaskHandle();
    source.OnCompleted(() => {
      try {
        var result = source.GetResult();
        handle.Completion.TrySetResult(Transfer(result.Value, result.Execution.Strings));
      }
      catch (OperationCanceledException) { handle.Completion.TrySetResult(new ErrorHixValue(HixString.Dynamic("task was cancelled"))); }
      catch (Exception error) { handle.Completion.TrySetResult(new ErrorHixValue(HixString.Dynamic(error.Message))); }
    });
    return new ObjectHixValue(handle);
  }

  private static IHixValue Transfer(IHixValue value, HixStringPool strings) {
    switch (value) {
      case LiteralHixValue literal when literal.Value.IsInterned:
        return new LiteralHixValue(HixString.Dynamic(literal.Value.Resolve(strings)));
      case ErrorHixValue error:
        return error with { Message = HixString.Dynamic(error.Message.Resolve(strings)) };
      case TupleHixValue tuple:
        return tuple.Transform(item => Transfer(item, strings));
      case HixTableValue table:
        var entries = new KeyValuePair<HixString, IHixValue>[table.Count];
        var index = 0;
        foreach (var entry in table.Entries)
          entries[index++] = new(HixString.Dynamic(entry.Key.Resolve(strings)), Transfer(entry.Value, strings));
        return new HixTableValue(entries);
      default:
        return value;
    }
  }

  private static HixTask<IHixValue> Combine(HixCoroutine thread, TupleHixValue values, bool any) {
    if (values.Values.Count == 0)
      return Ready(any ? thread.Error("awaitAny requires at least one task") : TupleHixValue.Empty);
    var tasks = new HixTask<IHixValue>[values.Values.Count];
    for (var index = 0; index < tasks.Length; index++)
      tasks[index] = Handle<TaskHandle>(values.Values[index], "task").Completion;
    var completion = new HixTask<IHixValue>();
    var results = new IHixValue[tasks.Length];
    var remaining = tasks.Length;
    for (var index = 0; index < tasks.Length; index++) {
      var position = index;
      tasks[index].OnCompleted(() => {
        var value = tasks[position].GetResult();
        if (any) {
          completion.TrySetResult(new HixTableValue([
            new(HixString.Dynamic("index"), new NumberHixValue(position)),
            new(HixString.Dynamic("value"), value)
          ]));
        } else {
          results[position] = value;
          if (Interlocked.Decrement(ref remaining) == 0) {
            IHixValue result = new TupleHixValue(results);
            foreach (var item in results)
              if (item is ErrorHixValue) { result = item; break; }
            completion.TrySetResult(result);
          }
        }
      });
    }
    return completion;
  }

  private static HixTask<IHixValue> Sleep(HixCoroutine thread, IHixValue[] args) {
    var delay = Count(args[0], "milliseconds");
    RequireSuspendable(thread);
    var completion = new HixTask<IHixValue>();
    // A timer only publishes completion; coroutine execution always resumes through its worker queue.
    var timer = new Timer(_ => completion.TrySetResult(NullHixValue.Instance), null, Timeout.Infinite, Timeout.Infinite);
    completion.OnCompleted(() => timer.Dispose());
    timer.Change(delay, Timeout.Infinite);
    return completion;
  }

  private sealed class SuspendingFunction(string name, FunctionSignature signature,
    Func<HixCoroutine, IHixValue[], HixTask<IHixValue>> action, string documentation) : FunctionDefinition(name, [signature], documentation) {
    public override bool HasEffects => true;

    public override HixTask<IHixValue> ExecuteAsync(HixCoroutine thread, IHixValue[] arguments, int line) {
      var completion = action(thread, arguments);
      if (!completion.IsCompleted)
        RequireSuspendable(thread);
      return completion;
    }
  }

  private sealed class TaskHandle {
    internal readonly HixTask<IHixValue> Completion = new();
    public override string ToString() => Completion.IsCompleted ? "<task completed>" : "<task pending>";
  }

  private sealed class Channel(int capacity) {
    private readonly object gate = new();
    private readonly Queue<IHixValue> buffer = new();
    private readonly Queue<HixTask<IHixValue>> receivers = new();
    private readonly Queue<(IHixValue Value, HixTask<IHixValue> Completion)> senders = new();
    private bool closed;

    internal HixTask<IHixValue> Send(HixCoroutine thread, IHixValue value) {
      HixTask<IHixValue> receiver = null;
      lock (gate) {
        if (closed) return Ready(thread.Error("channel is closed"));
        if (receivers.Count != 0) receiver = receivers.Dequeue();
        else if (buffer.Count < capacity) buffer.Enqueue(value);
        else {
          RequireSuspendable(thread);
          var waiting = new HixTask<IHixValue>();
          senders.Enqueue((value, waiting));
          return waiting;
        }
      }
      receiver?.TrySetResult(value);
      return Ready(NullHixValue.Instance);
    }

    internal HixTask<IHixValue> Receive(HixCoroutine thread) {
      HixTask<IHixValue> sender = null;
      IHixValue value;
      lock (gate) {
        if (buffer.Count != 0) {
          value = buffer.Dequeue();
          if (senders.Count != 0) {
            var next = senders.Dequeue();
            buffer.Enqueue(next.Value);
            sender = next.Completion;
          }
        } else if (senders.Count != 0) {
          var next = senders.Dequeue();
          value = next.Value;
          sender = next.Completion;
        } else if (closed) return Ready(thread.Error("channel is closed"));
        else {
          RequireSuspendable(thread);
          var waiting = new HixTask<IHixValue>();
          receivers.Enqueue(waiting);
          return waiting;
        }
      }
      sender?.TrySetResult(NullHixValue.Instance);
      return Ready(value);
    }

    internal bool Close() {
      HixTask<IHixValue>[] waitingReceivers;
      (IHixValue Value, HixTask<IHixValue> Completion)[] waitingSenders;
      lock (gate) {
        if (closed) return false;
        closed = true;
        waitingReceivers = receivers.ToArray();
        waitingSenders = senders.ToArray();
        receivers.Clear();
        senders.Clear();
      }
      var error = new ErrorHixValue(HixString.Dynamic("channel is closed"));
      foreach (var receiver in waitingReceivers) receiver.TrySetResult(error);
      foreach (var sender in waitingSenders) sender.Completion.TrySetResult(error);
      return true;
    }

    public override string ToString() => "<channel>";
  }

  private sealed class Semaphore {
    private readonly object gate = new();
    private readonly Queue<HixTask<IHixValue>> waiters = new();
    private readonly int maximum;
    private int permits;

    internal Semaphore(int permits, int maximum) {
      if (maximum == 0 || permits > maximum)
        throw new ArgumentException("maximum must be positive and at least permits");
      this.permits = permits;
      this.maximum = maximum;
    }

    internal HixTask<IHixValue> Acquire(HixCoroutine thread) {
      lock (gate) {
        if (permits != 0) {
          permits--;
          return Ready(NullHixValue.Instance);
        }
        RequireSuspendable(thread);
        var completion = new HixTask<IHixValue>();
        waiters.Enqueue(completion);
        return completion;
      }
    }

    internal void Release() {
      HixTask<IHixValue> waiter = null;
      lock (gate) {
        if (waiters.Count != 0) waiter = waiters.Dequeue();
        else {
          if (permits == maximum) throw new ArgumentException("semaphore maximum exceeded");
          permits++;
        }
      }
      waiter?.TrySetResult(NullHixValue.Instance);
    }

    public override string ToString() => "<semaphore>";
  }
}

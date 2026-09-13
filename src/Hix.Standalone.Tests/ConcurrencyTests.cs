using System;
using System.IO;
using System.Linq;
using System.Threading;
using Hix.Compiler;
using Hix.Runtime;
using Xunit;

namespace Hix.Standalone.Tests;

public sealed class ConcurrencyTests {
  [Theory]
  [InlineData(0)]
  [InlineData(1)]
  [InlineData(4)]
  public void MainAndDefaultSpawnStayOnCallingThreadIndependentlyOfPool(int workers) {
    var caller = Thread.CurrentThread.ManagedThreadId;
    var value = Run("""
      func child { yield(); sleep(1); return(workerId()) }
      func main {
        local task = spawn(child)
        return(@[workerId(), awaitTask(local#task), taskDone(local#task)])
      }
      """, workers);
    var tuple = Assert.IsType<TupleHixValue>(value).Values;
    Assert.Equal(caller, Assert.IsType<NumberHixValue>(tuple[0]).Value);
    Assert.Equal(caller, Assert.IsType<NumberHixValue>(tuple[1]).Value);
    Assert.Equal(BooleanHixValue.True, tuple[2]);
  }

  [Fact]
  public void ExplicitPoolSpawnDoesNotMoveMainAndSingleWorkerRunsConcurrentChildren() {
    var caller = Thread.CurrentThread.ManagedThreadId;
    var value = Run("""
      func child(opaque channel) -> number {
        send($channel, workerId())
        return(receive($channel))
      }
      func main {
        local channel = channel()
        local task = spawn(child, @[local#channel], useThreadPool = true)
        local worker = receive(local#channel)
        send(local#channel, 42)
        return(@[workerId(), local#worker, awaitTask(local#task)])
      }
      """, 1);
    var tuple = Assert.IsType<TupleHixValue>(value).Values;
    Assert.Equal(caller, Assert.IsType<NumberHixValue>(tuple[0]).Value);
    Assert.NotEqual(caller, Assert.IsType<NumberHixValue>(tuple[1]).Value);
    Assert.Equal(42, Assert.IsType<NumberHixValue>(tuple[2]).Value);
  }

  [Fact]
  public void LambdasArgumentsAndAllResultsArePositionalAndOrdered() {
    var value = Run("""
      func main {
        local first = spawn(func => plus($0, 1), @[41])
        local second = spawn(func => uppercase($0), @[<hello>])
        return(awaitAll(@[local#first, local#second, local#first]))
      }
      """);
    var values = Assert.IsType<TupleHixValue>(value).Values;
    Assert.Equal(42, Assert.IsType<NumberHixValue>(values[0]).Value);
    Assert.Equal("HELLO", Assert.IsType<LiteralHixValue>(values[1]).Value.Resolve(null));
    Assert.Equal(values[0], values[2]);
  }

  [Fact]
  public void AnyReturnsFirstCompletionWithoutCancellingOtherTasks() {
    var value = Run("""
      func slow(opaque channel) -> number { receive($channel); return(7) }
      func fast { return(42) }
      func main {
        local channel = channel()
        local slow = spawn(slow, @[local#channel])
        local fast = spawn(fast)
        local first = awaitAny(@[local#slow, local#fast])
        send(local#channel, null)
        return(@[local#first#index, local#first#value, awaitTask(local#slow)])
      }
      """);
    Assert.Equal(new double[] {1, 42, 7}, Assert.IsType<TupleHixValue>(value).Values.Cast<NumberHixValue>().Select(number => number.Value));
  }

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public void BoundedChannelSuspendsProducersAndPreservesFifoAcrossWorkers(bool pool) {
    var value = Run("""
      func producer(opaque channel) -> null {
        send($channel, 1)
        send($channel, 2)
        closeChannel($channel)
      }
      func main {
        local channel = channel(1)
        local producer = spawn(producer, @[local#channel], useThreadPool = POOL)
        local first = receive(local#channel)
        local second = receive(local#channel)
        awaitTask(local#producer)
        return(@[local#first, local#second, [receive(local#channel)?], closeChannel(local#channel)])
      }
      """.Replace("POOL", pool ? "true" : "false"), pool ? 2 : 0);
    var values = Assert.IsType<TupleHixValue>(value).Values;
    Assert.Equal(new double[] {1, 2}, values.Take(2).Cast<NumberHixValue>().Select(number => number.Value));
    Assert.True(Assert.IsType<ErrorHixValue>(values[2]).IsChecked);
    Assert.Equal(BooleanHixValue.False, values[3]);
  }

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public void SemaphoreSuspendsWithoutBlockingWorker(bool pool) {
    var value = Run("""
      func first(opaque semaphore, opaque channel) -> number {
        acquire($semaphore)
        send($channel, 1)
        release($semaphore)
        return(1)
      }
      func second(opaque semaphore) -> number {
        acquire($semaphore)
        release($semaphore)
        return(2)
      }
      func main {
        local semaphore = semaphore()
        local channel = channel()
        local first = spawn(first, @[local#semaphore, local#channel], useThreadPool = POOL)
        local second = spawn(second, @[local#semaphore], useThreadPool = POOL)
        receive(local#channel)
        return(awaitAll(@[local#first, local#second]))
      }
      """.Replace("POOL", pool ? "true" : "false"), pool ? 1 : 0);
    Assert.Equal(new double[] {1, 2}, Assert.IsType<TupleHixValue>(value).Values.Cast<NumberHixValue>().Select(number => number.Value));
  }

  [Fact]
  public void AsyncEntrypointCanBePumpedOnCurrentWorker() {
    var backend = new HixStandaloneBackend(new StringWriter());
    var program = HixCompiler.CompileFunctions(["func child { yield(); return(42) } func main { return(awaitTask(spawn(child))) }"], backend);
    using var vm = new HixVM([program], 2);
    var completion = vm.InvokeAsync(program);
    Assert.False(completion.IsCompleted);
    var result = vm.CurrentThread.RunUntil(completion, 1);
    Assert.True(result.Success);
    Assert.Equal(42, Assert.IsType<NumberHixValue>(result.Value).Value);
  }

  [Theory]
  [InlineData("awaitAny(@[])", "at least one task")]
  [InlineData("sleep(-1)", "non-negative integer")]
  [InlineData("channel(0.5)", "non-negative integer")]
  [InlineData("semaphore(2, 1)", "maximum")]
  [InlineData("spawn(func => 1, useThreadPool = true)", "no background worker pool")]
  public void InvalidOperationsProduceErrors(string expression, string message) {
    var result = Invoke("func main { return(" + expression + ") }", 0);
    Assert.False(result.Success);
    Assert.Contains(message, result.Value.Render(null).Resolve(null));
  }

  [Fact]
  public void ChildErrorsPropagateAndEmptyAllCompletesImmediately() {
    Assert.Empty(Assert.IsType<TupleHixValue>(Run("func main { return(awaitAll(@[])) }")).Values);
    var result = Invoke("func child { return(cast(<child failed>, error)) } func main { return(awaitTask(spawn(child))) }", 0);
    Assert.False(result.Success);
    Assert.Equal("child failed", Assert.IsType<ErrorHixValue>(result.Value).Message.Resolve(result.Execution.Strings));
  }

  [Theory]
  [InlineData("send($channel, 1)")]
  [InlineData("receive($channel)")]
  public void ClosingChannelWakesParkedCoroutines(string operation) {
    var result = Invoke("""
      func waiting(opaque channel) -> any { return(OPERATION) }
      func main {
        local channel = channel()
        local waiting = spawn(waiting, @[local#channel])
        yield()
        closeChannel(local#channel)
        return(awaitTask(local#waiting))
      }
      """.Replace("OPERATION", operation), 0);
    Assert.False(result.Success);
    Assert.Equal("channel is closed", Assert.IsType<ErrorHixValue>(result.Value).Message.Resolve(result.Execution.Strings));
  }

  [Fact]
  public void AllWaitsForEveryChildBeforePropagatingFailure() {
    var value = Run("""
      func failing { return(cast(<failed>, error)) }
      func other(opaque channel) -> number { send($channel, 42); return(7) }
      func main {
        local channel = channel(1)
        local failed = spawn(failing)
        local other = spawn(other, @[local#channel])
        local failure = catch([awaitAll(@[local#failed, local#other])?])
        return(@[local#failure, taskDone(local#other), receive(local#channel)])
      }
      """);
    var values = Assert.IsType<TupleHixValue>(value).Values;
    Assert.IsType<NullHixValue>(values[0]);
    Assert.Equal(BooleanHixValue.True, values[1]);
    Assert.Equal(42, Assert.IsType<NumberHixValue>(values[2]).Value);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(1)]
  [InlineData(8)]
  public void SharedChannelTransfersEveryMessageAcrossPoolWorkers(int capacity) {
    const int count = 32;
    var submissions = string.Join("\n", Enumerable.Range(0, count).Select(index =>
      $"local t{index} = spawn(sendValue, @[local#channel, {index}], useThreadPool = true)"));
    var receives = string.Join(", ", Enumerable.Repeat("receive(local#channel)", count));
    var tasks = string.Join(", ", Enumerable.Range(0, count).Select(index => $"local#t{index}"));
    var value = Run($$"""
      func sendValue(opaque channel, number value) -> number { send($channel, $value); return($value) }
      func main {
        local channel = channel({{capacity}})
        {{submissions}}
        local received = @[{{receives}}]
        return(@[local#received, awaitAll(@[{{tasks}}])])
      }
      """, 4);
    var values = Assert.IsType<TupleHixValue>(value).Values;
    Assert.Equal(Enumerable.Range(0, count).Select(index => (double)index),
      Assert.IsType<TupleHixValue>(values[0]).Values.Cast<NumberHixValue>().Select(number => number.Value).OrderBy(number => number));
    Assert.Equal(Enumerable.Range(0, count).Select(index => (double)index),
      Assert.IsType<TupleHixValue>(values[1]).Values.Cast<NumberHixValue>().Select(number => number.Value));
  }

  [Fact]
  public void PendingWaitInsideNativeCallbackIsRejectedInsteadOfBlockingWorker() {
    var backend = new NativeCallbackBackend();
    var program = HixCompiler.CompileFunctions(["func sleeper { sleep(1) } func main { return(nativeCallback(sleeper)) }"], backend);
    using var vm = new HixVM([program]);
    var result = vm.Invoke(program, backend.CreateCoroutine());
    Assert.False(result.Success);
    Assert.Contains("synchronous host callback", Assert.IsType<ErrorHixValue>(result.Value).Message.Resolve(result.Execution.Strings));
  }

  private static IHixValue Run(string source, int workers = 0) {
    var result = Invoke(source, workers);
    Assert.True(result.Success, result.Value is ErrorHixValue error ? error.Message.Resolve(null) : null);
    return result.Value;
  }

  private static HixInvocationResult Invoke(string source, int workers) {
    var backend = new HixStandaloneBackend(new StringWriter());
    var program = HixCompiler.CompileFunctions([source], backend);
    using var vm = new HixVM([program], workers);
    return vm.Invoke(program, backend.CreateCoroutine());
  }

  private sealed class NativeCallbackBackend() : HixStandaloneBackend(new StringWriter()) {
    protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {
      base.RegisterFunctions(functions);
      functions.AddLibrary("pure func nativeCallback(function arg0) -> any external;");
      functions.Add(new SimpleFunction("nativeCallback", (thread, args) => thread.Callback(args[0], [], 0)));
    }
  }
}

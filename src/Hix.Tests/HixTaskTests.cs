using System;
using System.Threading;
using System.Threading.Tasks;
using Hix.Runtime;
using Xunit;

namespace Hix.Tests;

public class HixTaskTests {
  [Fact]
  public void CompletionPublishesOneResultAndPreservesFailureAndCancellation() {
    var task = new HixTask<int>();
    Assert.False(task.Wait(TimeSpan.Zero));
    Assert.True(task.TrySetResult(42));
    Assert.False(task.TrySetResult(7));
    Assert.True(task.IsCompletedSuccessfully);
    Assert.Equal(42, task.Result);

    var failed = new HixTask<int>();
    var error = new InvalidOperationException("failed");
    Assert.True(failed.TrySetException(error));
    Assert.True(failed.IsFaulted);
    Assert.Same(error, Assert.Throws<InvalidOperationException>(() => failed.GetResult()));

    var cancelled = new HixTask<int>();
    Assert.True(cancelled.TrySetCanceled());
    Assert.True(cancelled.IsCanceled);
    Assert.Throws<OperationCanceledException>(() => cancelled.GetResult());
  }

  [Fact]
  public void RegistrationRacingCompletionQueuesEveryContinuationExactlyOnce() {
    var worker = HixThread.Current;
    var owner = Environment.CurrentManagedThreadId;
    var task = new HixTask<int>();
    var awaiter = task.GetAwaiter();
    var callbacks = 0;
    Parallel.For(0, 512, index => {
      awaiter.OnCompleted(() => {
        Assert.Equal(owner, Environment.CurrentManagedThreadId);
        Interlocked.Increment(ref callbacks);
      });
      if (index == 128) task.TrySetResult(42);
    });
    Assert.Equal(0, callbacks);
    worker.RunUntilIdle();
    Assert.Equal(512, callbacks);
    Assert.Equal(42, task.Result);
  }

  [Fact]
  public void AwaitResumesThroughTheOwnerQueueRatherThanTheCompletingThread() {
    var worker = HixThread.Current;
    var owner = Environment.CurrentManagedThreadId;
    var task = new HixTask<int>();
    async Task<int> Observe() {
      var result = await task;
      Assert.Equal(owner, Environment.CurrentManagedThreadId);
      return result;
    }
    var observed = Observe();
    var producer = new Thread(() => task.TrySetResult(42));
    producer.Start();
    Assert.True(producer.Join(TimeSpan.FromSeconds(5)));
    Assert.False(observed.IsCompleted);
    worker.RunUntilIdle();
    Assert.Equal(42, observed.Result);
  }

  [Fact]
  public void DotNetAdaptersAreExplicitAndPreserveResultsAndErrors() {
    var task = new HixTask<int>();
    var adapted = task.AsTask();
    task.TrySetResult(42);
    Assert.Equal(42, adapted.Result);
    Assert.Equal(42, Task.FromResult(42).AsHixTask().Result);

    var error = new InvalidOperationException("adapter failure");
    var failed = Task.FromException<int>(error).AsHixTask();
    Assert.Same(error, Assert.Throws<InvalidOperationException>(() => failed.GetResult()));
    Assert.True(Task.FromCanceled<int>(new CancellationToken(true)).AsHixTask().IsCanceled);
  }
}

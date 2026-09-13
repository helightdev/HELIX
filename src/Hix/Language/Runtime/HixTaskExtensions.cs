using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hix.Runtime;

/// <summary>Explicit adapters for .NET hosts; not used by the Hix scheduler.</summary>
public static class HixTaskExtensions {
    public static Task<T> AsTask<T>(this HixTask<T> task) {
        if (task == null)
            throw new ArgumentNullException(nameof(task));
        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        task.OnCompleted(() => {
            if (task.IsCanceled) {
                completion.TrySetCanceled();
                return;
            }

            try {
                completion.TrySetResult(task.GetResult());
            } catch (Exception error) {
                completion.TrySetException(error);
            }
        });
        return completion.Task;
    }

    public static HixTask<T> AsHixTask<T>(this Task<T> task) {
        if (task == null)
            throw new ArgumentNullException(nameof(task));
        var completion = new HixTask<T>();
        task.ContinueWith(_ => {
            if (task.IsCanceled) {
                completion.TrySetCanceled();
                return;
            }

            try {
                completion.TrySetResult(task.GetAwaiter().GetResult());
            } catch (Exception error) {
                completion.TrySetException(error);
            }
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        return completion;
    }
}

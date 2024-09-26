using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Utils
{
    public static class ObservableExtensions
    {
        public static int Counter;
        public static Task<T> ToOnceTask<T>(this IObservable<T> observable, Action? runAfterSubscription = null, CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>(cancellationToken);
            var subscription = observable.Subscribe(x =>
            {
                Interlocked.Increment(ref Counter);
                tcs.TrySetResult(x);
            }, onCompleted: tcs.SetCanceled, onError: tcs.SetException);
            runAfterSubscription?.Invoke();
            return tcs.Task.ContinueWith(t =>
            {
                subscription.Dispose();
                return t;
            }, cancellationToken).Unwrap();
        }
    }
}

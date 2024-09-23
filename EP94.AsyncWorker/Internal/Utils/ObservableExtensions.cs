using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Utils
{
    public static class ObservableExtensions
    {
        public static Task<T> ToOnceTask<T>(this IObservable<T> observable, Action? runAfterSubscription = null, CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            var subscription = observable.Subscribe(x =>
            {
                tcs.TrySetResult(x);
            }, onCompleted: tcs.SetCanceled, onError: tcs.SetException);
            runAfterSubscription?.Invoke();
            return tcs.Task.WaitAsync(cancellationToken).ContinueWith(t =>
            {
                subscription.Dispose();
                return t;
            }, cancellationToken).Unwrap().WaitAsync(cancellationToken);
        }
    }
}

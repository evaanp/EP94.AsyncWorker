using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    public class DependOnCondition<T>(IWorkHandle<T> dependsOn, Predicate<T> condition) : IDependOnCondition
    {
        private IWorkHandle<T> _dependsOn = dependsOn;
        private Predicate<T> _condition = condition;
        public Task WaitForConditionAsync(CancellationToken cancellationToken)
        {
            if (_dependsOn is null)
            {
                return Task.CompletedTask;
            }
            TaskCompletionSource taskCompletionSource = new TaskCompletionSource(cancellationToken);
            IDisposable disposable = _dependsOn.Subscribe(x =>
            {
                if (_condition(x))
                {
                    taskCompletionSource.TrySetResult();
                }
            }, (e) => taskCompletionSource.TrySetException(e), () => taskCompletionSource.TrySetCanceled());
            return taskCompletionSource.Task
                .ContinueWith(x => disposable.Dispose());
        }
    }
}

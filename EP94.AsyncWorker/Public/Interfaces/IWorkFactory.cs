using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Internal.Models;
using EP94.AsyncWorker.Public.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Interfaces
{
    public interface IWorkFactory : IAsyncDisposable
    {
        ITrigger<TParam> CreateTriggerAsync<TParam>(bool triggerOnlyOnChanges = false, string? name = null);
        ITrigger<TParam> CreateTriggerAsync<TParam>(TParam? initialValue, bool triggerOnlyOnChanges = false, string? name = null);
        IBackgroundWork<TResult> CreateBackgroundWork<TResult>(FuncWorkDelegate<TResult> work, TimeSpan interval, Func<bool>? predicate = null, string? name = null, CancellationToken cancellationToken = default);
        IBackgroundWork CreateBackgroundWork(ActionWorkDelegate work, TimeSpan interval, Func<bool>? predicate = null, string? name = null, CancellationToken cancellationToken = default);
        IWorkHandle<TResult> CreateWork<TResult>(FuncWorkDelegate<TResult> work, ConfigureAction<TResult>? configureAction = null, string? name = null, CancellationToken cancellationToken = default);
        IWorkHandle CreateWork(ActionWorkDelegate work, ConfigureAction? configureAction = null, string? name = null, CancellationToken cancellationToken = default);
        internal IWorkHandle<TResult> CreateWork<TParam, TResult>(IUnitOfWork<TParam> previous, FuncWorkDelegate<TParam, TResult> work, ConfigureAction<TResult>? configureAction = null, string? name = null, CancellationToken cancellationToken = default);
        internal IWorkHandle CreateWork<TParam>(IUnitOfWork<TParam> previous, ActionWorkDelegate<TParam> work, ConfigureAction? configureAction = null, string? name = null, CancellationToken cancellationToken = default);

        public static IWorkFactory Create(int maxLevelOfConcurrency, TimeSpan? defaultTimeout = null, CancellationToken cancellationToken = default) => Create(maxLevelOfConcurrency, TaskScheduler.Current, defaultTimeout, cancellationToken);
        public static IWorkFactory Create(int maxLevelOfConcurrency, TaskScheduler taskScheduler, TimeSpan? defaultTimeout = null, CancellationToken cancellationToken = default) => new WorkFactory(maxLevelOfConcurrency, taskScheduler, defaultTimeout, cancellationToken);
    }
}

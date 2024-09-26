using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Public.Interfaces;
using EP94.AsyncWorker.Public.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    public class WorkFactory(int maxLevelOfConcurrency, TaskScheduler taskScheduler, TimeSpan? defaultTimeout, CancellationToken cancellationToken) : IWorkFactory
    {
        private AsyncWorkHandler _workHandler = new AsyncWorkHandler(maxLevelOfConcurrency, taskScheduler, defaultTimeout, cancellationToken);

        public IBackgroundWork<TResult> CreateBackgroundWork<TResult>(FuncWorkDelegate<TResult> work, TimeSpan interval, Func<bool>? predicate = null, string? name = null, CancellationToken cancellationToken = default) => new BackgroundWork<TResult>(WorkDelegate.Create(work), interval, predicate, _workHandler, this, name, cancellationToken);
        public IBackgroundWork CreateBackgroundWork(ActionWorkDelegate work, TimeSpan interval, Func<bool>? predicate = null, string? name = null, CancellationToken cancellationToken = default) => new BackgroundWork(WorkDelegate.Create(work), interval, predicate, _workHandler, this, name, cancellationToken);

        public ITrigger<TParam> CreateTriggerAsync<TParam>(bool triggerOnlyOnChanges = false, string? name = null) => new Trigger<TParam>(_workHandler, this, name, triggerOnlyOnChanges, cancellationToken);
        public ITrigger<TParam> CreateTriggerAsync<TParam>(TParam? initialValue, bool triggerOnlyOnChanges = false, string? name = null) => new Trigger<TParam>(_workHandler, this, initialValue, name, triggerOnlyOnChanges, cancellationToken);
        public IWorkHandle<TResult> CreateWork<TResult>(FuncWorkDelegate<TResult> work, ConfigureAction<TResult>? configureAction, string? name = null, CancellationToken cancellationToken = default) => Configure(new UnitOfWork<TResult?>(WorkDelegate.Create(work), null, _workHandler, this, name, cancellationToken), configureAction);
        public IWorkHandle CreateWork(ActionWorkDelegate work, ConfigureAction? configureAction, string? name = null, CancellationToken cancellationToken = default) => ConfigureVoid(new UnitOfWork<Empty, Empty>(WorkDelegate.Create(work), null, _workHandler, this, name, cancellationToken), configureAction);
        public IWorkHandle<TResult> CreateWork<TParam, TResult>(IUnitOfWork<TParam> previous, FuncWorkDelegate<TParam, TResult> work, ConfigureAction<TResult>? configureAction, string? name = null, CancellationToken cancellationToken = default) => Configure(new UnitOfWork<TResult?>(WorkDelegate.Create(work), previous, _workHandler, this, name, cancellationToken), configureAction);
        public IWorkHandle CreateWork<TParam>(IUnitOfWork<TParam> previous, ActionWorkDelegate<TParam> work, ConfigureAction? configureAction, string? name = null, CancellationToken cancellationToken = default) => ConfigureVoid(new UnitOfWork<Empty>(WorkDelegate.Create(work), previous, _workHandler, this, name, cancellationToken), configureAction);
        public ValueTask DisposeAsync() => _workHandler.DisposeAsync();

        private IWorkHandle<T> Configure<T>(IWorkOptions<T?> t, ConfigureAction<T>? configureAction)
        {
            configureAction?.Invoke(t);
            return (IWorkHandle<T>)t;
        }

        private T ConfigureVoid<T>(T t, ConfigureAction? configureAction) where T : IWorkOptions
        {
            configureAction?.Invoke(t);
            return t;
        }
    }
}

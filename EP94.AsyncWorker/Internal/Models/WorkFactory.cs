using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Public.Interfaces;
using EP94.AsyncWorker.Public.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    public class WorkFactory(int maxLevelOfConcurrency, TaskScheduler taskScheduler, TimeSpan? defaultTimeout, CancellationToken cancellationToken) : IWorkFactory
    {
        private AsyncWorkHandler _workHandler = new AsyncWorkHandler(maxLevelOfConcurrency, taskScheduler, defaultTimeout, cancellationToken);

        public IBackgroundWork<TResult> CreateBackgroundWork<TResult>(FuncWorkDelegate<TResult> work, TimeSpan interval, Func<bool>? predicate = null, CancellationToken cancellationToken = default) => new BackgroundWork<TResult>(WorkDelegate.Create(work), interval, predicate, _workHandler, this, cancellationToken);
        public IBackgroundWork CreateBackgroundWork(ActionWorkDelegate work, TimeSpan interval, Func<bool>? predicate = null, CancellationToken cancellationToken = default) => new BackgroundWork(WorkDelegate.Create(work), interval, predicate, _workHandler, this, cancellationToken);
        public IFuncWorkHandle<T> CreateTimebasedTrigger<T>(FuncWorkDelegate<T> work, IResultWorkHandle<DateTimeOffset> dueTimeWorkHandle, IResultWorkHandle<DateTimeOffset> nextRunWorkHandle, CancellationToken cancellationToken = default) => new TimeBasedTrigger<T>(WorkDelegate.Create(work), dueTimeWorkHandle, nextRunWorkHandle, _workHandler, this, cancellationToken);
        public IFuncWorkHandle<T> CreateTimebasedTrigger<T>(Func<T> work, IResultWorkHandle<DateTimeOffset> dueTimeWorkHandle, IResultWorkHandle<DateTimeOffset> nextRunWorkHandle, CancellationToken cancellationToken = default) => new TimeBasedTrigger<T>(WorkDelegate.Create(work), dueTimeWorkHandle, nextRunWorkHandle, _workHandler, this, cancellationToken);
        public ITrigger<TParam> CreateTrigger<TParam>(bool triggerOnlyOnChanges = false, IEqualityComparer<TParam>? equalityComparer = null) => new Trigger<TParam>(_workHandler, this, triggerOnlyOnChanges, equalityComparer ?? EqualityComparer<TParam>.Default, cancellationToken);
        public ITrigger<TParam> CreateTrigger<TParam>(TParam? initialValue, bool triggerOnlyOnChanges = false, IEqualityComparer<TParam>? equalityComparer = null) => new Trigger<TParam>(_workHandler, this, initialValue, triggerOnlyOnChanges, equalityComparer ?? EqualityComparer<TParam>.Default, cancellationToken);
        public IFuncWorkHandle<TResult> CreateWork<TResult>(FuncWorkDelegate<TResult> work, CancellationToken cancellationToken = default) => new FuncUnitOfWork<TResult>(WorkDelegate.Create(work), _workHandler, this, cancellationToken);
        public IFuncWorkHandle<TResult> CreateWork<TResult>(Func<TResult> work, CancellationToken cancellationToken = default) => new FuncUnitOfWork<TResult>(WorkDelegate.Create(work), _workHandler, this, cancellationToken);
        public IFuncWorkHandle<TParam, TResult> CreateWork<TParam, TResult>(FuncWorkDelegate<TParam, TResult> work, CancellationToken cancellationToken = default) => new FuncUnitOfWork<TParam, TResult>(WorkDelegate.Create(work), _workHandler, this, cancellationToken);
        public IFuncWorkHandle<TParam, TResult> CreateWork<TParam, TResult>(Func<TParam, TResult> work, CancellationToken cancellationToken = default) => new FuncUnitOfWork<TParam, TResult>(WorkDelegate.Create(work), _workHandler, this, cancellationToken);
        public IActionWorkHandle CreateWork(ActionWorkDelegate work, CancellationToken cancellationToken = default) => new ActionUnitOfWork(WorkDelegate.Create(work), _workHandler, this, cancellationToken);
        public IActionWorkHandle CreateWork(Action work, CancellationToken cancellationToken = default) => new ActionUnitOfWork(WorkDelegate.Create(work), _workHandler, this, cancellationToken);
        public IActionWorkHandle<TParam> CreateWork<TParam>(ActionWorkDelegate<TParam> work, CancellationToken cancellationToken = default) => new ActionUnitOfWork<TParam>(WorkDelegate.Create(work), _workHandler, this, cancellationToken);
        public IActionWorkHandle<TParam> CreateWork<TParam>(Action<TParam> work, CancellationToken cancellationToken = default) => new ActionUnitOfWork<TParam>(WorkDelegate.Create(work), _workHandler, this, cancellationToken);
        public ValueTask DisposeAsync() => _workHandler.DisposeAsync();
    }
}

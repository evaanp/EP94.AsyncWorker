﻿using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Internal.Models;
using EP94.AsyncWorker.Public.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Interfaces
{
    public interface IWorkFactory : IAsyncDisposable
    {
        IFuncWorkHandle<T> CreateTimebasedTrigger<T>(FuncWorkDelegate<T> work, IResultWorkHandle<DateTimeOffset> dueTimeWorkHandle, IResultWorkHandle<DateTimeOffset> nextRunWorkHandle, CancellationToken cancellationToken = default);
        IFuncWorkHandle<T> CreateTimebasedTrigger<T>(Func<T> work, IResultWorkHandle<DateTimeOffset> dueTimeWorkHandle, IResultWorkHandle<DateTimeOffset> nextRunWorkHandle, CancellationToken cancellationToken = default);
        ITrigger<TParam> CreateTrigger<TParam>(bool triggerOnlyOnChanges = false, IEqualityComparer<TParam>? equalityComparer = null);
        ITrigger<TParam> CreateTrigger<TParam>(TParam? initialValue, bool triggerOnlyOnChanges = false, IEqualityComparer<TParam>? equalityComparer = null);
        IBackgroundWork<TResult> CreateBackgroundWork<TResult>(FuncWorkDelegate<TResult> work, TimeSpan interval, Func<bool>? predicate = null, CancellationToken cancellationToken = default);
        IBackgroundWork CreateBackgroundWork(ActionWorkDelegate work, TimeSpan interval, Func<bool>? predicate = null, CancellationToken cancellationToken = default);
        IFuncWorkHandle<TResult> CreateWork<TResult>(FuncWorkDelegate<TResult> work, CancellationToken cancellationToken = default);
        IFuncWorkHandle<TResult> CreateWork<TResult>(Func<TResult> work, CancellationToken cancellationToken = default);
        IActionWorkHandle CreateWork(ActionWorkDelegate work, CancellationToken cancellationToken = default);
        IActionWorkHandle CreateWork(Action work, CancellationToken cancellationToken = default);
        IFuncWorkHandle<TParam, TResult> CreateWork<TParam, TResult>(FuncWorkDelegate<TParam, TResult> work, CancellationToken cancellationToken = default);
        IFuncWorkHandle<TParam, TResult> CreateWork<TParam, TResult>(Func<TParam, TResult> work, CancellationToken cancellationToken = default);
        IActionWorkHandle<TParam> CreateWork<TParam>(ActionWorkDelegate<TParam> work, CancellationToken cancellationToken = default);
        IActionWorkHandle<TParam> CreateWork<TParam>(Action<TParam> work, CancellationToken cancellationToken = default);

        public static IWorkFactory Create(int maxLevelOfConcurrency, TimeSpan? defaultTimeout = null, CancellationToken cancellationToken = default) => Create(maxLevelOfConcurrency, TaskScheduler.Current, defaultTimeout, cancellationToken);
        public static IWorkFactory Create(int maxLevelOfConcurrency, TaskScheduler taskScheduler, TimeSpan? defaultTimeout = null, CancellationToken cancellationToken = default) => new WorkFactory(maxLevelOfConcurrency, taskScheduler, defaultTimeout, cancellationToken);

        public static IWorkFactory Default { get; set; } = Create(int.MaxValue);
    }
}

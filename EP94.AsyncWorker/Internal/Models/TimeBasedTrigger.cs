using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    internal class TimeBasedTrigger<T> : WorkBase<Unit, T>, IFuncWorkHandle<T>
    {
        protected override IObservable<T> RunObservable => _subject;
        private ReplaySubject<T> _subject = new ReplaySubject<T>(1);

        private Func<DateTime> _getNextTimeFunc;
        private IWorkDelegate _task;

        public TimeBasedTrigger(IFuncWorkDelegate<T> task, DateTime firstRun, Func<DateTime> getNextTimeFunc, IWorkScheduler workScheduler, IWorkFactory workFactory, string? name, CancellationToken cancellationToken)
            : base(workScheduler, workFactory, name, cancellationToken)
        {
            _task = task;
            _getNextTimeFunc = getNextTimeFunc;
            workScheduler.ScheduleWork(new ExecuteWorkItem<Unit, T>(this, Unit.Default), firstRun);
        }
        protected override ISubject<IObservable<Unit>> GetParameterSubject()
        {
            return new Subject<IObservable<Unit>>();
        }

        protected override async Task DoExecuteAsync(ExecuteWorkItem<Unit, T> executeWorkItem)
        {
            await SafeExecuteAsync<T>(_task,
                onSuccess: result => {
                    _subject.OnNext(result);
                    executeWorkItem.ResultSubject.OnNext(result);
                    executeWorkItem.ResultSubject.OnCompleted();
                },
                onCanceled: () =>
                {
                    _subject.OnCompleted();
                    executeWorkItem.ResultSubject.OnCompleted();
                },
                onFail: (e) =>
                {
                    _subject.OnError(e);
                    executeWorkItem.ResultSubject.OnError(e);
                }, null, null);
            WorkScheduler.ScheduleWork(executeWorkItem, _getNextTimeFunc());
        }
    }
}

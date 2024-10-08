using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using EP94.AsyncWorker.Internal.Models;
using System.Reactive.Linq;
using System.Reactive;

namespace EP94.AsyncWorker.Internal.Models
{
    internal class BackgroundWork<T> : WorkBase<Unit, T>, IBackgroundWork<T>
    {
        private IWorkDelegate _task;
        private TimeSpan _interval;
        private Func<bool>? _predicate;

        protected override IObservable<T> RunObservable => _subject;
        private ISubject<T> _subject = new Subject<T>();

        public BackgroundWork(IWorkDelegate task, TimeSpan interval, Func<bool>? predicate, IWorkScheduler workScheduler, IWorkFactory workFactory, string? name, CancellationToken cancellationToken) : base(workScheduler, workFactory, name, cancellationToken)
        {
            _task = task;
            _interval = interval;
            _predicate = predicate;
            //Observable.Timer(TimeSpan.MinValue, interval).Do(x =>
            //{
            //    Console.WriteLine("hoi");
            //    ParameterSubject.OnNext(Observable.Return(Unit.Default));
            //}).Subscribe(CancellationToken);
            workScheduler.ScheduleWork(new ExecuteWorkItem<Unit, T>(this, Unit.Default), null);
        }

        protected override async Task DoExecuteAsync(ExecuteWorkItem<Unit, T> executeWorkItem)
        {
            if (_predicate?.Invoke() ?? true)
            {
                await SafeExecuteAsync<T>(_task,
                    onSuccess: result => {
                        _subject.OnNext(result);
                        executeWorkItem.ResultSubject.OnNext(result);
                        executeWorkItem.ResultSubject.OnCompleted();
                    },
                    onCanceled: () => {
                        _subject.OnCompleted();
                        executeWorkItem.ResultSubject.OnCompleted();
                    },
                    onFail: (e) => {
                        _subject.OnError(e);
                        executeWorkItem.ResultSubject.OnError(e);
                    }, null, null);
            }
            WorkScheduler.ScheduleWork(executeWorkItem, DateTime.UtcNow.Add(_interval));
        }

        protected override ISubject<IObservable<Unit>> GetParameterSubject()
        {
            return new Subject<IObservable<Unit>>();
        }
    }

    internal class BackgroundWork(IWorkDelegate task, TimeSpan interval, Func<bool>? predicate, IWorkScheduler workScheduler, IWorkFactory workFactory, string? name, CancellationToken cancellationToken) : BackgroundWork<Unit>(task, interval, predicate, workScheduler, workFactory, name, cancellationToken), IBackgroundWork
    {

    }
}

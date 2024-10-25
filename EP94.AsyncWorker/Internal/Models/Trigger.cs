using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Internal.Utils;
using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    internal class Trigger<T> : WorkBase<Unit, T>, ITrigger<T>
    {
        protected override IObservable<T> RunObservable => _subject.Retry();

        private ReplaySubject<T> _subject = new ReplaySubject<T>(1);
        private bool _triggerOnlyOnChanges;
        private T? _lastValue;
        private IEqualityComparer<T> _comparer;

        public Trigger(IWorkScheduler workScheduler, IWorkFactory workFactory, string? name, bool triggerOnlyOnChanges, IEqualityComparer<T> equalityComparer, CancellationToken cancellationToken) : base(workScheduler, workFactory, name, cancellationToken)
        {
            _triggerOnlyOnChanges = triggerOnlyOnChanges;
            _subject.Subscribe(x => _lastValue = x);
            _comparer = equalityComparer;
        }

        public Trigger(IWorkScheduler workScheduler, IWorkFactory workFactory, T? initialValue, string? name, bool triggerOnlyOnChanges, IEqualityComparer<T> equalityComparer, CancellationToken cancellationToken) : this(workScheduler, workFactory, name, triggerOnlyOnChanges, equalityComparer, cancellationToken)
        { 
            if (initialValue is not null)
            {
                _subject.OnNext(initialValue);
            }
        }

        public void OnNext(T param)
        {
            if (!_triggerOnlyOnChanges || !_comparer.Equals(param, _lastValue))
            {
                _lastValue = param;
                _subject.OnNext(param);
                //ParameterSubject.OnNext(Observable.Return(Unit.Default));
                //WorkScheduler.ScheduleWork(new ExecuteWorkItem<Unit, T>(this, Unit.Default), null);
            }
        }

        protected override Task DoExecuteAsync(ExecuteWorkItem<Unit, T> executeWorkItem, CancellationToken cancellationToken)
        {
            //if (!_triggerOnlyOnChanges || !_comparer.Equals(executeWorkItem.Parameter, _lastValue))
            //{
            //    _subject.OnNext(executeWorkItem.Parameter);
            //}
            executeWorkItem.ResultSubject.OnNext(_lastValue);
            return Task.CompletedTask;
        }

        //public override IDisposable Subscribe(IObserver<T> observer) => _subject.Subscribe(observer);

        protected override ISubject<IObservable<Unit>> GetParameterSubject()
        {
            return new ReplaySubject<IObservable<Unit>>(1);
        }
    }
}

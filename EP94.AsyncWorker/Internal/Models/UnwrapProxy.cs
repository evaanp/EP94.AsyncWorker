using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Internal.Utils;
using EP94.AsyncWorker.Public.Interfaces;
using EP94.AsyncWorker.Public.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    internal class UnwrapProxy<TFirstResult, TNextResult> : WorkBase<IResultWorkHandle<TFirstResult>, TNextResult>, IResultWorkHandle<TNextResult>
    {
        protected override IObservable<TNextResult> RunObservable { get; }

        public UnwrapProxy(IResultWorkHandle<TFirstResult> first, IResultWorkHandle<TNextResult> next, IWorkScheduler workScheduler, IWorkFactory workFactory, CancellationToken cancellationToken) : base(workScheduler, workFactory, $"UnwrapProxy", cancellationToken)
        {
            RunObservable = ParameterSubject
                .Select(x => x.Switch())
                .Switch()
                .Select(x => next)
                .Switch();

            ParameterSubject.OnNext(Observable.Return(first));
        }

        protected override Task DoExecuteAsync(ExecuteWorkItem<IResultWorkHandle<TFirstResult>, TNextResult> executeWorkItem)
        {
            Console.WriteLine();
            //if (executionStack.LastResult is TWrapped wrapped && wrapped is IWorkHandle<T> workHandle)
            //{
            //    lock (_locker)
            //    {
            //        _innerWorkHandle = workHandle;
            //        foreach (SubscriptionProxy subscriptionProxy in _subscriptionProxies)
            //        {
            //            if (!subscriptionProxy.IsDisposed)
            //            {
            //                subscriptionProxy.SubscriptionDisposable = _innerWorkHandle.Subscribe(subscriptionProxy.Observer.OnNext, onCompleted: subscriptionProxy.Observer.OnCompleted, onError: subscriptionProxy.Observer.OnError);
            //            }
            //        }
            //        workHandle.Then(this);
            //        if (_isStarted)
            //        {
            //            ((IUnitOfWork)workHandle).NotifyStart();
            //        }
            //    }
            //}
            //else
            //{
            //    ScheduleNext(executionStack);
            //}

            return Task.CompletedTask;
        }

        protected override ISubject<IObservable<IResultWorkHandle<TFirstResult>>> GetParameterSubject() => new ReplaySubject<IObservable<IResultWorkHandle<TFirstResult>>>(1);

        //public override void NotifyStart()
        //{
        //    lock (_locker) 
        //    {
        //        _isStarted = true;
        //        Previous!.NotifyStart();
        //    }
        //}

        //public override IDisposable Subscribe(IObserver<T> observer)
        //{
        //    IDisposable result;
        //    lock (_locker)
        //    {
        //        if (_innerWorkHandle is null)
        //        {
        //            SubscriptionProxy subscriptionProxy = new SubscriptionProxy(observer);
        //            _subscriptionProxies.Add(subscriptionProxy);
        //            result = subscriptionProxy;
        //        }
        //        else
        //        {
        //            result = _innerWorkHandle.Subscribe(observer);
        //        }
        //    }
        //    NotifyStart();
        //    return result;
        //}

        //public override ISubject<T1> CreateSubject<T1>() => new ReplaySubject<T1>(1);

        //protected override void DoSetCanceled()
        //{

        //}

        //private class SubscriptionProxy(IObserver<T> observer) : IDisposable
        //{
        //    public IObserver<T> Observer = observer;
        //    public bool IsDisposed
        //    {
        //        get
        //        {
        //            lock (this)
        //            {
        //                return _isDisposed;
        //            }
        //        }
        //    }
        //    private bool _isDisposed;
        //    public IDisposable? SubscriptionDisposable { get; set; }
        //    public void Dispose()
        //    {
        //        lock (this)
        //        {
        //            _isDisposed = true;
        //        }
        //        SubscriptionDisposable?.Dispose();
        //    }
        //}
    }
}

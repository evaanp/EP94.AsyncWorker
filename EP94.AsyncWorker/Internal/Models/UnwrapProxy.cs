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
    internal class UnwrapProxy<TWrapped, T>(IUnitOfWork previous, IWorkScheduler workScheduler, IWorkFactory workFactory, CancellationToken cancellationToken) : WorkBase<T>(previous, workScheduler, workFactory, $"UnwrapProxy_{previous.Name}", cancellationToken), IWorkHandle<T?>
    {
        private object _locker = new object();
        private List<SubscriptionProxy> _subscriptionProxies = new List<SubscriptionProxy>();
        private IWorkHandle<T>? _innerWorkHandle;
        private bool _isStarted;

        protected override Task DoExecuteAsync(ExecutionStack executionStack)
        {
            if (executionStack.LastResult is TWrapped wrapped && wrapped is IWorkHandle<T> workHandle)
            {
                lock (_locker)
                {
                    _innerWorkHandle = workHandle;
                    foreach (SubscriptionProxy subscriptionProxy in _subscriptionProxies)
                    {
                        if (!subscriptionProxy.IsDisposed)
                        {
                            subscriptionProxy.SubscriptionDisposable = _innerWorkHandle.Subscribe(subscriptionProxy.Observer.OnNext, onCompleted: subscriptionProxy.Observer.OnCompleted, onError: subscriptionProxy.Observer.OnError);
                        }
                    }
                    workHandle.Then(this);
                    if (_isStarted)
                    {
                        ((IUnitOfWork)workHandle).NotifyStart();
                    }
                }
            }
            else
            {
                ScheduleNext(executionStack);
            }

            return Task.CompletedTask;
        }

        public override void NotifyStart()
        {
            lock (_locker) 
            {
                _isStarted = true;
                Previous!.NotifyStart();
            }
        }

        public override IDisposable Subscribe(IObserver<T> observer)
        {
            IDisposable result;
            lock (_locker)
            {
                if (_innerWorkHandle is null)
                {
                    SubscriptionProxy subscriptionProxy = new SubscriptionProxy(observer);
                    _subscriptionProxies.Add(subscriptionProxy);
                    result = subscriptionProxy;
                }
                else
                {
                    result = _innerWorkHandle.Subscribe(observer);
                }
            }
            NotifyStart();
            return result;
        }

        public override ISubject<T1> CreateSubject<T1>() => new ReplaySubject<T1>(1);

        private class SubscriptionProxy(IObserver<T> observer) : IDisposable
        {
            public IObserver<T> Observer = observer;
            public bool IsDisposed
            {
                get
                {
                    lock (this)
                    {
                        return _isDisposed;
                    }
                }
            }
            private bool _isDisposed;
            public IDisposable? SubscriptionDisposable { get; set; }
            public void Dispose()
            {
                lock (this)
                {
                    _isDisposed = true;
                }
                SubscriptionDisposable?.Dispose();
            }
        }
    }
}

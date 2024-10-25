using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Internal.Models;
using EP94.AsyncWorker.Public.Interfaces;
using EP94.AsyncWorker.Public.Models;
using System;
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

namespace EP94.AsyncWorker.Internal.Utils
{
    public static class ObservableExtensions
    {
        public static async Task<T> ToOnceTask<T>(this IObservable<T> observable, Action? runAfterSubscription = null, CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>(cancellationToken);
            //Subject<T> resultSubject = new Subject<T>();
            observable.SubscribeOnce(Observer.Create<T>(tcs.SetResult, e =>
            {
                tcs.TrySetException(e);
            }, () => {
                tcs.TrySetCanceled();
            }), cancellationToken);
            //IDisposable disposable = observable.TakeUntil(resultSubject).Subscribe(resultSubject);
            runAfterSubscription?.Invoke();
            return await tcs.Task.ConfigureAwait(false);

            //TaskCompletionSource<T> tcs = new TaskCompletionSource<T>(cancellationToken);
            //observable.SubscribeOnce(Observer.Create<T>(tcs.SetResult, tcs.SetException, tcs.SetCanceled), cancellationToken);
            //return tcs.Task;
        }

        public static IFuncWorkHandle<TNextResult> ThenDo<TNextResult>(this IObservable<Unit> observable, FuncWorkDelegate<TNextResult> next, IWorkFactory? workFactory = null)
            => observable.ConnectTo(GetOrDefault(workFactory).CreateWork(next));

        public static IFuncWorkHandle<TNextResult> ThenDo<TNextResult>(this IObservable<Unit> observable, Func<TNextResult> next, IWorkFactory? workFactory = null)
            => observable.ConnectTo(GetOrDefault(workFactory).CreateWork(_ => Task.FromResult(next())));

        public static IFuncWorkHandle<TNextParam, TNextResult> ThenDo<TNextParam, TNextResult>(this IObservable<TNextParam> observable, FuncWorkDelegate<TNextParam, TNextResult> next, IWorkFactory? workFactory = null)
            => observable.ConnectTo(GetOrDefault(workFactory).CreateWork(next));

        public static IFuncWorkHandle<TNextParam, TNextResult> ThenDo<TNextParam, TNextResult>(this IObservable<TNextParam> observable, Func<TNextParam, TNextResult> next, IWorkFactory? workFactory = null)
            => observable.ConnectTo(GetOrDefault(workFactory).CreateWork<TNextParam, TNextResult>((param, c) => Task.FromResult(next(param))));

        public static IActionWorkHandle ThenDo(this IObservable<Unit> observable, ActionWorkDelegate next, IWorkFactory? workFactory = null)
            => observable.ConnectTo(GetOrDefault(workFactory).CreateWork(next));

        public static IActionWorkHandle ThenDo(this IObservable<Unit> observable, Action next, IWorkFactory? workFactory = null)
            => observable.ConnectTo(GetOrDefault(workFactory).CreateWork(_ =>
            {
                next();
                return Task.CompletedTask;
            }));

        public static IActionWorkHandle<TParameter> ThenDo<TParameter>(this IObservable<TParameter> observable, ActionWorkDelegate<TParameter> next, IWorkFactory? workFactory = null) 
            => observable.ConnectTo(GetOrDefault(workFactory).CreateWork(next));

        public static IActionWorkHandle<TParameter> ThenDo<TParameter>(this IObservable<TParameter> observable, Action<TParameter> next, IWorkFactory? workFactory = null)
            => observable.ConnectTo(GetOrDefault(workFactory).CreateWork<TParameter>((param, _) =>
            {
                next(param);
                return Task.CompletedTask;
            }));

        public static void SubscribeOnce<TResult>(this IObservable<TResult> observable, IObserver<TResult> observer, CancellationToken cancellationToken)
        {
            //CancellationTokenSource cancelSubscriptionSource = new CancellationTokenSource();
            //CancellationTokenSource linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancelSubscriptionSource.Token);
            Subject<TResult> resultSubject = new Subject<TResult>();
            IDisposable? disposable;
            resultSubject.Subscribe(observer.OnNext, observer.OnError, observer.OnCompleted);
            disposable = observable.TakeUntil(resultSubject).Subscribe(resultSubject);
            cancellationToken.Register(observer.OnCompleted, false);

            //void CancelTokens()
            //{
            //    linkedToken.Cancel();
            //    cancelSubscriptionSource.Cancel();
            //    linkedToken.Dispose();
            //    cancelSubscriptionSource.Dispose();
            //}
        }

        public static void SubscribeOnce<TResult>(this IObservable<TResult> observable, CancellationToken cancellationToken) => observable.SubscribeOnce(Observer.Create<TResult>((_) => { }), cancellationToken);

        //public static IObservable<TInnerResult> InvokeAsync<TInnerResult>(this IObservable<Unit> observable, IResultWorkHandle<TInnerResult> invokeWorkHandle)
        //{
        //    IFuncWorkHandle<IObservable<TInnerResult>> toInvoke = observable
        //        .ThenDo(c => Task.FromResult(invokeWorkHandle.AsObservable()));

        //    return new UnwrapProxy2<TInnerResult>(toInvoke);
        //}

        public static IResultWorkHandle<TInnerResult> InvokeAsync<TObservable, TInnerResult>(this IObservable<TObservable> observable, IObservable<TInnerResult> innerObservable, CancellationToken cancellationToken = default)
        {
            IFuncWorkHandle<IObservable<TInnerResult>> toInvoke = observable
                .AsUnitObservable()
                .ThenDo(() => innerObservable.AsObservable());

            return new UnwrapProxy2<TInnerResult>(toInvoke, cancellationToken);
        }

        public static IObservable<Unit> AsUnitObservable<TCurrent>(this IObservable<TCurrent> observable) => observable.Select(_ => Unit.Default);

        private static IWorkFactory GetOrDefault(IWorkFactory? workFactory) => workFactory ?? IWorkFactory.Default;
        //public static IObservable<TResult> ConnectTo<TParam, TResult>(this IObservable<TParam> observable, IParameterWorkHandle<TParam, TResult> workHandle)
        //{
        //    return workHandle.ConnectTo(observable);
        //}

        //public static Task<TResult> InvokeAsync<TParam, TResult>(this IObservable<TParam> observable, IFuncWorkHandle<TParam, TResult> workHandle, CancellationToken cancellationToken = default)
        //{
        //    return workHandle.ToOnceTask(null, cancellationToken);
        //}

        internal static TNextWorkHandle ConnectTo<TResult, TNextWorkHandle>(this IObservable<TResult> observable, TNextWorkHandle nextWorkHandle) where TNextWorkHandle : IConnectableWorkHandle<TResult>
        {
            nextWorkHandle.SetSourceObservable(observable);
            return nextWorkHandle;
        }

        //public static TNextWorkHandle 

        private static TWorkHandle AddStackTrace<TWorkHandle>(TWorkHandle workHandle) where TWorkHandle : IWorkOptions
        {
            var aa = Environment.StackTrace;
            return workHandle;
        }
    }
}

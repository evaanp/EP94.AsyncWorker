using EP94.AsyncWorker.Internal.Models;
using EP94.AsyncWorker.Public.Interfaces;
using EP94.AsyncWorker.Public.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Utils
{
    public static class ObservableExtensions
    {
        public static Task<T> ToOnceTask<T>(this IObservable<T> observable, Action? runAfterSubscription = null, CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>(cancellationToken);
            var subscription = observable.Subscribe(tcs.SetResult, e => tcs.TrySetException([e]), () => tcs.TrySetCanceled());
            runAfterSubscription?.Invoke();
            return tcs.Task.ContinueWith(t =>
            {
                subscription.Dispose();
                return t;
            }, cancellationToken).Unwrap();
        }

        public static IFuncWorkHandle<TNextResult> ThenDo<TNextResult>(this IObservable<Unit> observable, FuncWorkDelegate<TNextResult> next, IWorkFactory? workFactory = null)
            => observable.ConnectTo(GetOrDefault(workFactory).CreateWork(next));

        public static IFuncWorkHandle<TNextParam, TNextResult> ThenDo<TNextParam, TNextResult>(this IObservable<TNextParam> observable, FuncWorkDelegate<TNextParam, TNextResult> next, IWorkFactory? workFactory = null)
            => observable.ConnectTo(GetOrDefault(workFactory).CreateWork(next));

        public static IActionWorkHandle ThenDo(this IObservable<Unit> observable, ActionWorkDelegate next, IWorkFactory? workFactory = null)
            => observable.ConnectTo(GetOrDefault(workFactory).CreateWork(next));

        public static IActionWorkHandle<TParameter> ThenDo<TParameter>(this IObservable<TParameter> observable, ActionWorkDelegate<TParameter> next, IWorkFactory? workFactory = null) 
            => observable.ConnectTo(GetOrDefault(workFactory).CreateWork(next));

        //public static IObservable<TInnerResult> InvokeAsync<TInnerResult>(this IObservable<Unit> observable, IResultWorkHandle<TInnerResult> invokeWorkHandle)
        //{
        //    IFuncWorkHandle<IObservable<TInnerResult>> toInvoke = observable
        //        .ThenDo(c => Task.FromResult(invokeWorkHandle.AsObservable()));

        //    return new UnwrapProxy2<TInnerResult>(toInvoke);
        //}

        public static IResultWorkHandle<TInnerResult> InvokeAsync<TObservable, TInnerResult>(this IObservable<TObservable> observable, IResultWorkHandle<TInnerResult> invokeWorkHandle, CancellationToken cancellationToken = default)
        {
            IFuncWorkHandle<IObservable<TInnerResult>> toInvoke = observable
                .AsUnitObservable()
                .ThenDo(c => Task.FromResult(invokeWorkHandle.AsObservable()));

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

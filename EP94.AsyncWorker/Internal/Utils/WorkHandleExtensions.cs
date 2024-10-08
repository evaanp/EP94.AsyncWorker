using EP94.AsyncWorker.Internal.Models;
using EP94.AsyncWorker.Public.Interfaces;
using EP94.AsyncWorker.Public.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Utils
{
    public static class WorkHandleExtensions
    {
        /// <summary>
        /// Specifies the way the work handles the latest result
        /// </summary>
        public static TWorkHandle ConfigureRetainResult<TWorkHandle>(this TWorkHandle workHandle, RetainResult retainResult) where TWorkHandle : IWorkOptions
        {
            workHandle.RetainResult = retainResult;
            return workHandle;
        }

        /// <summary>
        /// Configures debounce time. When the task with the same hashcode gets scheduled again within the debounce time of the previous, the previous task gets canceled.
        /// </summary>
        /// <param name="hashCode"></param>
        /// <param name="debounceTime"></param>
        public static TWorkHandle ConfigureDebounce<TWorkHandle>(this TWorkHandle workHandle, int hashCode, TimeSpan debounceTime) where TWorkHandle : IWorkOptions
        {
            workHandle.ConfigureDebounce(hashCode, debounceTime);
            return workHandle;
        }

        /// <summary>
        /// The number of retries before the task is considered as failed
        /// </summary>
        public static TWorkHandle ConfigureRetry<TWorkHandle>(this TWorkHandle workHandle, int numRetries) where TWorkHandle : IWorkOptions
        {
            workHandle.RetryCount = numRetries;
            return workHandle;
        }

        /// <summary>
        /// The maximum delay in seconds between retries, default = 30 
        /// </summary>
        public static TWorkHandle ConfigureMaxRetryDelay<TWorkHandle>(this TWorkHandle workHandle, int maxRetryDelay) where TWorkHandle : IWorkOptions
        {
            workHandle.MaxRetryDelay = maxRetryDelay;
            return workHandle;
        }

        /// /// <summary>
        /// Optional side effect when a task fails
        /// </summary>
        public static TWorkHandle ConfigureOnFail<TWorkHandle>(this TWorkHandle workHandle, Action<Exception> onFail) where TWorkHandle : IWorkOptions
        {
            workHandle.OnFail = onFail;
            return workHandle;
        }

        /// <summary>
        /// Optional condition to determine when the task succeeds
        /// </summary>
        public static TWorkHandle ConfigureSucceedsWhen<TWorkHandle, TResult>(this TWorkHandle workHandle, Predicate<TResult> predicate) where TWorkHandle : IWorkOptions<TResult>
        {
            workHandle.SucceedsWhen = predicate;
            return workHandle;
        }

        /// <summary>
        /// Optional condition to determine when the task fails
        /// </summary>
        public static TWorkHandle ConfigureFailsWhen<TWorkHandle, TResult>(this TWorkHandle workHandle, Predicate<TResult> predicate) where TWorkHandle : IWorkOptions<TResult>
        {
            workHandle.FailsWhen = predicate;
            return workHandle;
        }

        /// <summary>
        /// Configures the work to wait until the dependent workhandle produces a result that complies to the condition before it gets executed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable"></param>
        /// <param name="condition"></param>
        public static TWorkHandle ConfigureDependOn<TWorkHandle, TDependObservableValue>(this TWorkHandle workHandle, IObservable<TDependObservableValue> observable, Predicate<TDependObservableValue> condition) where TWorkHandle : IWorkOptions
        {
            workHandle.DependOn(observable, condition);
            return workHandle;
        }

        public static IResultWorkHandle<TNextResult> Unwrap<TFirstResult, TNextResult>(this IResultWorkHandle<TFirstResult> firstResult, IResultWorkHandle<TNextResult> next)
        {
            return firstResult.Unwrap(next);
        }

        //public static TInnerWorkHandle Unwrap<TWorkHandle, TInnerWorkHandle>(this TWorkHandle workHandle) where TWorkHandle : IResultWorkHandle<TInnerWorkHandle> where TInnerWorkHandle : IWorkHandle
        //{

        //}

        //public static TInnerWorkHandle Unwrap<TInnerWorkHandle>(this IResultWorkHandle<TInnerWorkHandle> workHandle) where TInnerWorkHandle : IWorkHandle
        //{

        //}

        public static IObservable<TInnerResult> Unwrap<TInnerResult>(this IResultWorkHandle<IObservable<TInnerResult>> workHandle, CancellationToken cancellationToken = default)
        {
            return new UnwrapProxy2<TInnerResult>(workHandle, cancellationToken);
            //return workHandle.CreateUnwrapProxy<TInnerResult>();
        }

        //public static IResultWorkHandle<TResult> Do<TResult>(this IResultWorkHandle<TResult> workHandle, Action<TResult> action)
        //{
        //    return (IResultWorkHandle<TResult>)((IObservable<TResult>)workHandle).Do(action);
        //}

        //public static IFuncWorkHandle<TResult> Do<TResult>(this IFuncWorkHandle<TResult> workHandle, Action<TResult> action)
        //{
        //    //workHandle.Then(action);
        //    return (IFuncWorkHandle<TResult>)((IObservable<TResult>)workHandle).Do(action);
        //}
    }
}

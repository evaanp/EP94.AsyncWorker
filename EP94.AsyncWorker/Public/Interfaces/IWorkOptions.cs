using EP94.AsyncWorker.Internal.Models;
using EP94.AsyncWorker.Public.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Interfaces
{
    public interface IWorkOptions
    {
        /// <summary>
        /// Specifies the way the work handles the latest result
        /// </summary>
        public RetainResult RetainResult { get; set; }
        /// <summary>
        /// Configures debounce time. When the task with the same hashcode gets scheduled again within the debounce time of the previous, the previous task gets canceled.
        /// </summary>
        /// <param name="hashCode"></param>
        /// <param name="debounceTime"></param>
        public void ConfigureDebounce(int hashCode, TimeSpan debounceTime);

        /// <summary>
        /// Configures the work to wait until the dependent workhandle produces a result that complies to the condition before it gets executed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="workHandle"></param>
        /// <param name="condition"></param>
        public void DependOn<T>(IWorkHandle<T> workHandle, Predicate<T> condition);
        /// <summary>
        /// Optional side effect when a task fails
        /// </summary>
        public Action<Exception>? OnFail { get; set; }
        /// <summary>
        /// The number of retries before the task is considered as failed
        /// </summary>
        public int? RetryCount { get; set; }
        /// <summary>
        /// The maximum delay in seconds between retries, default = 30 
        /// </summary>
        public int MaxRetryDelay { get; set; }

        internal void OnOptionsSet();
    }

    public interface IWorkOptions<TResult> : IWorkOptions
    {
        /// <summary>
        /// Optional condition to determine when the task succeeds
        /// </summary>
        public Predicate<TResult>? SucceedsWhen { get; set; }
        /// <summary>
        /// Optional condition to determine when the task fails
        /// </summary>
        public Predicate<TResult>? FailsWhen { get; set; }
    }
}

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
        internal RetainResult RetainResult { get; set; }
        
        internal void ConfigureDebounce(int hashCode, TimeSpan debounceTime);

        internal void DependOn<T>(IObservable<T> observable, Predicate<T> condition);
        
        internal Action<Exception>? OnFail { get; set; }
        
        internal int? RetryCount { get; set; }
        
        internal int MaxRetryDelay { get; set; }
    }

    public interface IWorkOptions<TResult> : IWorkOptions
    {
        internal Predicate<TResult>? SucceedsWhen { get; set; }
        
        internal Predicate<TResult>? FailsWhen { get; set; }
    }
}

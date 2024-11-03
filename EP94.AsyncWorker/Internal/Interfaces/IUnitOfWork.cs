using EP94.AsyncWorker.Internal.Models;
using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Interfaces
{
    public interface IUnitOfWork
    {
        internal CancellationToken CancellationToken { get; }
        internal TimeSpan? DebounceTime { get; }
        internal int? HashCode { get; }
        internal IDependOnCondition? DependsOn { get; }
        internal Task ExecuteAsync(ExecuteWorkItem executeWorkItem, CancellationToken cancellationToken);
        internal Task<bool> WaitForNextExecutionAsync(ExecuteWorkItem workItem, DateTimeOffset next, CancellationToken cancellationToken);
    }
    //public interface IUnitOfWork<TResult> : IUnitOfWork, IWorkHandle<TResult> 
    //{ 

    //}

    //public interface IUnitOfWork<TParam, TResult> : IUnitOfWork<TResult>, IWorkHandle<TParam, TResult> { }
}

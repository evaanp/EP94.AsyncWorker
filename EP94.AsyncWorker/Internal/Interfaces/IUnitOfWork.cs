using EP94.AsyncWorker.Internal.Models;
using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Interfaces
{
    public interface IUnitOfWork
    {
        internal string? Name { get; }
        internal bool HasNext { get; }
        internal IWorkCollection Next { get; }
        internal IUnitOfWork? Previous { get; set;}
        internal ExecutionStack? LatestExecutionStack { get; }
        internal Task ExecuteAsync(ExecutionStack executionStack);
        internal void SetException(Exception exception);
        internal void SetCanceled();
        internal Task WaitForNextExecutionAsync(DateTime next, CancellationToken cancellationToken);
        internal void NotifyStart();
        internal ISubject<T> CreateSubject<T>();
    }
    public interface IUnitOfWork<TResult> : IUnitOfWork, IWorkHandle<TResult> 
    { 

    }
}

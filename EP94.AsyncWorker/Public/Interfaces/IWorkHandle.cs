using EP94.AsyncWorker.Internal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Interfaces
{
    public interface IWorkHandle : ILinkWork
    {
        IWorkHandle First { get; }
        IWorkHandle? Previous { get; }
        TaskAwaiter GetAwaiter();
        void Run();
        Task AsTask();
    }
    public interface IWorkHandle<TResult> : IWorkHandle, IObservable<TResult>, ILinkWork<TResult>
    {
        new TaskAwaiter<TResult> GetAwaiter();
        new Task<TResult> AsTask();
        IObservable<TResult> RunAsObservable();
    }
    public interface IWorkHandle<TParam, TResult> : IWorkHandle<TResult>
    {
    }
}

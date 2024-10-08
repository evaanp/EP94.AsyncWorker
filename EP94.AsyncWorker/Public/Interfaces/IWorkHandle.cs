using EP94.AsyncWorker.Internal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Interfaces
{
    public interface IWorkHandle : ILinkWork
    {
        //void Run();
        TaskAwaiter GetAwaiter();
        Task AsTask();
    }

    public interface INoResultWorkHandle : IObservable<Unit>, ILinkWork<Unit>, IWorkHandle
    {

    }

    public interface INoParameterWorkHandle : IWorkHandle
    {
        void SetSourceObservable(IObservable<Unit> observable);
    }

    public interface IActionWorkHandle : INoResultWorkHandle, IParameterWorkHandle<Unit>, IConnectableWorkHandle<Unit>, IResultWorkHandle<Unit>, IWorkOptions
    {
        
    }

    //public interface IParameterWorkHandle<TParam, TResult> : IWorkHandle
    //{
        
    //}

    public interface IActionWorkHandle<TParam> : IParameterWorkHandle<TParam>, IConnectableWorkHandle<TParam>, IResultWorkHandle<Unit>, IWorkOptions
    {
        
    }
    public interface IFuncWorkHandle<TParam, TResult> : IParameterWorkHandle<TParam>, IResultWorkHandle<TResult>, IConnectableWorkHandle<TParam>, IWorkOptions<TResult>
    {

    }

    public interface IFuncWorkHandle<TResult> : IResultWorkHandle<TResult>, IConnectableWorkHandle<Unit>, INoParameterWorkHandle, IWorkOptions<TResult>
    {
        
    }

    public interface IParameterWorkHandle<TParam> : IWorkHandle
    {

    }

    public interface IResultWorkHandle<TResult> : IWorkHandle, IObservable<TResult>, ILinkWork<TResult>
    {
        new TaskAwaiter<TResult> GetAwaiter();
        new Task<TResult> AsTask();

        //internal IResultWorkHandle<TNextResult> Unwrap<TNextResult>(IResultWorkHandle<TNextResult> next);
        //internal TInnerWorkHandle Unwrap<TInnerWorkHandle>() where TInnerWorkHandle : IWorkHandle, TResult;
        //internal IResultWorkHandle<TInnerResult> CreateUnwrapProxy<TInnerResult>();
    }

    //public interface IResultWorkHandle<TParam, TResult> : IResultWorkHandle<TResult>
    //{
        
    //}

    public interface IConnectableWorkHandle<TParam>
    {
        void SetSourceObservable(IObservable<TParam> observable);
    }
}

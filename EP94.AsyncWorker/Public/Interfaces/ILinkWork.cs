using EP94.AsyncWorker.Internal.Models;
using EP94.AsyncWorker.Public.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Interfaces
{
    public interface ILinkWork
    {
        //TWorkHandle Then<TWorkHandle>(TWorkHandle task) where TWorkHandle : IWorkHandle;
        ////IResultWorkHandle<TNextResult> Then<TNextResult>(IResultWorkHandle<TNextResult> task);
        ////IWorkHandle<TNextResult?> Then<TNextResult>(out IWorkHandle previous, IWorkHandle<TNextResult?> task);
        //IActionWorkHandle Then(ActionWorkDelegate task, ConfigureAction? configureAction = null, string? name = null);
        //IActionWorkHandle Then(Action task, ConfigureAction? configureAction = null, string? name = null);
        //IWorkHandle Then(out IWorkHandle previous, ActionWorkDelegate task, ConfigureAction? configureAction = null, string? name = null);
    }
    public interface ILinkWork<TResult>
    {
        //IWorkHandle Then(IWorkHandle task, Predicate<TResult>? condition = null);
        //IActionWorkHandle<TResult> Then(Action<TResult> task, ConfigureAction? configureAction = null, Predicate<TResult>? condition = null, string? name = null);
        //IActionWorkHandle Then(ActionWorkDelegate task, ConfigureAction? configureAction = null, Predicate<TResult>? condition = null, string? name = null);
        //IActionWorkHandle<TResult> Then(ActionWorkDelegate<TResult> task, ConfigureAction? configureAction = null, Predicate<TResult>? predicate = null, string? name = null);
        //IFuncWorkHandle<TNextResult> Then<TNextResult>(FuncWorkDelegate<TNextResult> task, ConfigureAction<TNextResult>? configureAction = null, Predicate<TResult>? predicate = null, string? name = null);
        //IFuncWorkHandle<TResult, TNextResult> Then<TNextResult>(Func<TResult, TNextResult> task, ConfigureAction<TNextResult>? configureAction = null, Predicate<TResult>? condition = null, string? name = null);
        //IFuncWorkHandle<TResult, TNextResult> Then<TNextResult>(FuncWorkDelegate<TResult, TNextResult> task, ConfigureAction<TNextResult>? configureAction = null, Predicate<TResult>? predicate = null, string? name = null);
        ////IResultWorkHandle<TResult, TNextResult> Then<TNextResult>(IResultWorkHandle<TResult, TNextResult> task, Predicate<TResult>? condition = null);
        //TWorkHandle Then<TWorkHandle>(TWorkHandle workHandle) where TWorkHandle : IConnectableWorkHandle<TResult>;

        //TResult Unwrap();
    }
}

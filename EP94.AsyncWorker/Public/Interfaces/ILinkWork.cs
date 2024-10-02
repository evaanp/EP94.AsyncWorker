using EP94.AsyncWorker.Internal.Models;
using EP94.AsyncWorker.Public.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Interfaces
{
    public interface ILinkWork
    {
        IWorkHandle Then(IWorkHandle task);
        IWorkHandle<TNextResult> Then<TNextResult>(IWorkHandle<TNextResult> task);
        //IWorkHandle<TNextResult?> Then<TNextResult>(out IWorkHandle previous, IWorkHandle<TNextResult?> task);
        IWorkHandle Then(ActionWorkDelegate task, ConfigureAction? configureAction = null, string? name = null);
        IWorkHandle Then(Action task, ConfigureAction? configureAction = null, string? name = null);
        //IWorkHandle Then(out IWorkHandle previous, ActionWorkDelegate task, ConfigureAction? configureAction = null, string? name = null);
    }
    public interface ILinkWork<TResult> : ILinkWork
    {
        IWorkHandle Then(IWorkHandle task, Predicate<TResult>? condition = null);
        IWorkHandle Then(Action<TResult> task, ConfigureAction? configureAction = null, Predicate<TResult>? condition = null, string? name = null);
        IWorkHandle Then(ActionWorkDelegate task, ConfigureAction? configureAction = null, Predicate<TResult>? condition = null, string? name = null);
        IWorkHandle Then(ActionWorkDelegate<TResult> task, ConfigureAction? configureAction = null, Predicate<TResult>? predicate = null, string? name = null);
        IWorkHandle<TNextResult> Then<TNextResult>(FuncWorkDelegate<TNextResult> task, ConfigureAction<TNextResult>? configureAction = null, Predicate<TResult>? predicate = null, string? name = null);
        IWorkHandle<TNextResult> Then<TNextResult>(Func<TResult, TNextResult> task, ConfigureAction<TNextResult>? configureAction = null, Predicate<TResult>? condition = null, string? name = null);
        IWorkHandle<TNextResult> Then<TNextResult>(FuncWorkDelegate<TResult, TNextResult> task, ConfigureAction<TNextResult>? configureAction = null, Predicate<TResult>? predicate = null, string? name = null);
        IWorkHandle<TNextResult> Then<TNextResult>(IWorkHandle<TNextResult> task, Predicate<TResult>? condition = null);
        IWorkHandle<T> Unwrap<T>();
    }
}

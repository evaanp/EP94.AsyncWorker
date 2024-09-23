using EP94.AsyncWorker.Internal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Interfaces
{
    public interface IAsyncWorker
    {
        ITrigger<TParam> CreateTriggerAsync<TParam>();
        IObservable<T> ScheduleBackgroundWorkAsync<T>(IFuncWorkDelegate<T> task, TimeSpan interval, Func<bool> predicate, CancellationToken cancellationToken = default);
        IWorkHandle<TResult?> ScheduleWorkAsync<TResult>(IFuncWorkDelegate<TResult> task, Action<IWorkOptions<TResult?>>? configureAction = null, CancellationToken cancellationToken = default, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0);
        IWorkHandle ScheduleWorkAsync(IWorkDelegate task, Action<IWorkOptions>? configureAction = null, CancellationToken cancellationToken = default, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0);
    }
}

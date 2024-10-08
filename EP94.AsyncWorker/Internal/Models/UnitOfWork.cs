using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Internal.Utils;
using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    internal class UnitOfWork<TParameter, TResult> : WorkBase<TParameter, TResult>, IUnitOfWork, IWorkOptions<TResult>
    {
        public IWorkDelegate Work { get; }

        protected override IObservable<TResult> RunObservable { get; }

        public UnitOfWork(IWorkDelegate work, IWorkScheduler workScheduler, IWorkFactory workFactory, string? name, CancellationToken cancellationToken) : base(workScheduler, workFactory, name, cancellationToken)
        {
            Work = work;
            RunObservable = ParameterSubject
                .Switch()
                .Select(x =>
                {
                    //return Observable.FromAsync(() =>
                    //{
                        ExecuteWorkItem<TParameter, TResult> executeWorkItem = new ExecuteWorkItem<TParameter, TResult>(this, x);
                        workScheduler.ScheduleWork(executeWorkItem, null);
                    return executeWorkItem.ToOnceTask();
                    //});
                })
                .Switch();
        }

        protected override async Task DoExecuteAsync(ExecuteWorkItem<TParameter, TResult> executeWorkItem)
        {
            executeWorkItem.ExecutionCounter++;
            await SafeExecuteAsync(Work,
                onSuccess: (result) => {
                    executeWorkItem.ResultSubject.OnNext(result);
                    executeWorkItem.ResultSubject.OnCompleted();
                },
                onCanceled: () =>
                {
                    executeWorkItem.ResultSubject.OnCompleted();
                },
                onFail: e =>
                {
                    OnFail?.Invoke(e);
                    if (executeWorkItem.ExecutionCounter <= RetryCount)
                    {
                        double seconds = Math.Min(Math.Pow(2, executeWorkItem.ExecutionCounter), 30);
                        WorkScheduler.ScheduleWork(executeWorkItem, DateTime.UtcNow.AddSeconds(seconds));
                    }
                    else
                    {
                        executeWorkItem.ResultSubject.OnError(e);
                    }
                },
                succeedsWhen: SucceedsWhen,
                failsWhen: FailsWhen,
                executeWorkItem.Parameter);
        }

        protected override ISubject<IObservable<TParameter>> GetParameterSubject()
        {
            return new BehaviorSubject<IObservable<TParameter>>(Observable.Return(default(TParameter)));
            //return new ReplaySubject<IObservable<TParameter>>(1);
        }
    }
    internal class FuncUnitOfWork<TResult>(IFuncWorkDelegate<TResult> work, IWorkScheduler workScheduler, IWorkFactory workFactory, string? name, CancellationToken cancellationToken)
        : UnitOfWork<Unit, TResult>(work, workScheduler, workFactory, name, cancellationToken), IFuncWorkHandle<TResult>
    { }

    internal class FuncUnitOfWork<TParam, TResult>(IFuncWorkDelegate work, IWorkScheduler workScheduler, IWorkFactory workFactory, string? name, CancellationToken cancellationToken)
        : UnitOfWork<TParam, TResult>(work, workScheduler, workFactory, name, cancellationToken), IFuncWorkHandle<TParam, TResult>
    { }
    internal class ActionUnitOfWork<TParam>(IActionWorkDelegate work, IWorkScheduler workScheduler, IWorkFactory workFactory, string? name, CancellationToken cancellationToken)
        : UnitOfWork<TParam, Unit>(work, workScheduler, workFactory, name, cancellationToken), IActionWorkHandle<TParam>
    { }

    internal class ActionUnitOfWork(IActionWorkDelegate work, IWorkScheduler workScheduler, IWorkFactory workFactory, string? name, CancellationToken cancellationToken)
        : UnitOfWork<Unit, Unit>(work, workScheduler, workFactory, name, cancellationToken), IActionWorkHandle 
    {

    }

}

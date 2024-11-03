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

        public UnitOfWork(IWorkDelegate work, IWorkScheduler workScheduler, IWorkFactory workFactory, CancellationToken cancellationToken) : base(workScheduler, workFactory, cancellationToken)
        {
            Work = work;
            RunObservable = ParameterSubject
                .Switch()
                .Select(x =>
                {
                    ExecuteWorkItem<TParameter, TResult> executeWorkItem = new ExecuteWorkItem<TParameter, TResult>(this, x);
                    return Observable.FromAsync(() =>
                    {
                        return executeWorkItem.ToOnceTask(() =>
                        {
                            workScheduler.ScheduleWork(executeWorkItem, null);
                        }, CancellationToken);
                    });
                })
                .Switch();
        }

        protected override async Task DoExecuteAsync(ExecuteWorkItem<TParameter, TResult> executeWorkItem, CancellationToken cancellationToken)
        {
            executeWorkItem.ExecutionCounter++;
            await SafeExecuteAsync(Work,
                onSuccess: executeWorkItem.ResultSubject.OnNext,
                onCanceled: executeWorkItem.SetCanceled,
                onFail: e =>
                {
                    OnFail?.Invoke(e);
                    if (executeWorkItem.ExecutionCounter <= RetryCount)
                    {
                        double seconds = Math.Min(Math.Pow(2, executeWorkItem.ExecutionCounter), MaxRetryDelay);
                        WorkScheduler.ScheduleWork(executeWorkItem, DateTimeOffset.UtcNow.AddSeconds(seconds));
                    }
                    else
                    {
                        executeWorkItem.SetException(e);
                    }
                },
                succeedsWhen: SucceedsWhen,
                failsWhen: FailsWhen,
                cancellationToken,
                executeWorkItem.Parameter);
        }

        protected override ISubject<IObservable<TParameter>> GetParameterSubject()
        {
            return new BehaviorSubject<IObservable<TParameter>>(Observable.Return(default(TParameter)));
            //return new ReplaySubject<IObservable<TParameter>>(1);
        }
    }
    internal class FuncUnitOfWork<TResult>(IFuncWorkDelegate<TResult> work, IWorkScheduler workScheduler, IWorkFactory workFactory, CancellationToken cancellationToken)
        : UnitOfWork<Unit, TResult>(work, workScheduler, workFactory, cancellationToken), IFuncWorkHandle<TResult>
    { }

    internal class FuncUnitOfWork<TParam, TResult>(IFuncWorkDelegate work, IWorkScheduler workScheduler, IWorkFactory workFactory, CancellationToken cancellationToken)
        : UnitOfWork<TParam, TResult>(work, workScheduler, workFactory, cancellationToken), IFuncWorkHandle<TParam, TResult>
    { }
    internal class ActionUnitOfWork<TParam>(IActionWorkDelegate work, IWorkScheduler workScheduler, IWorkFactory workFactory, CancellationToken cancellationToken)
        : UnitOfWork<TParam, Unit>(work, workScheduler, workFactory, cancellationToken), IActionWorkHandle<TParam>
    { }

    internal class ActionUnitOfWork(IActionWorkDelegate work, IWorkScheduler workScheduler, IWorkFactory workFactory, CancellationToken cancellationToken)
        : UnitOfWork<Unit, Unit>(work, workScheduler, workFactory, cancellationToken), IActionWorkHandle 
    {

    }

}

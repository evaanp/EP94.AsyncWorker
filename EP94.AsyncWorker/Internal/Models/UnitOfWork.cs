using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Internal.Utils;
using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    internal class UnitOfWork<TParameter, TResult>(IWorkDelegate work, IUnitOfWork? previous, IWorkScheduler workScheduler, IWorkFactory workFactory, string? name, CancellationToken cancellationToken) : WorkBase<TResult>(previous, workScheduler, workFactory, name, cancellationToken), IUnitOfWork<TResult>, IWorkOptions<TResult>
    {

        public IWorkDelegate Work { get; } = work;

        public Predicate<TResult>? SucceedsWhen { get; set; }
        public Predicate<TResult>? FailsWhen { get; set; }
        public Action<Exception>? OnFail { get; set; }
        public int? RetryCount { get; set; }
        public int MaxRetryDelay { get; set; }

        private bool _hasNeverRan = true;
        private ISubject<TResult> _subject = previous?.CreateSubject<TResult>() ?? new Subject<TResult>();

        protected override async Task DoExecuteAsync(ExecutionStack executionStack)
        {
            ExecutionContext<TResult?> executionContext;
            if (!executionStack.TryPeek(out IExecutionContext? previousContext) || !ReferenceEquals(previousContext.Owner, this))
            {
                executionContext = new ExecutionContext<TResult?>(this);
                executionStack.Push(executionContext);
            }
            else
            {
                executionContext = (ExecutionContext<TResult?>)previousContext;
            }
            executionContext.ExecutionCounter++;
            await SafeExecuteAsync(Work,
                onSuccess: result =>
                {
                    executionStack.LastResult = result;
                    executionContext.TaskCompletionSource.TrySetResult(result);
                    _subject.OnNext(result);
                    if (HasNext)
                    {
                        ScheduleNext(executionStack);
                    }
                },
                onCanceled: () => {
                    DoSetCanceled();
                },
                onFail: e =>
                {
                    OnFail?.Invoke(e);
                    if (executionContext.ExecutionCounter <= RetryCount)
                    {
                        double seconds = Math.Min(Math.Pow(2, executionContext.ExecutionCounter), 30);
                        WorkScheduler.ScheduleWork(this, DateTime.UtcNow.AddSeconds(seconds), executionStack);
                    }
                    else
                    {
                        SetException(e);
                    }
                }, 
                succeedsWhen: SucceedsWhen, 
                failsWhen: FailsWhen, 
                executionStack.LastResult);
        }

        protected override void DoSetCanceled()
        {
            Next.SetCanceled();
            _subject.OnCompleted();
        }

        public override void SetException(Exception exception)
        {
            Next.SetException(exception);
            _subject.OnError(exception);
        }

        public override IDisposable Subscribe(IObserver<TResult> observer)
        {
            NotifyStart();
            return _subject.Subscribe(observer);
        }

        public override void NotifyStart()
        {
            if (Previous is not null)
            {
                Previous.NotifyStart();
            }
            else if (_hasNeverRan)
            {
                _hasNeverRan = false;
                ExecutionStack executionStack = new ExecutionStack();
                WorkScheduler.ScheduleWork(this, null, executionStack);
            }
        }

        public override ISubject<T> CreateSubject<T>()
        {
            if (Previous is not null)
            {
                return Previous.CreateSubject<T>();
            }
            return new Subject<T>();
        }
    }
    internal class UnitOfWork<TResult>(IWorkDelegate work, IUnitOfWork? previous, IWorkScheduler workScheduler, IWorkFactory workFactory, string? name, CancellationToken cancellationToken) 
        : UnitOfWork<Empty, TResult>(work, previous, workScheduler, workFactory, name, cancellationToken);
}

using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    internal class TimeBasedTrigger<T> : WorkBase<T>, IWorkHandle<T>
    {
        private Func<DateTime> _getNextTimeFunc;
        private IWorkDelegate _task;
        private ReplaySubject<T> _subject = new ReplaySubject<T>(1);

        public TimeBasedTrigger(IFuncWorkDelegate<T> task, DateTime firstRun, Func<DateTime> getNextTimeFunc, IWorkScheduler workScheduler, IWorkFactory workFactory, string? name, CancellationToken cancellationToken)
            : base(null, workScheduler, workFactory, name, cancellationToken)
        {
            _task = task;
            _getNextTimeFunc = getNextTimeFunc;
            workScheduler.ScheduleWork(this, firstRun, new ExecutionStack());
        }
        public override ISubject<T1> CreateSubject<T1>() => new ReplaySubject<T1>(1);

        public override void NotifyStart()
        {
            
        }

        public override IDisposable Subscribe(IObserver<T> observer) => _subject.Subscribe(observer);
        protected override async Task DoExecuteAsync(ExecutionStack executionStack)
        {
            ExecutionContext<T> executionContext = new ExecutionContext<T>(this);
            executionStack.Push(executionContext);
            await SafeExecuteAsync<T>(_task,
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
                onCanceled: DoSetCanceled,
                onFail: SetException, null, null);
            WorkScheduler.ScheduleWork(this, _getNextTimeFunc(), new ExecutionStack());
        }

        public override void SetException(Exception exception) => _subject.OnError(exception);
        protected override void DoSetCanceled() => _subject.OnCompleted();
    }
}

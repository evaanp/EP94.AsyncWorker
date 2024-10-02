using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using EP94.AsyncWorker.Internal.Models;
using System.Reactive.Linq;

namespace EP94.AsyncWorker.Internal.Models
{
    internal class BackgroundWork<T> : WorkBase<T>, IBackgroundWork<T>
    {
        private IWorkDelegate _task;
        private TimeSpan _interval;
        private Func<bool>? _predicate;
        private Subject<T> _subject = new Subject<T>();

        public BackgroundWork(IWorkDelegate task, TimeSpan interval, Func<bool>? predicate, IWorkScheduler workScheduler, IWorkFactory workFactory, string? name, CancellationToken cancellationToken) : base(null, workScheduler, workFactory, name, cancellationToken)
        {
            _task = task;
            _interval = interval;
            _predicate = predicate;
            workScheduler.ScheduleWork(this, null, new ExecutionStack());
        }

        protected override async Task DoExecuteAsync(ExecutionStack executionStack)
        {
            ExecutionContext<T> executionContext = new ExecutionContext<T>(this);
            executionStack.Clear();
            executionStack.Push(executionContext);
            if (_predicate?.Invoke() ?? true)
            {
                await SafeExecuteAsync<T>(_task, 
                    onSuccess: (result) => {
                        executionStack.LastResult = result;
                        executionContext.TaskCompletionSource.SetResult(result);
                        _subject.OnNext(result);
                        ScheduleNext(executionStack);
                        }, 
                    _subject.OnCompleted, 
                    _subject.OnError,
                    null,
                    null);
            }
            WorkScheduler.ScheduleWork(this, DateTime.UtcNow.Add(_interval), executionStack);
        }

        protected override void DoSetCanceled()
        {
            _subject.OnCompleted();
        }

        public override void SetException(Exception exception)
        {
            _subject.OnError(exception);
        }

        public override IDisposable Subscribe(IObserver<T> observer) => _subject.Subscribe(observer);

        public override void NotifyStart()
        {
            
        }

        public override ISubject<T1> CreateSubject<T1>() => new Subject<T1>();
    }

    internal class BackgroundWork(IWorkDelegate task, TimeSpan interval, Func<bool>? predicate, IWorkScheduler workScheduler, IWorkFactory workFactory, string? name, CancellationToken cancellationToken) : BackgroundWork<Empty>(task, interval, predicate, workScheduler, workFactory, name, cancellationToken), IBackgroundWork
    {

    }
}

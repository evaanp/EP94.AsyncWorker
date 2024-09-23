using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Internal.Utils;
using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    internal class Trigger<T>(IWorkScheduler workScheduler, IWorkFactory workFactory, string? name, CancellationToken cancellationToken) : WorkBase<T>(null, workScheduler, workFactory, name, cancellationToken), ITrigger<T>, ILinkWork, IWorkHandle
    {
        private ReplaySubject<T?> _subject = new ReplaySubject<T?>(1);

        public void OnNext(T param)
        {
            ExecutionStack executionStack = new ExecutionStack();
            ExecutionContext<T> executionContext = new ExecutionContext<T>(this);
            executionContext.TaskCompletionSource.SetResult(param);
            executionStack.LastResult = param;
            executionStack.Push(executionContext);
            WorkScheduler.ScheduleWork(this, null, executionStack);
        }

        protected override Task DoExecuteAsync(ExecutionStack executionStack)
        {
            ScheduleNext(executionStack);
            _subject.OnNext((T)executionStack.LastResult!);
            return Task.CompletedTask;
        }

        public void OnNext(object? param)
        {
            if (param is T p)
            {
                OnNext(p);
            }
            throw new InvalidOperationException($"Parameter of type '{typeof(T).Name}' expected but received '{param?.GetType().Name}'");
        }

        public override IDisposable Subscribe(IObserver<T?> observer) => _subject.Subscribe(observer);

        public override void SetCanceled() => _subject.OnCompleted();
        public override void SetException(Exception exception) => _subject.OnError(exception);

        public override void NotifyStart()
        {
           
        }

        public override ISubject<T1> CreateSubject<T1>() => new ReplaySubject<T1>(1);
    }
}

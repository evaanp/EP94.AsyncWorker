using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Internal.Utils;
using EP94.AsyncWorker.Public.Exceptions;
using EP94.AsyncWorker.Public.Interfaces;
using EP94.AsyncWorker.Public.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    internal abstract class WorkBase<TResult>(IUnitOfWork? previous, IWorkScheduler workScheduler, IWorkFactory workFactory, string? name, CancellationToken cancellationToken) : IUnitOfWork<TResult>
    {
        public string? Name { get; } = name;
        public bool HasNext => _next.Any();
        public IUnitOfWork? Previous 
        {
            get => _previous;
            set
            {
                ArgumentNullException.ThrowIfNull(value, nameof(value));
                if (_previous is not null)
                {
                    throw new InvalidOperationException("Previous already set");
                }
                _previous = value;
            }
        }
        private IUnitOfWork? _previous = previous;
        IWorkHandle? IWorkHandle.Previous => (IWorkHandle?)Previous;
        IWorkHandle IWorkHandle.First
        {
            get
            {
                IWorkHandle first = this;
                while (first.Previous is not null)
                {
                    first = first.Previous;
                }
                return first;
            }
        }
        public IWorkCollection Next => _next;
        private WorkCollection _next = new WorkCollection();
        public CancellationToken CancellationToken { get; } = cancellationToken;
        public ExecutionStack? LatestExecutionStack { get; private set; }

        protected IWorkScheduler WorkScheduler { get; } = workScheduler;

        private IWorkFactory _workFactory = workFactory;
        public Task ExecuteAsync(ExecutionStack executionStack)
        {
            LatestExecutionStack = executionStack;
            return DoExecuteAsync(executionStack);
        }
        protected abstract Task DoExecuteAsync(ExecutionStack executionStack);

        public virtual void SetCanceled() { }
        public virtual void SetException(Exception exception) { }
        public abstract void NotifyStart();
        public void Run() => NotifyStart();

        public IWorkHandle<T> Unwrap<T>()
        {
            UnwrapProxy<TResult, T> proxy = new UnwrapProxy<TResult, T>(this, WorkScheduler, _workFactory, CancellationToken);
            _next.Add(proxy);
            return proxy;
        }

        public abstract ISubject<T> CreateSubject<T>();

        protected async Task SafeExecuteAsync<T>(IWorkDelegate work, Action<T> onSuccess, Action onCanceled, Action<Exception> onFail, Predicate<T?>? succeedsWhen, Predicate<T>? failsWhen, params object?[]? args)
        {
            try
            {
                T returnValue = await work.InvokeAsync<T>(() => default, CancellationToken, args).WaitAsync(CancellationToken);
                if (succeedsWhen is not null && !succeedsWhen(returnValue))
                {
                    onFail(new WorkFailedException("Result of work didn't pass succeedsWhen predicate", returnValue, succeedsWhen.ToString()));
                }
                else if (failsWhen is not null && failsWhen(returnValue)) 
                {
                    onFail(new WorkFailedException("Result of work passed failsWhen predicate", returnValue, failsWhen.ToString()));
                }
                else
                {
                    onSuccess(returnValue);
                }
            }
            catch (TaskCanceledException)
            {
                onCanceled();
            }
            catch (OperationCanceledException)
            {
                onCanceled();
            }
            catch (Exception e)
            {
                onFail(e);
            }
        }

        protected void ScheduleNext(ExecutionStack executionStack)
        {
            foreach (IUnitOfWork next in Next)
            {
                WorkScheduler.ScheduleWork(next, null, executionStack.Clone());
            }
        }

        public async Task WaitForNextExecutionAsync(DateTime next, CancellationToken cancellationToken)
        {
            using CancellationTokenSource linkedToken = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken, cancellationToken);
            try
            {
                TimeSpan timeDiff = next - DateTime.UtcNow;
                if (timeDiff > TimeSpan.Zero)
                {
                    await Task.Delay(timeDiff, linkedToken.Token);
                }
            }
            catch (TaskCanceledException)
            {
                SetCanceled();
            }
        }

        public IWorkHandle Then(ActionWorkDelegate task, ConfigureAction? configureAction = null, string? name = null)
        {
            IWorkHandle unitOfWork = _workFactory.CreateWork(task, configureAction, name, CancellationToken);
            ((IUnitOfWork)unitOfWork).Previous = this;
            _next.Add((IUnitOfWork)unitOfWork);
            return unitOfWork;
        }

        public IWorkHandle Then(ActionWorkDelegate<TResult> task, ConfigureAction? configureAction = null, string? name = null)
        {
            IWorkHandle unitOfWork = _workFactory.CreateWork(this, task, configureAction, name, CancellationToken);
            _next.Add((IUnitOfWork)unitOfWork);
            return unitOfWork;
        }

        public IWorkHandle<TNextResult> Then<TNextResult>(FuncWorkDelegate<TResult, TNextResult> task, ConfigureAction<TNextResult>? configureAction = null, string? name = null)
        {
            IWorkHandle<TNextResult> unitOfWork = _workFactory.CreateWork((IUnitOfWork<TResult>)this, task, configureAction, name, CancellationToken);
            _next.Add((IUnitOfWork)unitOfWork);
            return unitOfWork;
        }

        public IWorkHandle<TNextResult> Then<TNextResult>(IWorkHandle<TNextResult> task)
        {
            IUnitOfWork first = (IUnitOfWork)task;
            while (first.Previous is not null)
            {
                first = first.Previous;
            }
            first.Previous = this;
            _next.Add((IUnitOfWork)task);
            return task;
        }

        public IWorkHandle<TNextResult> Then<TNextResult>(FuncWorkDelegate<TNextResult> task, ConfigureAction<TNextResult>? configureAction = null, string? name = null)
        {
            IWorkHandle<TNextResult> unitOfWork = _workFactory.CreateWork(task, configureAction, name, CancellationToken);
            ((IUnitOfWork)unitOfWork).Previous = this;
            _next.Add((IUnitOfWork)unitOfWork);
            return unitOfWork;
        }

        public Task<TResult> AsTask() => this.ToOnceTask(NotifyStart, CancellationToken);
        Task IWorkHandle.AsTask() => AsTask();
        TaskAwaiter<TResult> IWorkHandle<TResult>.GetAwaiter() => AsTask().GetAwaiter();
        TaskAwaiter IWorkHandle.GetAwaiter() => ((Task)AsTask()).GetAwaiter();

        public abstract IDisposable Subscribe(IObserver<TResult> observer);


        //public IWorkHandle<TNextResult?> Then<TNextResult>(IWorkHandle<TNextResult?> task)
        //{
        //    ((IUnitOfWork)task).Previous = this;
        //    _next.Add((IUnitOfWork)task);
        //    return task;
        //}
    }
}

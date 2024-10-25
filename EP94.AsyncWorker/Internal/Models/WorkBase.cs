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
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    internal abstract class WorkBase<TParameter, TResult> : ObservableWorkBase<TResult>, IUnitOfWork, IParameterWorkHandle<TParameter>, IResultWorkHandle<TResult>
    {
        public RetainResult RetainResult { get; set; }
        public int? HashCode { get; private set; }
        public TimeSpan? DebounceTime { get; private set; }
        public IDependOnCondition? DependsOn { get; protected set; }
        public string? Name { get; }
        public Predicate<TResult>? SucceedsWhen { get; set; }
        public Predicate<TResult>? FailsWhen { get; set; }
        public Action<Exception>? OnFail { get; set; }
        public int? RetryCount { get; set; }
        public int MaxRetryDelay { get; set; }
        public CancellationToken CancellationToken { get; }
        protected IWorkScheduler WorkScheduler { get; }

        private IWorkFactory _workFactory;

        protected ISubject<IObservable<TParameter>> ParameterSubject { get; }

        //protected abstract IObservable<TResult> RunObservable { get; }

        public WorkBase(IWorkScheduler workScheduler, IWorkFactory workFactory, string? name, CancellationToken cancellationToken)
            : base (cancellationToken)
        {
            Name = name;
            CancellationToken = cancellationToken;
            WorkScheduler = workScheduler;
            _workFactory = workFactory;
            ParameterSubject = GetParameterSubject();
        }

        public Task ExecuteAsync(ExecuteWorkItem executeWorkItem, CancellationToken cancellationToken)
        {
            if (executeWorkItem is not ExecuteWorkItem<TParameter, TResult> genericExecuteWorkItem)
            {
                throw new ArgumentException($"WorkItem with generic type '{typeof(TParameter)}' expected, but received '{executeWorkItem.GetType().GenericTypeArguments.FirstOrDefault()}'");
            }
            return DoExecuteAsync(genericExecuteWorkItem, cancellationToken);
        }
        protected abstract Task DoExecuteAsync(ExecuteWorkItem<TParameter, TResult> executeWorkItem, CancellationToken cancellationToken);

        //public void Run() => this.Subscribe();
        public void ConfigureDebounce(int hashCode, TimeSpan debounceTime)
        {
            HashCode = hashCode;
            DebounceTime = debounceTime;
        }

        //public TaskAwaiter<TResult> GetAwaiter() => this.ToOnceTask(null, CancellationToken).GetAwaiter();
        //public Task<TResult> AsTask() => this.ToOnceTask(null, CancellationToken);

        //Task IWorkHandle.AsTask() => this.ToOnceTask(null, CancellationToken);
        //TaskAwaiter IWorkHandle.GetAwaiter() => ((IWorkHandle)this).AsTask().GetAwaiter();

        protected abstract ISubject<IObservable<TParameter>> GetParameterSubject();

        protected async Task SafeExecuteAsync<T>(IWorkDelegate work, Action<T> onSuccess, Action onCanceled, Action<Exception> onFail, Predicate<T>? succeedsWhen, Predicate<T>? failsWhen, CancellationToken cancellationToken, params object?[]? args)
        {
            try
            {
                T returnValue = await work.InvokeAsync<T>(() => default, cancellationToken, args).WaitAsync(cancellationToken);
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
            catch (TimeoutException)
            {
                onCanceled();
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

        public async Task<bool> WaitForNextExecutionAsync(ExecuteWorkItem workItem, DateTimeOffset next, CancellationToken cancellationToken)
        {
            CancellationTokenSource cancelWaitToken = new CancellationTokenSource();
            CancellationTokenSource linkedToken = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken, cancellationToken);
            try
            {
                TimeSpan timeDiff = next - DateTimeOffset.UtcNow;
                if (timeDiff > TimeSpan.Zero)
                {
                    await Task.Delay(timeDiff, linkedToken.Token);
                }
                return true;
            }
            catch (TaskCanceledException)
            {
                workItem.SetCanceled();
            }
            finally
            {
                cancelWaitToken.Dispose();
                linkedToken.Dispose();
            }
            return false;
        }

        //public IDisposable Subscribe(IObserver<TResult> observer) => RunObservable.Subscribe(observer);

        public void DependOn<T>(IObservable<T> workHandle, Predicate<T> condition)
        {
            DependsOn = new DependOnCondition<T>(workHandle, condition);
        }

        public void SetSourceObservable(IObservable<TParameter> observable)
        {
            ParameterSubject.OnNext(observable);
        }

        //public IResultWorkHandle<TResult, TNextResult> Then<TNextResult>(IResultWorkHandle<TResult, TNextResult> task, Predicate<TResult>? condition = null)
        //{
        //    throw new NotImplementedException();
        //}

        //public IObservable<TResult> SetSourceObservable(IObservable<Unit> observable)
        //{
        //    throw new NotImplementedException();
        //}

        //TWorkHandle ILinkWork.Then<TWorkHandle>(TWorkHandle task)
        //{
        //    if (task is IActionWorkHandle<TResult> parameterWorkHandle)
        //    {
        //        parameterWorkHandle.SetSourceObservable(this);
        //    }
        //    else if (task is INoParameterWorkHandle noParameterWorkHandle)
        //    {
        //        noParameterWorkHandle.SetSourceObservable(this.Select(_ => Unit.Default));
        //    }
        //    return task;
        //}

        public IResultWorkHandle<TNextResult> Unwrap<TNextResult>(IResultWorkHandle<TNextResult> next)
        {
            return new UnwrapProxy<TResult, TNextResult>(this, next, WorkScheduler, _workFactory, CancellationToken.None);
        }

        //public TResult Unwrap<TInnerWorkHandle>() where TInnerWorkHandle : IWorkHandle, TResult
        //{
        //    return new UnwrapProxy2<TInnerWorkHandle, TResult>(this, WorkScheduler, _workFactory, Name, CancellationToken);
        //}

        //public TInnerWorkHandle Unwrap<TInnerWorkHandle>() where TInnerWorkHandle : IWorkHandle, TResult
        //{
        //    return new UnwrapProxy2<TInnerWorkHandle>((IResultWorkHandle<TInnerWorkHandle>)this, WorkScheduler, _workFactory, Name, CancellationToken);
        //}

        //public TResult Unwrap<T>() where T : IConnectableWorkHandle<>
        //{

        //}

        //void IConnectableWorkHandle<Unit>.SetSourceObservable(IObservable<Unit> observable)
        //{
        //    SetSourceObservable(observable.Select(_ => default(TParameter)));
        //}

        //IWorkHandle<TResult, Unit> ILinkWork<TResult>.Then(ActionWorkDelegate<TResult> task, ConfigureAction? configureAction, Predicate<TResult>? predicate, string? name)
        //{
        //    IWorkHandle<TResult, Unit> unitOfWork = _workFactory.CreateWork(task, configureAction, name, CancellationToken);
        //    unitOfWork.Connect(this.Where(x => predicate?.Invoke(x) ?? true));
        //    return unitOfWork;
        //}
    }

    //internal abstract class WorkBase<TResult>(IWorkScheduler workScheduler, IWorkFactory workFactory, string? name, CancellationToken cancellationToken) : WorkBase<Unit, TResult>(workScheduler, workFactory, name, cancellationToken)
    //{
    //}
}

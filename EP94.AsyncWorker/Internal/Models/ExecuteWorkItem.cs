using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    internal class ExecuteWorkItem(IUnitOfWork unitOfWork)
    {
        public int ExecutionCounter { get; set; }
        public DateTimeOffset TimeStamp { get; } = DateTimeOffset.UtcNow;
        public IUnitOfWork UnitOfWork { get; } = unitOfWork;
        //public override int GetHashCode()
        //{
        //    if (UnitOfWork.HashCode.HasValue)
        //    {
        //        return UnitOfWork.HashCode.Value;
        //    }
        //    return base.GetHashCode();
        //}
        public virtual void SetCanceled() { }
        public virtual void SetException(Exception e) { }
    }
    internal class ExecuteWorkItem<TParam, TResult>(IUnitOfWork unitOfWork, TParam parameter) : ExecuteWorkItem(unitOfWork), IObservable<TResult>
    {
        public TParam Parameter { get; } = parameter;
        public ISubject<TResult> ResultSubject = new ReplaySubject<TResult>(1);
        public IDisposable Subscribe(IObserver<TResult> observer)
        {
            return ResultSubject.Subscribe(observer);
        }

        public override void SetCanceled()
        {
            ResultSubject.OnCompleted();
        }

        public override void SetException(Exception e)
        {
            ResultSubject.OnError(e);
        }
    }
}

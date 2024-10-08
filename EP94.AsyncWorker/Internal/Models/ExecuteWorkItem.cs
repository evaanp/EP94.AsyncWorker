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
    internal record ExecuteWorkItem(IUnitOfWork UnitOfWork)
    {
        public int ExecutionCounter { get; set; }
        public DateTime TimeStamp { get; } = DateTime.UtcNow;
        public override int GetHashCode()
        {
            if (UnitOfWork.HashCode.HasValue)
            {
                return UnitOfWork.HashCode.Value;
            }
            return base.GetHashCode();
        }
        public virtual void SetCanceled() { }
        public virtual void SetException(Exception e) { }
    }
    internal record ExecuteWorkItem<TParam, TResult>(IUnitOfWork UnitOfWork, TParam Parameter) : ExecuteWorkItem(UnitOfWork), IObservable<TResult>
    {
        public ISubject<TResult> ResultSubject = new Subject<TResult>();
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

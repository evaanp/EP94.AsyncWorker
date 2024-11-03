using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    internal class InterruptableTimer<T> : WorkBase<Unit, T>
    {
        private TimeSpan? _dueTime;
        private TimeSpan _interval;
        private Subject<IObservable<long>> _currentTimer = new Subject<IObservable<long>>();

        public InterruptableTimer(IWorkScheduler workScheduler, IWorkFactory workFactory, TimeSpan? dueTime, TimeSpan interval, CancellationToken cancellationToken) : base(workScheduler, workFactory, cancellationToken)
        {
            _dueTime = dueTime;
            _interval = interval;
        }

        protected override IObservable<T> RunObservable => throw new NotImplementedException();

        protected override Task DoExecuteAsync(ExecuteWorkItem<Unit, T> executeWorkItem, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override ISubject<IObservable<Unit>> GetParameterSubject() => new Subject<IObservable<Unit>>();

        public void Stop()
        {

        }

        public void Reset(TimeSpan? dueTime = null)
        {

        }
    }
}

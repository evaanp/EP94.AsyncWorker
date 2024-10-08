using EP94.AsyncWorker.Internal.Utils;
using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    internal abstract class ObservableWorkBase<TResult>(CancellationToken cancellationToken) : IResultWorkHandle<TResult>
    {
        protected abstract IObservable<TResult> RunObservable { get; }
        private CancellationToken _cancellationToken = cancellationToken;


        public IDisposable Subscribe(IObserver<TResult> observer) => RunObservable.Subscribe(observer);
        public TaskAwaiter<TResult> GetAwaiter() => this.ToOnceTask(null, _cancellationToken).GetAwaiter();
        public Task<TResult> AsTask() => this.ToOnceTask(null, _cancellationToken);

        Task IWorkHandle.AsTask() => this.ToOnceTask(null, _cancellationToken);
        TaskAwaiter IWorkHandle.GetAwaiter() => ((IWorkHandle)this).AsTask().GetAwaiter();

    }
}

using EP94.AsyncWorker.Internal.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    internal class ConcurrentWorkQueue(int maxLevelOfConcurrency) : IDisposable
    {
        private AsyncAutoResetEvent _signaler = new AsyncAutoResetEvent(maxLevelOfConcurrency);
        private AsyncAutoResetEvent _queueBusySignaler = new AsyncAutoResetEvent(1);
        private ConcurrentQueue<ExecuteWorkItem> _queue = new ConcurrentQueue<ExecuteWorkItem>();

        public Task AwaitWorkAsync(CancellationToken cancellationToken)
        {
            if (_queue.IsEmpty)
            {
                return _signaler.WaitOneAsync(cancellationToken);
            }
            return Task.CompletedTask;
        }
        public Task AwaitQueueBusyAsync(CancellationToken cancellationToken) => _queueBusySignaler.WaitOneAsync(cancellationToken);

        public void ScheduleWork(IUnitOfWork unitOfWork, ExecutionStack executionStack)
        {
            _queue.Enqueue(new ExecuteWorkItem(unitOfWork, executionStack));
            _signaler.Signal();
            if (_queue.Count > 1)
            {
                _queueBusySignaler.Signal();
            }
        }

        public bool TryDequeue([NotNullWhen(true)] out ExecuteWorkItem? unitOfWork)
        {
            return _queue.TryDequeue(out unitOfWork);
        }

        public void Dispose()
        {
            _signaler.Dispose();
            _queueBusySignaler.Dispose();
        }
    }
}

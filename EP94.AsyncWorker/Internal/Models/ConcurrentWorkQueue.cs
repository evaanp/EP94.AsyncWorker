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
        private ConcurrentQueue<ExecuteWorkItem> _queue = new ConcurrentQueue<ExecuteWorkItem>();

        public async Task<ExecuteWorkItem> AwaitWorkAsync(CancellationToken cancellationToken)
        {
            ExecuteWorkItem? next = null;
            while (next is null)
            {
                if (_queue.IsEmpty)
                {
                    await _signaler.WaitOneAsync(cancellationToken);
                }
                _queue.TryDequeue(out next);
            }
            
            return next!;
        }

        public void ScheduleWork(ExecuteWorkItem workItem)
        {
            _queue.Enqueue(workItem);
            _signaler.Signal();
        }

        public void ScheduleWork(IUnitOfWork unitOfWork, ExecutionStack executionStack)
        {
            _queue.Enqueue(new ExecuteWorkItem(unitOfWork, executionStack));
            _signaler.Signal();
        }

        public bool TryDequeue([NotNullWhen(true)] out ExecuteWorkItem? unitOfWork)
        {
            return _queue.TryDequeue(out unitOfWork);
        }

        public void Dispose()
        {
            _signaler.Dispose();
        }
    }
}

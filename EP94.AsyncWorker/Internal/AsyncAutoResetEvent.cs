using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal
{
    internal class AsyncAutoResetEvent(int maxCount) : IDisposable
    {
        private bool _disposed;
        private SemaphoreSlim _semaphore = new SemaphoreSlim(0, maxCount);
        private int _maxCount = maxCount;

        public Task WaitOneAsync(CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _semaphore.WaitAsync(cancellationToken);
        }

        public void Signal()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            lock (this)
            {
                if (_semaphore.CurrentCount < _maxCount)
                {
                    _semaphore.Release();
                }
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _semaphore.Dispose();
        }
    }
}

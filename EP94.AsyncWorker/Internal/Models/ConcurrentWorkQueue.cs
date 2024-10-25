using EP94.AsyncWorker.Internal.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    internal class ConcurrentWorkQueue
    {
        private Channel<ExecuteWorkItem> _executeWorkChannel = Channel.CreateUnbounded<ExecuteWorkItem>();

        public async Task<ExecuteWorkItem> AwaitWorkAsync(CancellationToken cancellationToken)
        {
            return await _executeWorkChannel.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        }

        public void ScheduleWork(ExecuteWorkItem executeWorkItem)
        {
            _executeWorkChannel.Writer.TryWrite(executeWorkItem);
        }
    }
}

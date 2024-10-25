using EP94.AsyncWorker.Internal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Interfaces
{
    internal interface IWorkScheduler
    {
        TimeSpan? DefaultTimeout { get; }
        CancellationToken StopToken { get; }
        void ScheduleWork(ExecuteWorkItem executeWorkItem, DateTimeOffset? dateTime);
    }
}

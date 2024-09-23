using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Interfaces
{
    internal interface IWorkScheduler
    {
        void ScheduleWork(IUnitOfWork unitOfWork, DateTime? dateTime, ExecutionStack executionStack);
    }
}

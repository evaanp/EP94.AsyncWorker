using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EP94.AsyncWorker.Internal.Interfaces;

namespace EP94.AsyncWorker.Public.Interfaces
{
    internal interface IWorkCollection : IEnumerable<IConditionalWork>
    {
        void ScheduleNext(ExecutionStack executionStack);
        void SetException(Exception exception);
        void SetCanceled();
    }
}

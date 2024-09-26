using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    internal record ExecuteWorkItem(IUnitOfWork UnitOfWork, ExecutionStack ExecutionStack)
    {
        public DateTime TimeStamp => ExecutionStack.TimeStamp;
        public bool IgnoreDebounce { get; set; }
        public override int GetHashCode()
        {
            if (UnitOfWork.HashCode.HasValue)
            {
                return UnitOfWork.HashCode.Value;
            }
            return base.GetHashCode();
        }
    }
}

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
    }
}

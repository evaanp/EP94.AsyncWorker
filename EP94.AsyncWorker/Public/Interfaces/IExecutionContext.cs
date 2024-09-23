using EP94.AsyncWorker.Internal.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Interfaces
{
    public interface IExecutionContext
    {
        IUnitOfWork Owner { get; }
        object? Result { get; }
        int ExecutionCounter { get; set; }
    }
}

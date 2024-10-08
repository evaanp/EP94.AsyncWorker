using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Internal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Interfaces
{
    public interface IBackgroundWork<T> : IFuncWorkHandle<T>, IUnitOfWork
    {

    }

    public interface IBackgroundWork : IBackgroundWork<Unit>
    {

    }
}

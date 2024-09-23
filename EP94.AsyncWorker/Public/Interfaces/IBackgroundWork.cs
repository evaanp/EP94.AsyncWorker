using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Internal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Interfaces
{
    public interface IBackgroundWork<T> : IObservable<T>, IUnitOfWork, ILinkWork<T>
    {

    }

    public interface IBackgroundWork : IBackgroundWork<Empty>
    {

    }
}

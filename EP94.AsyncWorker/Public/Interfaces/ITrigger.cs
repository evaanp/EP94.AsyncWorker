using EP94.AsyncWorker.Internal.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Interfaces
{
    public interface ITrigger : IWorkHandle, ILinkWork, IUnitOfWork
    {
        
    }
    public interface ITrigger<T> : ITrigger, IWorkHandle<T>
    {
        void OnNext(T param);
    }
}

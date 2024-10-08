using EP94.AsyncWorker.Internal.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Interfaces
{
    public interface ITrigger<T> : IFuncWorkHandle<T>
    {
        void OnNext(T param);
    }
}

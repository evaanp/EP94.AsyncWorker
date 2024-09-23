using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Interfaces
{
    public interface IFuncWorkDelegate : IWorkDelegate
    {
    }

    public interface IFuncWorkDelegate<out T> : IFuncWorkDelegate { }
}

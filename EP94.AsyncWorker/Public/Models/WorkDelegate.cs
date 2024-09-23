using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Internal.Models;
using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Models
{
    public static class WorkDelegate
    {
        public static IFuncWorkDelegate Create<TParam, TResult>(FuncWorkDelegate<TParam, TResult> task) => 
            new ResultWorkDelegate<TParam, TResult>(task);

        public static IActionWorkDelegate Create<TParam>(ActionWorkDelegate<TParam> task) =>
            new VoidWorkDelegate<TParam>(task);

        public static IFuncWorkDelegate<TResult> Create<TResult>(FuncWorkDelegate<TResult> task) =>
            new ResultWorkDelegate<TResult>(task);

        public static IActionWorkDelegate Create(ActionWorkDelegate task) =>
            new VoidWorkDelegate(task);
    }
}

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

        public static IFuncWorkDelegate Create<TParam, TResult>(Func<TParam, TResult> task) =>
            new ResultWorkDelegate<TParam, TResult>((param, c) => Task.FromResult(task(param)));

        public static IActionWorkDelegate Create<TParam>(ActionWorkDelegate<TParam> task) =>
            new VoidWorkDelegate<TParam>(task);

        public static IActionWorkDelegate Create<TParam>(Action<TParam> task) =>
            new VoidWorkDelegate<TParam>((param, c) => { task(param); return Task.CompletedTask; });

        public static IFuncWorkDelegate<TResult> Create<TResult>(FuncWorkDelegate<TResult> task) =>
            new ResultWorkDelegate<TResult>(task);

        public static IFuncWorkDelegate<TResult> Create<TResult>(Func<TResult> task) =>
            new ResultWorkDelegate<TResult>((c) => Task.FromResult(task()));

        public static IActionWorkDelegate Create(ActionWorkDelegate task) =>
            new VoidWorkDelegate(task);

        public static IActionWorkDelegate Create(Action task) =>
            new VoidWorkDelegate((c) => { task(); return Task.CompletedTask; });
    }
}

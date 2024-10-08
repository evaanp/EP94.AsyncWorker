using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Utils
{
    public static class TaskExtensions
    {
        public static IFuncWorkHandle<TResult> AsWorkHandle<TResult>(this Func<Task<TResult>> task, IWorkFactory? workFactory = null, CancellationToken cancellationToken = default)
        {
            return GetOrDefault(workFactory).CreateWork((c) => task(), cancellationToken: cancellationToken);
        }

        public static IActionWorkHandle AsWorkHandle(this Func<Task> task, IWorkFactory? workFactory = null, CancellationToken cancellationToken = default)
        {
            return GetOrDefault(workFactory).CreateWork((c) => task(), cancellationToken: cancellationToken);
        }

        //public static IFuncWorkHandle<TResult> AsWorkHandle<TResult>(this Task<TResult> task, IWorkFactory? workFactory = null, CancellationToken cancellationToken = default)
        //{
        //    return GetOrDefault(workFactory).CreateWork((c) =>
        //    {
        //        task.Start();
        //        return task;
        //    }, cancellationToken: cancellationToken);
        //}

        ////public static IActionWorkHandle AsWorkHandle<TResult>(this Func<Task> task, IWorkFactory? workFactory = null, CancellationToken cancellationToken = default)
        ////{
        ////    return GetOrDefault(workFactory).CreateWork((c) => task(), cancellationToken: cancellationToken);
        ////}

        private static IWorkFactory GetOrDefault(IWorkFactory? workFactory) => workFactory ?? IWorkFactory.Default;
    }
}

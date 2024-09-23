using EP94.AsyncWorker.Public.Interfaces;
using EP94.AsyncWorker.Public.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    internal class VoidWorkDelegate(ActionWorkDelegate task) : IActionWorkDelegate
    {
        private ActionWorkDelegate _workDelegate = task;

        public async Task<T> InvokeAsync<T>(Func<T> defaultValue, CancellationToken cancellationToken, params object?[] args)
        {
            await _workDelegate(cancellationToken).ConfigureAwait(false);
            return defaultValue();
        }
    }

    internal class VoidWorkDelegate<TParam>(ActionWorkDelegate<TParam> task) : IActionWorkDelegate
    {
        private ActionWorkDelegate<TParam> _workDelegate = task;
        public async Task<T> InvokeAsync<T>(Func<T> defaultValue, CancellationToken cancellationToken, params object?[] args)
        {
            object? firstArg = args.FirstOrDefault();
            if (firstArg is not TParam param)
            {
                throw new ArgumentException($"Argument of type {typeof(TParam).Name} expected, but received {firstArg?.GetType().Name}");
            }
            await _workDelegate(param, cancellationToken);
            return defaultValue();
        }
    }

    internal class ResultWorkDelegate<TParam, TResult>(FuncWorkDelegate<TParam, TResult> workDelegate) : IFuncWorkDelegate<TResult>
    {
        private FuncWorkDelegate<TParam, TResult> _workDelegate = workDelegate;

        public async Task<T> InvokeAsync<T>(Func<T> defaultValue, CancellationToken cancellationToken, params object?[] args)
        {
            object? firstArg = args.FirstOrDefault();
            if (firstArg is null)
            {
                return defaultValue();
            }
            if (firstArg is not TParam param)
            {
                throw new ArgumentException($"Argument of type {typeof(TParam).Name} expected, but received {firstArg?.GetType().Name}");
            }
            object? result = await _workDelegate(param, cancellationToken).ConfigureAwait(false);
            if (result is null)
            {
                return defaultValue();
            }
            if (result is not T t)
            {
                throw new ArgumentException($"Expected a returnvalue of type {typeof(TResult).Name} but received type {result?.GetType().Name}");
            }
            return t;
        }
    }

    internal class ResultWorkDelegate<TResult>(FuncWorkDelegate<TResult> workDelegate) : IFuncWorkDelegate<TResult>
    {
        private FuncWorkDelegate<TResult> _workDelegate = workDelegate;

        public async Task<T> InvokeAsync<T>(Func<T> defaultValue, CancellationToken cancellationToken, params object?[] args)
        {
            object? result = await _workDelegate(cancellationToken).ConfigureAwait(false);
            if (result is null)
            {
                return defaultValue();
            }
            if (result is not T t)
            {
                throw new ArgumentException($"Expected a returnvalue of type {typeof(TResult).Name} but received type {result?.GetType().Name}");
            }
            return t;
        }
    }
}

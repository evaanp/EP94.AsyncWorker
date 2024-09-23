using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    public class ExecutionContext<TResult>(IUnitOfWork owner) : IExecutionContext
    {
        public IUnitOfWork Owner { get; } = owner;
        public int ExecutionCounter { get; set; }
        public TaskCompletionSource<TResult?> TaskCompletionSource { get; } = new TaskCompletionSource<TResult?>();
        public Task<TResult?> Task => TaskCompletionSource.Task;
        public Task NonGenericTask => TaskCompletionSource.Task;
        public object? Result => TaskCompletionSource.Task.IsCompleted ? TaskCompletionSource.Task.Result : throw new InvalidOperationException("Task not yet finished");
    }
}

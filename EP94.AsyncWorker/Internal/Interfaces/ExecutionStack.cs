using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Interfaces
{
    internal class ExecutionStack
    {
        public object? LastResult { get; set; }
        private Stack<IExecutionContext> _innerStack = new Stack<IExecutionContext>();

        public ExecutionStack(IExecutionContext[] content, object? lastResult)
        {
            _innerStack = new Stack<IExecutionContext>(content);
            LastResult = lastResult;
        }

        public ExecutionStack()
        {
            _innerStack = new Stack<IExecutionContext>();
        }

        public bool IsEmpty => _innerStack.Count == 0;

        public void Push(IExecutionContext executionContext)
        {
            _innerStack.Push(executionContext);
        }

        public void Pop()
        {
            _innerStack.Pop();
        }

        public bool TryPeek([NotNullWhen(true)] out IExecutionContext? executionContext) => _innerStack.TryPeek(out executionContext);

        public void Clear() => _innerStack.Clear();

        public ExecutionStack Clone() => new ExecutionStack([.. _innerStack], LastResult);
    }
}

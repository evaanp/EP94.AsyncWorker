using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Public.Interfaces;
using EP94.AsyncWorker.Public.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EP94.AsyncWorker.Internal.Models
{
    internal class WorkCollection(IWorkScheduler workScheduler) : IWorkCollection
    {
        private IWorkScheduler _workScheduler = workScheduler;
        private ConcurrentBag<IConditionalWork> _items = new ConcurrentBag<IConditionalWork>();

        public void Add(IConditionalWork conditionalWork) 
        {
            _items.Add(conditionalWork);
        }

        public bool Any() => !_items.IsEmpty;

        public IEnumerator<IConditionalWork> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

        public void SetCanceled()
        {
            foreach (IUnitOfWork unitOfWork in _items)
            {
                unitOfWork.SetCanceled();
            }
        }

        public void SetException(Exception exception)
        {
            foreach (IUnitOfWork unitOfWork in _items)
            {
                unitOfWork.SetException(exception);
            }
        }

        public TaskAwaiter GetAwaiter() => AsTask().GetAwaiter();

        public Task AsTask() => Task.WhenAll(_items.Select(x => ((IWorkHandle)x).AsTask()));

        public void ScheduleNext(ExecutionStack executionStack)
        {
            foreach (IConditionalWork item in _items)
            {
                if (item.SchouldExecute(executionStack.LastResult))
                {
                    _workScheduler.ScheduleWork(item.UnitOfWork, null, executionStack.Clone());
                }
            }
        }
    }
}

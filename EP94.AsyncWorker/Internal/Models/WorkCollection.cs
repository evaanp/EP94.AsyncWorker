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

namespace EP94.AsyncWorker.Internal.Models
{
    internal class WorkCollection : IWorkCollection
    {
        private ConcurrentBag<IUnitOfWork> _items = new ConcurrentBag<IUnitOfWork>();

        public void Add(IUnitOfWork unitOfWork) 
        {
            _items.Add(unitOfWork);
        }

        public bool Any() => !_items.IsEmpty;

        public IEnumerator<IUnitOfWork> GetEnumerator() => _items.GetEnumerator();
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
    }
}

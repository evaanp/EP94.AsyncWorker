using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    internal class DebounceItem(ExecuteWorkItem executeWorkItem)
    {
        public ExecuteWorkItem WorkItem
        {
            get
            {
                lock (this)
                {
                    return _workItem;
                }
            }
        }
        private ExecuteWorkItem _workItem = executeWorkItem;

        public void Update(ExecuteWorkItem workItem)
        {
            lock (this)
            {
                WorkItem.UnitOfWork.SetCanceled();
                _workItem = workItem;
            }
        }
    }
}

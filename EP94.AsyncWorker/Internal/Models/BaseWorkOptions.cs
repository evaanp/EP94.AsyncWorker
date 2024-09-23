using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    public abstract class BaseWorkOptions
    {
        /// <summary>
        /// Optional side effect when a task fails
        /// </summary>
        public Func<Exception?>? OnFail { get; set; }
        /// <summary>
        /// The number of retries before the task is considered as failed
        /// </summary>
        public int? RetryCount { get; set; }
        /// <summary>
        /// The maximum delay in seconds between retries, default = 30 
        /// </summary>
        public int MaxRetryDelay { get; set; } = 30;
    }
}

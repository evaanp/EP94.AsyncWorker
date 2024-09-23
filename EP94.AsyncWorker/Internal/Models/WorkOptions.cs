using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    public class WorkOptions(Func<Task> work)
    {
        internal Func<Task> Work { get; } = work;
    }
    public class WorkOptions<TResult>(Func<Task<TResult>> work) : BaseWorkOptions
    {
        internal Func<Task<TResult>> Work { get; set; } = work;
        /// <summary>
        /// Optional condition to determine when the task succeeds
        /// </summary>
        public Predicate<TResult>? SucceedsWhen { get; set; }
        /// <summary>
        /// Optional condition to determine when the task fails
        /// </summary>
        public Predicate<TResult>? FailsWhen { get; set; }
    }
}

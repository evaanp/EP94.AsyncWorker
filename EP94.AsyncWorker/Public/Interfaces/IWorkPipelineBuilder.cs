using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Interfaces
{
    public interface IWorkPipelineBuilder
    {
        public IWorkPipelineBuilder Then<TResult>(Func<Task<TResult>> task, Action<IWorkOptions<TResult>>? configureAction = null, CancellationToken cancellationToken = default);
        public IWorkPipelineBuilder Then(Func<Task> task, Action<IWorkOptions>? configureAction = null, CancellationToken cancellationToken = default);
    }
}

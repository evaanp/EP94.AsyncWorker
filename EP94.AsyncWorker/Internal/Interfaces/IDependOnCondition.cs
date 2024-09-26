using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Interfaces
{
    public interface IDependOnCondition
    {
        Task WaitForConditionAsync(CancellationToken cancellationToken);
    }
}

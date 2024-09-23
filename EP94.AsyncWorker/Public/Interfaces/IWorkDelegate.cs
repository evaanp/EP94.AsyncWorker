using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Interfaces
{
    public interface IWorkDelegate
    {
        internal Task<T> InvokeAsync<T>(Func<T> defaultValue, CancellationToken cancellationToken, params object?[]? args);
    }
}

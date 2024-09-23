using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Models
{
    public delegate Task ActionWorkDelegate(CancellationToken cancellationToken = default);
    public delegate Task ActionWorkDelegate<in TParam>(TParam param, CancellationToken cancellationToken = default);
    public delegate Task<TResult> FuncWorkDelegate<TResult>(CancellationToken cancellationToken = default);
    public delegate Task<TResult> FuncWorkDelegate<in TParam, TResult>(TParam param, CancellationToken cancellationToken = default);
}

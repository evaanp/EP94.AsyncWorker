using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Models
{
    public delegate void ConfigureAction<T>(IWorkOptions<T> options);
    public delegate void ConfigureAction(IWorkOptions options);
}

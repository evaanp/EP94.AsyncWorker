using EP94.AsyncWorker.Internal.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    internal record ConditionalWork(IUnitOfWork UnitOfWork) : IConditionalWork
    {
        public bool SchouldExecute(object? value) => true;
    }
    internal record ConditionalWork<T>(IUnitOfWork UnitOfWork, Predicate<T>? Predicate) : IConditionalWork
    {
        public bool SchouldExecute(object? value)
        {
            if (Predicate is not null)
            {
                return value is T t && Predicate(t);
            }
            return true;
        }
    }
}

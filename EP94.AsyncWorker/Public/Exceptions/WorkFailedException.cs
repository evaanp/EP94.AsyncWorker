using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Public.Exceptions
{
    public class WorkFailedException(string message, object? value, string predicate) : Exception(message)
    {
        public object? Value { get; } = value;
        public string Predicate { get; } = predicate;
    }
}

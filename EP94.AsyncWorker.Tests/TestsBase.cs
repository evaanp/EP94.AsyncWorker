using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Tests
{
    public class TestsBase
    {
        public static IWorkFactory CreateDefaultWorkFactory(int numberOfWorkers = 1, TimeSpan? defaultTimeout = null) => IWorkFactory.Create(numberOfWorkers, defaultTimeout ?? TimeSpan.FromSeconds(10));
        public static IEnumerable<object[]> TestData()
        {
            yield return [true];
            yield return [5];
            yield return ["string"];
        }
    }
}

using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Tests
{
    public class TimebasedTriggerTests : TestsBase
    {
        [Fact]
        public async Task TestTimebasedTriggerAsync()
        {
            Random random = new Random();
            IWorkFactory workFactory = CreateDefaultWorkFactory();
            List<int> results = new List<int>();
            IFuncWorkHandle<int> workHandle = workFactory.CreateTimebasedTrigger<int>((c) =>
            {
                int result = random.Next();
                results.Add(result);
                return Task.FromResult(result);
            }, workFactory.CreateWork(() => DateTimeOffset.UtcNow), workFactory.CreateWork(() => DateTimeOffset.UtcNow.AddSeconds(5)));

            await Task.Delay(6000);
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public async Task TestTimebaseTriggerCancelAsync()
        {
            Assert.Fail(); // TODO
        }
    }
}

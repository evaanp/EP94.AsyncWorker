using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Tests
{
    public class BackgroundWorkTests : TestsBase
    {
        [Theory]
        [MemberData(nameof(TestData))]
        public async Task TestBackgroundWork_ReturnValueAsync<T>(T returnValue)
        {
            IWorkFactory workFactory = CreateDefaultWorkFactory();
            List<T> received = [];
            IBackgroundWork<T> backgroundWork = workFactory.CreateBackgroundWork((c) =>
            {
                return Task.FromResult(returnValue);
            }, TimeSpan.FromSeconds(1));
            backgroundWork.Subscribe(received.Add);
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            Assert.Single(received);
            Assert.Equal(returnValue, received.First());
        }

    }
}

using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Tests
{
    public class TriggerTests : TestsBase
    {
        [Theory]
        [MemberData(nameof(TestData))]
        public async Task TestTrigger_SubscribeBeforeValue<T>(T returnValue)
        {
            IWorkFactory workFactory = CreateDefaultWorkFactory();
            ITrigger<T> trigger = workFactory.CreateTriggerAsync<T>();
            List<T> received = [];
            IDisposable subscription = trigger.Subscribe(received.Add);
            trigger.OnNext(returnValue);
            await Task.Delay(200);
            Assert.Single(received);
            Assert.Equal(returnValue, received.First());
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task TestTrigger_SubscribeAfterValue<T>(T returnValue)
        {
            IWorkFactory workFactory = CreateDefaultWorkFactory();
            ITrigger<T> trigger = workFactory.CreateTriggerAsync<T>();
            List<T> received = [];
            trigger.OnNext(returnValue);
            IDisposable subscription = trigger.Subscribe(received.Add);
            await Task.Delay(200);
            Assert.Single(received);
            Assert.Equal(returnValue, received.First());
        }
    }
}

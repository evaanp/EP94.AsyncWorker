using EP94.AsyncWorker.Internal.Utils;
using EP94.AsyncWorker.Public.Interfaces;
using EP94.AsyncWorker.Public.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Tests
{
    public class CombinedTests : TestsBase
    {
        [Theory]
        [MemberData(nameof(TestData))]
        public async Task TestTrigger_ThenUnitOfWork_ReturnValue_SubscribeAfter<T>(T returnValue)
        {
            IWorkFactory workFactory = CreateDefaultWorkFactory();
            ITrigger<T> trigger = workFactory.CreateTriggerAsync<T>();
            List<T> received = [];
            IFuncWorkHandle<T, T> workHandle = trigger.ThenDo((value, c) =>
            {
                return Task.FromResult(value);
            });
            trigger.OnNext(returnValue);
            workHandle.Subscribe(received.Add);
            await Task.Delay(500);
            Assert.Single(received);
            Assert.Equal(returnValue, received.First());
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task TestTrigger_ThenUnitOfWork_ReturnValue_SubscribeBefore<T>(T returnValue)
        {
            IWorkFactory workFactory = CreateDefaultWorkFactory();
            ITrigger<T> trigger = workFactory.CreateTriggerAsync<T>();
            List<T> received = [];
            IFuncWorkHandle<T, T> workHandle = trigger.ThenDo((value, c) =>
            {
                return Task.FromResult(value);
            });
            workHandle.Subscribe(received.Add);
            trigger.OnNext(returnValue);
            await Task.Delay(500);
            Assert.Single(received);
            Assert.Equal(returnValue, received.First());
        }

        [Theory]
        [InlineData(2)]
        [InlineData(8)]
        public async Task TestMultipleLinkedWorkHandles(int number)
        {
            IWorkFactory workFactory = CreateDefaultWorkFactory();
            ITrigger<int> trigger = workFactory.CreateTriggerAsync<int>();
            IFuncWorkHandle<int, int> workHandle = trigger.ThenDo((i, c) => Task.FromResult(i));
            for (int i = 0; i < number; i++)
            {
                workHandle = workHandle
                    .ThenDo((n, c) =>
                    {
                        return Task.FromResult(++n);
                    });
            }
            trigger.OnNext(1);
            int result = await workHandle;
            Assert.Equal(number + 1, result);
        }

        [Fact]
        public async Task TestUnrelatedMultipleLinkedAsync()
        {
            List<object> result = new List<object>();
            //IActionWorkHandle first = new Task(() =>
            //{
            //    result.Add(1);
            //}).AsWorkHandle();

        }

        //[Fact]
        //public async Task UpwrapTest()
        //{
        //    int value = 5;
        //    IWorkFactory workFactory = CreateDefaultWorkFactory();
        //    int result = await workFactory.CreateWork(c =>
        //    {
        //        ITrigger<int> trigger = workFactory.CreateTriggerAsync<int>();
        //        IWorkHandle<int> resultWorkHandle = trigger.Then((value, c) =>
        //        {
        //            return Task.FromResult(value);
        //        });
        //        trigger.OnNext(value);
        //        return Task.FromResult(resultWorkHandle);
        //    })
        //    .Unwrap<int>();
        //    Assert.Equal(value, result);
        //}
    }
}

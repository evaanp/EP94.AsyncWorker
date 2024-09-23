using EP94.AsyncWorker.Public.Exceptions;
using EP94.AsyncWorker.Public.Interfaces;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace EP94.AsyncWorker.Tests
{
    public class UnitOfWorkTests : TestsBase
    {
        [Theory]
        [MemberData(nameof(TestData))]
        public async Task TestUnitOfWork_ReturnValueAsync<T>(T returnValue)
        {
            IWorkFactory workFactory = CreateDefaultWorkFactory();
            T result = await workFactory.CreateWork((c) =>
            {
                return Task.FromResult(returnValue);
            });
            Assert.Equal(returnValue, result);
        }

        [Fact]
        public async Task TestRetryTwoTimes()
        {
            IWorkFactory workFactory = CreateDefaultWorkFactory();
            int retried = 0;
            IWorkHandle workHandle = workFactory.CreateWork((c) =>
            {
                if (retried == 2)
                {
                    return Task.CompletedTask;
                }
                retried++;
                throw new InvalidOperationException();
            }, options => options.RetryCount = 2);
            await workHandle;
            Assert.Equal(2, retried);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TestFailsWhenOption(bool value)
        {
            IWorkFactory workFactory = CreateDefaultWorkFactory();
            IWorkHandle<bool> workHandle = workFactory.CreateWork(c =>
            {
                return Task.FromResult(value);
            }, options =>
            {
                options.FailsWhen = value => !value;
            });
            if (value)
            {
                Assert.Equal(value, await workHandle);
            }
            else
            {
                await Assert.ThrowsAsync<WorkFailedException>(async () =>
                {
                    await workHandle;
                });
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TestSucceedsWhenOption(bool value)
        {
            IWorkFactory workFactory = CreateDefaultWorkFactory();
            IWorkHandle<bool> workHandle = workFactory.CreateWork(c =>
            {
                return Task.FromResult(value);
            }, options =>
            {
                options.SucceedsWhen = value => value;
            });
            if (value)
            {
                Assert.Equal(value, await workHandle);
            }
            else
            {
                await Assert.ThrowsAsync<WorkFailedException>(async () =>
                {
                    await workHandle;
                });
            }
        }

        [Fact]
        public async Task TestRateLimitingOrder()
        {
            IWorkFactory workFactory = CreateDefaultWorkFactory();
            List<int> results = new List<int>();
            int[] expectedValues = Enumerable.Range(0, 100).ToArray();
            List<IWorkHandle> workHandles = [];
            for (int i = 0; i < expectedValues.Length; i++)
            {
                int index = i;
                IWorkHandle workHandle = workFactory.CreateWork((c) =>
                {
                    results.Add(expectedValues[index]);
                    return Task.CompletedTask;
                });
                workHandles.Add(workHandle);
            }
            foreach (IWorkHandle workHandle in workHandles)
            {
                workHandle
                    .Run();
            }
            await Task.WhenAll(workHandles.Select(x => x.AsTask()));
            Assert.Equal(expectedValues, results);
        }

        [Fact]
        public async Task TestTimeout()
        {
            IWorkFactory workFactory = CreateDefaultWorkFactory(defaultTimeout: TimeSpan.FromSeconds(1));
            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await workFactory.CreateWork((c) => Task.Delay(TimeSpan.FromSeconds(2), c));
            });
        }
    }
}
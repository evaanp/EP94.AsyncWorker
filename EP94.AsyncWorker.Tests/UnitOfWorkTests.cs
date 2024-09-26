using EP94.AsyncWorker.Public.Exceptions;
using EP94.AsyncWorker.Public.Interfaces;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using System.Diagnostics;

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
                }, options => options.RetainResult = Public.Models.RetainResult.RetainLast, $"Item_{index}", new CancellationTokenSource(2000).Token);
                workHandles.Add(workHandle);
            }
            foreach (IWorkHandle workHandle in workHandles)
            {
                workHandle
                    .Run();
            }
            await Task.Delay(500);
            try
            {

            await Task.WhenAll(workHandles.Select(x => x.AsTask()));
            }
            catch (Exception e)
            {
                int counter = EP94.AsyncWorker.Internal.Utils.ObservableExtensions.Counter;
            }
            Assert.Equal(expectedValues, results);
        }

        [Fact]
        public async Task TestSameHashCode()
        {
            IWorkFactory workFactory = CreateDefaultWorkFactory();
            List<int> results = new List<int>();
            int[] values = Enumerable.Range(0, 100).ToArray();
            List<IWorkHandle> workHandles = [];
            for (int i = 0; i < values.Length; i++)
            {
                int index = i;
                IWorkHandle workHandle = workFactory.CreateWork((c) =>
                {
                    results.Add(values[index]);
                    return Task.CompletedTask;
                }, options => options.ConfigureDebounce(nameof(TestSameHashCode).GetHashCode(), TimeSpan.FromMilliseconds(50)));
                workHandles.Add(workHandle);
            }
            foreach (IWorkHandle workHandle in workHandles)
            {
                workHandle
                    .Run();
            }
            foreach (IWorkHandle workHandle in workHandles)
            {
                if (workHandle != workHandles.Last())
                {
                    await Assert.ThrowsAsync<TaskCanceledException>(async () => await workHandle);
                }
                else
                {
                    await workHandle;
                }
            }
            //await Task.WhenAll(workHandles.Select(x => x.AsTask()));
            Assert.Equal([values.Last()], results);
        }

        [Fact]
        public async Task TestSameHashCodeDependOnTrigger()
        {
            IWorkFactory workFactory = CreateDefaultWorkFactory();
            List<int> results = new List<int>();
            int[] values = Enumerable.Range(0, 100).ToArray();
            List<IWorkHandle> workHandles = [];
            ITrigger<bool> trigger = workFactory.CreateTriggerAsync(false, false);
            for (int i = 0; i < values.Length; i++)
            {
                int index = i;
                IWorkHandle workHandle = workFactory.CreateWork((c) =>
                {
                    results.Add(values[index]);
                    return Task.CompletedTask;
                }, options => 
                {
                    options.ConfigureDebounce(nameof(TestSameHashCode).GetHashCode(), TimeSpan.FromMilliseconds(50));
                    options.DependOn(trigger, value => value);
                }, name: index.ToString());
                workHandles.Add(workHandle);
            }
            foreach (IWorkHandle workHandle in workHandles)
            {
                workHandle
                    .Run();
            }
            await Task.Delay(2000);
            trigger.OnNext(true);
            foreach (IWorkHandle workHandle in workHandles)
            {
                if (workHandle != workHandles.Last())
                {
                    await Assert.ThrowsAsync<TaskCanceledException>(async () => await workHandle);
                }
                else
                {
                    await workHandle;
                }
            }
            //await Task.WhenAll(workHandles.Select(x => x.AsTask()));
            Assert.Equal([values.Last()], results);
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
using EP94.AsyncWorker.Internal.Utils;
using EP94.AsyncWorker.Public.Exceptions;
using EP94.AsyncWorker.Public.Interfaces;
using EP94.AsyncWorker.Public.Models;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using System.Diagnostics;
using System.Reactive.Threading.Tasks;

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
            IActionWorkHandle workHandle = workFactory.CreateWork((c) =>
            {
                if (retried == 2)
                {
                    return Task.CompletedTask;
                }
                retried++;
                throw new InvalidOperationException();
            })
                .ConfigureRetry(2);
            await workHandle;
            Assert.Equal(2, retried);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TestFailsWhenOption(bool value)
        {
            IWorkFactory workFactory = CreateDefaultWorkFactory();
            IFuncWorkHandle<bool> workHandle = workFactory.CreateWork(c =>
            {
                return Task.FromResult(value);
            })
                .ConfigureFailsWhen((bool value) => !value);
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
            IFuncWorkHandle<bool> workHandle = workFactory.CreateWork(c =>
            {
                return Task.FromResult(value);
            })
                .ConfigureSucceedsWhen((bool value) => value);
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
            List<IActionWorkHandle> workHandles = [];
            for (int i = 0; i < expectedValues.Length; i++)
            {
                int index = i;
                IActionWorkHandle workHandle = workFactory.CreateWork((c) =>
                {
                    results.Add(expectedValues[index]);
                    return Task.CompletedTask;
                }, $"Item_{index}", new CancellationTokenSource(2000).Token)
                    .ConfigureRetainResult(RetainResult.RetainLast);
                workHandles.Add(workHandle);
            }
            List<Task> tasks = new List<Task>();
            foreach (IActionWorkHandle workHandle in workHandles)
            {
                tasks.Add(workHandle.AsTask());
            }
            await Task.Delay(500);
            await Task.WhenAll(tasks);
            Assert.Equal(expectedValues, results);
        }

        [Fact]
        public async Task TestSameHashCode()
        {
            IWorkFactory workFactory = CreateDefaultWorkFactory(10);
            List<int> results = new List<int>();
            int[] values = Enumerable.Range(0, 100).ToArray();
            List<IActionWorkHandle> workHandles = [];
            for (int i = 0; i < values.Length; i++)
            {
                int index = i;
                IActionWorkHandle workHandle = workFactory.CreateWork((c) =>
                {
                    results.Add(values[index]);
                    return Task.CompletedTask;
                }).ConfigureDebounce(nameof(TestSameHashCode).GetHashCode(), TimeSpan.FromMilliseconds(100));
                workHandles.Add(workHandle);
            }
            List<Task> tasks = new List<Task>();
            foreach (IActionWorkHandle workHandle in workHandles)
            {
                tasks.Add(workHandle.AsTask());
            }
            foreach (Task task in tasks)
            {
                if (task != tasks.Last())
                {
                    await Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
                }
                else
                {
                    await task;
                }
            }
            //await Task.WhenAll(workHandles.Select(x => x.AsTask()));
            Assert.Equal([values.Last()], results);
        }

        [Fact]
        public async Task TestSameHashCodeDependOnTrigger()
        {
            IWorkFactory workFactory = CreateDefaultWorkFactory(10);
            List<int> results = new List<int>();
            int[] values = Enumerable.Range(0, 100).ToArray();
            List<IWorkHandle> workHandles = [];
            ITrigger<bool> trigger = workFactory.CreateTriggerAsync(false, false);
            for (int i = 0; i < values.Length; i++)
            {
                int index = i;
                IActionWorkHandle workHandle = workFactory.CreateWork((c) =>
                {
                    results.Add(values[index]);
                    return Task.CompletedTask;
                }, name: index.ToString())
                    .ConfigureDependOn(trigger, value => value)
                    .ConfigureDebounce(nameof(TestSameHashCode).GetHashCode(), TimeSpan.FromMilliseconds(100));

                workHandles.Add(workHandle);
            }
            List<Task> tasks = new List<Task>();
            foreach (IActionWorkHandle workHandle in workHandles)
            {
                tasks.Add(workHandle.AsTask());
            }
            await Task.Delay(2000);
            trigger.OnNext(true);
            foreach (Task task in tasks)
            {
                if (task != tasks.Last())
                {
                    await Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
                }
                else
                {
                    await task;
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
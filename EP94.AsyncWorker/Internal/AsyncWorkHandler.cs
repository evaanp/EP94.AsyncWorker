using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Internal.Models;
using EP94.AsyncWorker.Internal.Utils;
using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EP94.AsyncWorker.Internal
{
    internal class AsyncWorkHandler : IWorkScheduler, IAsyncDisposable
    {
        public TimeSpan? DefaultTimeout { get; }
        public CancellationToken StopToken => _stopCts.Token;

        private Task _workQueueListenerTask;
        private ConcurrentDictionary<ExecuteWorkItem, Task> _waitTasks;
        private ConcurrentWorkQueue _workQueue;
        private CancellationTokenSource _stopCts;
        private TimeSpan? _defaultTimeout;
        private TaskFactory _taskFactory;
        private Dictionary<int, Task> _workers;
        private SemaphoreSlim _activeWorkSemaphore;
        private ConcurrentDictionary<long, Lazy<Task>> _runningTasks;
        private long _workIdCounter;
        private ConcurrentDictionary<int, DebounceItem> _debounceWork;
        private SemaphoreSlim _debounceWorkSemaphore;
        public AsyncWorkHandler(int maxLevelOfConcurrency, TaskScheduler taskScheduler, TimeSpan? defaultTimeout, CancellationToken cancellationToken = default)
        {
            _workQueue = new ConcurrentWorkQueue();
            _stopCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _taskFactory = new TaskFactory(taskScheduler);
            _workers = new Dictionary<int, Task>();
            _workQueueListenerTask = _taskFactory.StartNew(WorkQueueListenerAsync, TaskCreationOptions.LongRunning).Result;
            _waitTasks = new ConcurrentDictionary<ExecuteWorkItem, Task>();
            _defaultTimeout = defaultTimeout;
            _activeWorkSemaphore = new SemaphoreSlim(maxLevelOfConcurrency);
            _runningTasks = new ConcurrentDictionary<long, Lazy<Task>>();
            _debounceWork = new ConcurrentDictionary<int, DebounceItem>();
            _debounceWorkSemaphore = new SemaphoreSlim(1);
        }

        private async Task WorkQueueListenerAsync()
        {
            try
            {
                while (!_stopCts.IsCancellationRequested)
                {
                    ExecuteWorkItem workItem = await _workQueue.AwaitWorkAsync(_stopCts.Token).ConfigureAwait(false);
                    if (!workItem.UnitOfWork.DebounceTime.HasValue)
                    {
                        await ScheduleWorkAsync(workItem).ConfigureAwait(false);
                    }
                    else
                    {
                        await ScheduleDebounceAsync(workItem).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception e)
            {
                Debugger.Break();
            }
        }

        private async Task ScheduleWorkAsync(ExecuteWorkItem workItem)
        {
            await _activeWorkSemaphore.WaitAsync(_stopCts.Token);
            long id = _workIdCounter == long.MaxValue ? 0 : ++_workIdCounter;

            Lazy<Task> task = new Lazy<Task>(() => _taskFactory.StartNew(() => HandleWorkAsync(id, workItem)));
            bool added = _runningTasks.TryAdd(id, new Lazy<Task>(() => _taskFactory.StartNew(() => HandleWorkAsync(id, workItem))));
            Debug.Assert(added);
            _ = task.Value;
        }

        private async Task HandleWorkAsync(long workerId, ExecuteWorkItem item)
        {
            List<IDisposable> disposables = new List<IDisposable>();
            //Debug.WriteLine($"Worker {workerId}: Executing workitem: '{item.UnitOfWork.Name}'");
            try
            {
                CancellationToken cancellationToken;
                if (_defaultTimeout.HasValue)
                {
                    CancellationTokenSource timeoutSource = new CancellationTokenSource(_defaultTimeout.Value);
                    CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, _stopCts.Token);
                    disposables.AddRange([timeoutSource, linkedSource]);
                    cancellationToken = linkedSource.Token;
                }
                else
                {
                    cancellationToken = _stopCts.Token;
                }

                await item.UnitOfWork.ExecuteAsync(item, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e) when (e is TimeoutException or OperationCanceledException or TaskCanceledException)
            {
                item.SetCanceled();
            }
            catch (Exception e)
            {
                item.SetException(e);
            }
            finally
            {
                _activeWorkSemaphore.Release();
                disposables.ForEach(x => x.Dispose());
                bool removed = _runningTasks.Remove(workerId, out Lazy<Task>? _);
                Debug.Assert(removed);
            }
        }

        public async Task ScheduleDebounceAsync(ExecuteWorkItem executeWorkItem)
        {
            if (executeWorkItem.UnitOfWork.HashCode is null)
            {
                throw new InvalidOperationException("HashCode is null");
            }
            CancellationTokenSource timeoutCts = new CancellationTokenSource();
            if (_defaultTimeout.HasValue)
            {
                timeoutCts.CancelAfter(_defaultTimeout.Value);
            }

            CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, StopToken, executeWorkItem.UnitOfWork.CancellationToken);
            try
            { 
                await _debounceWorkSemaphore.WaitAsync(_stopCts.Token).ConfigureAwait(false);
                if (_debounceWork.TryGetValue(executeWorkItem.UnitOfWork.HashCode.Value, out DebounceItem? debounceItem))
                {
                    if (debounceItem.WorkItem.TimeStamp > executeWorkItem.TimeStamp)
                    {
                        executeWorkItem.SetCanceled();
                        return;
                    }
                    debounceItem.Update(executeWorkItem);
                }
                else
                {
                    bool debounceAdded = _debounceWork.TryAdd(executeWorkItem.UnitOfWork.HashCode.Value, new DebounceItem(executeWorkItem));
                    Debug.Assert(debounceAdded);
                }
                bool added = _waitTasks.TryAdd(executeWorkItem, WaitForDebounceAsync(executeWorkItem));
                //Debug.Assert(added);
            }
            catch (Exception e) when (e is TimeoutException or OperationCanceledException or TaskCanceledException)
            {
                executeWorkItem.SetCanceled();
            }
            catch (Exception e)
            {
                executeWorkItem.SetException(e);
            }
            finally
            {
                timeoutCts.Dispose();
                linkedSource.Dispose();
                _debounceWorkSemaphore.Release();
            }
        }

        private async Task WaitForDebounceAsync(ExecuteWorkItem executeWorkItem)
        {
            if (executeWorkItem.UnitOfWork.DebounceTime is null)
            {
                throw new InvalidOperationException("DebounceTime is null");
            }
            if (executeWorkItem.UnitOfWork.HashCode is null)
            {
                throw new InvalidOperationException("HashCode is null");
            }
            CancellationTokenSource timeoutCts = new CancellationTokenSource();
            if (_defaultTimeout.HasValue)
            {
                timeoutCts.CancelAfter(_defaultTimeout.Value);
            }
            CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, StopToken, executeWorkItem.UnitOfWork.CancellationToken);
            try
            {
                if (await executeWorkItem.UnitOfWork.WaitForNextExecutionAsync(executeWorkItem, DateTimeOffset.UtcNow.Add(executeWorkItem.UnitOfWork.DebounceTime.Value), linkedSource.Token).WaitAsync(linkedSource.Token).ConfigureAwait(false))
                {
                    await _debounceWorkSemaphore.WaitAsync(_stopCts.Token).ConfigureAwait(false);
                    if (_debounceWork.TryGetValue(executeWorkItem.UnitOfWork.HashCode.Value, out DebounceItem? item) && ReferenceEquals(item.WorkItem, executeWorkItem))
                    {
                        bool removed = _debounceWork.Remove(executeWorkItem.UnitOfWork.HashCode.Value, out DebounceItem? _);
                        Debug.Assert(removed);
                        await ScheduleWorkAsync(executeWorkItem).ConfigureAwait(false);
                    }
                    _debounceWorkSemaphore.Release();
                }
            }
            catch (Exception e) when (e is TimeoutException or OperationCanceledException or TaskCanceledException)
            {
                executeWorkItem.SetCanceled();
            }
            catch (Exception e)
            {
                executeWorkItem.SetException(e);
            }
            finally
            {
                timeoutCts.Dispose();
                linkedSource.Dispose();
                bool removed = _waitTasks.Remove(executeWorkItem, out Task? _);
                Debug.Assert(removed);
            }
        }

        public async ValueTask DisposeAsync()
        {
            _stopCts.Cancel();
            await Task.WhenAll(_runningTasks.Values.Select(x => x.Value).Concat(_waitTasks.Values));
            _stopCts.Dispose();
        }

        public void ScheduleWork(ExecuteWorkItem executeWorkItem, DateTimeOffset? dateTime)
        {
            if (dateTime.HasValue)
            {
                _waitTasks.TryAdd(executeWorkItem, WaitForNextExecutionAsync(executeWorkItem, dateTime.Value));
            }
            else if (executeWorkItem.UnitOfWork.DependsOn is IDependOnCondition dependOnCondition)
            {
                _waitTasks.TryAdd(executeWorkItem, WaitForDependsOnConditionAsync(executeWorkItem, dependOnCondition));
            }
            else
            {
                _workQueue.ScheduleWork(executeWorkItem);
            }
        }

        private async Task WaitForDependsOnConditionAsync(ExecuteWorkItem executeWorkItem, IDependOnCondition dependsOnCondition)
        {
            await dependsOnCondition.WaitForConditionAsync(_stopCts.Token);
            _workQueue.ScheduleWork(executeWorkItem);
        }

        private async Task WaitForNextExecutionAsync(ExecuteWorkItem executeWorkItem, DateTimeOffset dateTime)
        {
            try
            {
                if (await executeWorkItem.UnitOfWork.WaitForNextExecutionAsync(executeWorkItem, dateTime, _stopCts.Token).WaitAsync(_stopCts.Token))
                {
                    ScheduleWork(executeWorkItem, null);
                }
            }
            catch (Exception e) when (e is TimeoutException or OperationCanceledException or TaskCanceledException)
            {
                executeWorkItem.SetCanceled();
            }
            catch (Exception e)
            {
                executeWorkItem.SetException(e);
            }
            finally
            {
                bool removed = _waitTasks.Remove(executeWorkItem, out Task? _);
                //Debug.Assert(removed);
            }
        }
    }
}

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
        private Dictionary<int, DebounceItem> _debounceWork;
        private SemaphoreSlim _debounceWorkSemaphore;
        public AsyncWorkHandler(int maxLevelOfConcurrency, TaskScheduler taskScheduler, TimeSpan? defaultTimeout, CancellationToken cancellationToken = default)
        {
            _workQueue = new ConcurrentWorkQueue();
            _stopCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _taskFactory = new TaskFactory(taskScheduler);
            _workers = new Dictionary<int, Task>();
            _workQueueListenerTask = _taskFactory.StartNew(WorkQueueListenerAsync);
            _waitTasks = new ConcurrentDictionary<ExecuteWorkItem, Task>();
            _defaultTimeout = defaultTimeout;
            _activeWorkSemaphore = new SemaphoreSlim(maxLevelOfConcurrency);
            _runningTasks = new ConcurrentDictionary<long, Lazy<Task>>();
            _debounceWork = new Dictionary<int, DebounceItem>();
            _debounceWorkSemaphore = new SemaphoreSlim(1);
        }

        private async Task WorkQueueListenerAsync()
        {
            while (!_stopCts.IsCancellationRequested)
            {
                ExecuteWorkItem workItem = await _workQueue.AwaitWorkAsync(_stopCts.Token);
                if (!workItem.UnitOfWork.DebounceTime.HasValue)
                {
                    await ScheduleWorkAsync(workItem);
                }
                else
                {
                    await ScheduleDebounceAsync(workItem);
                }
            }
        }

        private async Task ScheduleWorkAsync(ExecuteWorkItem workItem)
        {
            await _activeWorkSemaphore.WaitAsync(_stopCts.Token);
            long id = _workIdCounter++;
            Lazy<Task> task = _runningTasks[id] = new Lazy<Task>(() => _taskFactory.StartNew(() => HandleWorkAsync(id, workItem), _stopCts.Token));
            _ = task.Value;
        }

        private async Task HandleWorkAsync(long workerId, ExecuteWorkItem item)
        {
            List<IDisposable> disposables = new List<IDisposable>();
            //Debug.WriteLine($"Worker {workerId}: Executing workitem: '{item.UnitOfWork.Name}', next count: '{item.UnitOfWork.Next.Count()}'");
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

                await item.UnitOfWork.ExecuteAsync(item).WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                item.SetCanceled();
            }
            catch (OperationCanceledException)
            {
                item.SetCanceled();
            }
            if (!_runningTasks.Remove(workerId, out Lazy<Task>? _))
            {
                Console.WriteLine();
            }
            _activeWorkSemaphore.Release();
            disposables.ForEach(x => x.Dispose());
        }

        public async Task ScheduleDebounceAsync(ExecuteWorkItem executeWorkItem)
        {
            CancellationTokenSource timeoutCts = new CancellationTokenSource();
            if (_defaultTimeout.HasValue)
            {
                timeoutCts.CancelAfter(_defaultTimeout.Value);
            }
            CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, StopToken, executeWorkItem.UnitOfWork.CancellationToken);

            if (executeWorkItem.UnitOfWork.HashCode is null)
            {
                throw new InvalidOperationException("HashCode is null");
            }
            await _debounceWorkSemaphore.WaitAsync();
            try
            { 
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
                    _debounceWork.Add(executeWorkItem.UnitOfWork.HashCode.Value, new DebounceItem(executeWorkItem));
                }
                _waitTasks.TryAdd(executeWorkItem, WaitForDebounceAsync(executeWorkItem));
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
            try
            {
                if (await executeWorkItem.UnitOfWork.WaitForNextExecutionAsync(executeWorkItem, DateTime.UtcNow.Add(executeWorkItem.UnitOfWork.DebounceTime.Value), _stopCts.Token).WaitAsync(_stopCts.Token))
                {
                    await _debounceWorkSemaphore.WaitAsync();
                    if (_debounceWork.TryGetValue(executeWorkItem.UnitOfWork.HashCode.Value, out DebounceItem? item) && ReferenceEquals(item.WorkItem, executeWorkItem))
                    {
                        _debounceWork.Remove(executeWorkItem.UnitOfWork.HashCode.Value);
                        await ScheduleWorkAsync(executeWorkItem);
                    }
                    _debounceWorkSemaphore.Release();
                    
                }
            }
            finally
            {
                _waitTasks.Remove(executeWorkItem, out Task? _);
            }
        }

        public async ValueTask DisposeAsync()
        {
            _stopCts.Cancel();
            await Task.WhenAll(_runningTasks.Values.Select(x => x.Value).Concat(_waitTasks.Values));
            _stopCts.Dispose();
        }

        public void ScheduleWork(ExecuteWorkItem executeWorkItem, DateTime? dateTime)
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

        private async Task WaitForNextExecutionAsync(ExecuteWorkItem executeWorkItem, DateTime dateTime)
        {
            try
            {
                if (await executeWorkItem.UnitOfWork.WaitForNextExecutionAsync(executeWorkItem, dateTime, _stopCts.Token).WaitAsync(_stopCts.Token))
                {
                    ScheduleWork(executeWorkItem, null);
                }
            }
            finally
            {
                _waitTasks.TryRemove(executeWorkItem, out Task? _);
            }
        }
    }
}

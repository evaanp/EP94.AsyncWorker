using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Internal.Models;
using EP94.AsyncWorker.Internal.Utils;
using EP94.AsyncWorker.Public.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EP94.AsyncWorker.Internal
{
    internal class AsyncWorkHandler : IWorkScheduler, IAsyncDisposable
    {
        private Task _queueBusyListenerTask;
        //private Task[] _workers;
        private ConcurrentDictionary<IUnitOfWork, Task> _waitTasks;
        private ConcurrentWorkQueue _workQueue;
        private CancellationTokenSource _stopCts;
        private TimeSpan? _defaultTimeout;
        private TaskFactory _taskFactory;
        private Dictionary<int, Task> _workers;
        private SemaphoreSlim _activeWorkSemaphore;
        private ConcurrentDictionary<long, Lazy<Task>> _runningTasks;
        private long _workIdCounter;
        public AsyncWorkHandler(int maxLevelOfConcurrency, TaskScheduler taskScheduler, TimeSpan? defaultTimeout, CancellationToken cancellationToken = default)
        {
            //_workQueue = new ConcurrentWorkQueue(maxLevelOfConcurrency);
            _workQueue = new ConcurrentWorkQueue(1);
            _stopCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _taskFactory = new TaskFactory(taskScheduler);
            _workers = new Dictionary<int, Task>(maxLevelOfConcurrency);
            _queueBusyListenerTask = _taskFactory.StartNew(QueueBusyListenerAsync);
            _waitTasks = new ConcurrentDictionary<IUnitOfWork, Task>();
            _defaultTimeout = defaultTimeout;
            _activeWorkSemaphore = new SemaphoreSlim(maxLevelOfConcurrency);
            _runningTasks = new ConcurrentDictionary<long, Lazy<Task>>();
        }

        private async Task QueueBusyListenerAsync()
        {
            while (!_stopCts.IsCancellationRequested)
            {
                await _workQueue.AwaitWorkAsync(_stopCts.Token);
                await _activeWorkSemaphore.WaitAsync(_stopCts.Token);
                long id = _workIdCounter++;
                Lazy<Task> task = _runningTasks[id] = new Lazy<Task>(() => _taskFactory.StartNew(() => HandleWorkAsync(id), _stopCts.Token));
                _ = task.Value;
            }
        }

        private async Task HandleWorkAsync(long workerId)
        {
            List<IDisposable> disposables = new List<IDisposable>();
            if (_workQueue.TryDequeue(out ExecuteWorkItem? item))
            {
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

                    await item.UnitOfWork.ExecuteAsync(item.ExecutionStack).WaitAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    item.UnitOfWork.SetCanceled();
                }
                catch (OperationCanceledException)
                {
                    item.UnitOfWork.SetCanceled();
                }
            }
            if (!_runningTasks.Remove(workerId, out Lazy<Task>? _))
            {
                Console.WriteLine();
            }
            _activeWorkSemaphore.Release();
            disposables.ForEach(x => x.Dispose());
            
        }

        public async ValueTask DisposeAsync()
        {
            _stopCts.Cancel();
            await Task.WhenAll(_runningTasks.Values.Select(x => x.Value).Concat(_waitTasks.Values));
            _stopCts.Dispose();
        }

        public void ScheduleWork(IUnitOfWork unitOfWork, DateTime? dateTime, ExecutionStack executionStack)
        {
            if (dateTime.HasValue)
            {
                _waitTasks.TryAdd(unitOfWork, WaitForNextExecutionAsync(unitOfWork, dateTime.Value, executionStack));
            }
            else
            {
                _workQueue.ScheduleWork(unitOfWork, executionStack);
            }
        }

        private async Task WaitForNextExecutionAsync(IUnitOfWork unitOfWork, DateTime dateTime, ExecutionStack executionStack)
        {
            await unitOfWork.WaitForNextExecutionAsync(dateTime, _stopCts.Token).WaitAsync(_stopCts.Token);
            ScheduleWork(unitOfWork, null, executionStack);
            _waitTasks.TryRemove(unitOfWork, out Task? _);
        }
    }
}

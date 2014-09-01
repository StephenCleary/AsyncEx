using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using C5;
using Comparers;
using System.Runtime.CompilerServices;

namespace Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class AsyncPriorityLockExample
    {
        public sealed class PriorityWaitQueue<T> : IAsyncWaitQueue<T>
        {
            private sealed class Entry
            {
                public int Priority { get; set; }
                public PriorityTask Task { get; set; }
            }

            private readonly object _mutex = new object();
            private IntervalHeap<Entry> _queue = new IntervalHeap<Entry>(Compare<Entry>.OrderBy(x => x.Priority));

            public PriorityWaitQueue()
            {
                RestartQueue();
            }

            private void RestartQueue()
            {
                _queue = new IntervalHeap<Entry>(Compare<Entry>.OrderBy(x => x.Priority));
            }

            bool IAsyncWaitQueue<T>.IsEmpty
            {
                get { lock (_mutex) { return _queue.IsEmpty; } }
            }

            internal int NextPriority { get; set; }

            Task<T> IAsyncWaitQueue<T>.Enqueue()
            {
                IPriorityQueueHandle<Entry> handle = null;
                var entry = new Entry { Priority = NextPriority, Task = new PriorityTask() };
                NextPriority = 0;
                lock (_mutex)
                    _queue.Add(ref handle, entry);
                entry.Task.Handle = handle;
                return entry.Task;
            }

            IDisposable IAsyncWaitQueue<T>.Dequeue(T result)
            {
                Entry ret;
                lock (_mutex)
                    ret = _queue.DeleteMax();
                return new CompleteDisposable(result, ret.Task);
            }

            IDisposable IAsyncWaitQueue<T>.DequeueAll(T result)
            {
                PriorityTask[] entries = null;
                lock (_mutex)
                {
                    entries = _queue.Select(x => x.Task).ToArray();
                    RestartQueue();
                }
                return new CompleteDisposable(result, entries);
            }

            IDisposable IAsyncWaitQueue<T>.TryCancel(Task task)
            {
                var priorityTask = task as PriorityTask;
                if (priorityTask == null)
                    return new CancelDisposable();
                Entry entry = null;
                try
                {
                    lock (_mutex)
                        entry = _queue.Delete(priorityTask.Handle);
                }
                catch (C5.InvalidPriorityQueueHandleException)
                {
                    return new CancelDisposable();
                }
                return new CancelDisposable(entry.Task);
            }

            IDisposable IAsyncWaitQueue<T>.CancelAll()
            {
                PriorityTask[] entries = null;
                lock (_mutex)
                {
                    entries = _queue.Select(x => x.Task).ToArray();
                    RestartQueue();
                }
                return new CancelDisposable(entries);
            }

            public bool TryChangePriority(Task task, int priority)
            {
                var priorityTask = task as PriorityTask;
                if (priorityTask == null)
                    return false;
                var entry = new Entry { Priority = priority, Task = priorityTask };
                try
                {
                    lock (_mutex)
                        _queue.Replace(priorityTask.Handle, entry);
                }
                catch (C5.InvalidPriorityQueueHandleException)
                {
                    return false;
                }
                return true;
            }

            private sealed class CancelDisposable : IDisposable
            {
                private readonly PriorityTask[] _tasks;

                public CancelDisposable(params PriorityTask[] tasks)
                {
                    _tasks = tasks;
                }

                public void Dispose()
                {
                    foreach (var task in _tasks)
                        task.TrySetCanceled();
                }
            }

            private sealed class CompleteDisposable : IDisposable
            {
                private readonly PriorityTask[] _tasks;
                private readonly T _result;

                public CompleteDisposable(T result, params PriorityTask[] tasks)
                {
                    _result = result;
                    _tasks = tasks;
                }

                public void Dispose()
                {
                    foreach (var task in _tasks)
                        task.TrySetResult(_result);
                }
            }

            private sealed class PriorityTask : TaskBaseWithCompletion<T>
            {
                internal IPriorityQueueHandle<Entry> Handle { get; set; }

                internal void TrySetResult(T result)
                {
                    TaskCompletionSource.TrySetResult(result);
                    EnsureCompleted();
                }

                internal void TrySetCanceled()
                {
                    TaskCompletionSource.TrySetCanceled();
                    EnsureCompleted();
                }
            }
        }

        public sealed class AsyncPriorityLock
        {
            private readonly PriorityWaitQueue<IDisposable> _queue;
            private readonly AsyncLock _mutex;

            public AsyncPriorityLock()
            {
                _queue = new PriorityWaitQueue<IDisposable>();
                _mutex = new AsyncLock(_queue);
            }

            public LockRequest LockAsync(int priority = 0, CancellationToken token = new CancellationToken())
            {
                lock (_queue)
                {
                    _queue.NextPriority = priority;
                    var ret = _mutex.LockAsync(token).AsTask();
                    return new LockRequest(this, ret, priority);
                }
            }

            public sealed class LockRequest
            {
                private readonly AsyncPriorityLock _mutex;
                private readonly Task<IDisposable> _key;
                private int _priority;

                internal LockRequest(AsyncPriorityLock mutex, Task<IDisposable> key, int priority)
                {
                    _mutex = mutex;
                    _key = key;
                    _priority = priority;
                }

                public int Priority
                {
                    get { return _priority; }

                    set
                    {
                        if (_mutex._queue.TryChangePriority(_key, value))
                            _priority = value;
                    }
                }

                public TaskAwaiter<IDisposable> GetAwaiter()
                {
                    return _key.GetAwaiter();
                }

                public Task<IDisposable> AsTask()
                {
                    return _key;
                }
            }
        }

        [TestMethod]
        public async Task AsyncLock_IsFIFO()
        {
            var mutex = new AsyncLock();
            var key = await mutex.LockAsync();
            var wait1 = mutex.LockAsync().AsTask();
            var wait2 = mutex.LockAsync().AsTask();

            key.Dispose();

            var releasedWaiter = await Task.WhenAny(wait1, wait2);
            Assert.AreEqual(wait1, releasedWaiter);
        }

        [TestMethod]
        public async Task PriorityLock_IsByPriority()
        {
            var mutex = new AsyncPriorityLock();
            var key = await mutex.LockAsync();
            var wait1 = mutex.LockAsync(1);
            var wait2 = mutex.LockAsync(2);

            key.Dispose();

            var releasedWaiter = await Task.WhenAny(wait1.AsTask(), wait2.AsTask());
            Assert.AreEqual(wait2.AsTask(), releasedWaiter);
        }

        [TestMethod]
        public async Task PriorityLock_CanChangePriority()
        {
            var mutex = new AsyncPriorityLock();
            var key = await mutex.LockAsync();
            var wait1 = mutex.LockAsync(1);
            var wait2 = mutex.LockAsync(2);

            wait1.Priority = 3;
            key.Dispose();

            var releasedWaiter = await Task.WhenAny(wait1.AsTask(), wait2.AsTask());
            Assert.AreEqual(wait1.AsTask(), releasedWaiter);
        }
    }
}

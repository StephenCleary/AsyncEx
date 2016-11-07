using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// A collection of cancelable <see cref="TaskCompletionSource{T}"/> instances. Implementations must be threadsafe <b>and</b> must work correctly if the caller is holding a lock.
    /// </summary>
    /// <typeparam name="T">The type of the results. If this isn't needed, use <see cref="Object"/>.</typeparam>
    public interface IAsyncWaitQueue<T>
    {
        /// <summary>
        /// Gets whether the queue is empty.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Creates a new entry and queues it to this wait queue. The returned task must support both synchronous and asynchronous waits.
        /// </summary>
        /// <returns>The queued task.</returns>
        Task<T> Enqueue();

        /// <summary>
        /// Removes a single entry in the wait queue. Returns a disposable that completes that entry.
        /// </summary>
        /// <param name="result">The result used to complete the wait queue entry. If this isn't needed, use <c>default(T)</c>.</param>
        IDisposable Dequeue(T result = default(T));

        /// <summary>
        /// Removes all entries in the wait queue. Returns a disposable that completes all entries.
        /// </summary>
        /// <param name="result">The result used to complete the wait queue entries. If this isn't needed, use <c>default(T)</c>.</param>
        IDisposable DequeueAll(T result = default(T));

        /// <summary>
        /// Attempts to remove an entry from the wait queue. Returns a disposable that cancels the entry.
        /// </summary>
        /// <param name="task">The task to cancel.</param>
        /// <returns>A value indicating whether the entry was found and canceled.</returns>
        IDisposable TryCancel(Task task);

        /// <summary>
        /// Removes all entries from the wait queue. Returns a disposable that cancels all entries.
        /// </summary>
        IDisposable CancelAll();
    }

    /// <summary>
    /// Provides extension methods for wait queues.
    /// </summary>
    public static class AsyncWaitQueueExtensions
    {
        /// <summary>
        /// Creates a new entry and queues it to this wait queue. If the cancellation token is already canceled, this method immediately returns a canceled task without modifying the wait queue.
        /// </summary>
        /// <param name="this">The wait queue.</param>
        /// <param name="token">The token used to cancel the wait.</param>
        /// <returns>The queued task.</returns>
        [Obsolete("Use the Enqueue overload that takes a synchronization object.")]
        public static Task<T> Enqueue<T>(this IAsyncWaitQueue<T> @this, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return TaskConstants<T>.Canceled;

            var ret = @this.Enqueue();
            if (!token.CanBeCanceled)
                return ret;

            var registration = token.Register(() => @this.TryCancel(ret).Dispose(), useSynchronizationContext: false);
            ret.ContinueWith(_ => registration.Dispose(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            return ret;
        }

        /// <summary>
        /// Creates a new entry and queues it to this wait queue. If the cancellation token is already canceled, this method immediately returns a canceled task without modifying the wait queue.
        /// </summary>
        /// <param name="this">The wait queue.</param>
        /// <param name="syncObject">A synchronization object taken while cancelling the entry.</param>
        /// <param name="token">The token used to cancel the wait.</param>
        /// <returns>The queued task.</returns>
        public static Task<T> Enqueue<T>(this IAsyncWaitQueue<T> @this, object syncObject, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return TaskConstants<T>.Canceled;

            var ret = @this.Enqueue();
            if (!token.CanBeCanceled)
                return ret;

            var registration = token.Register(() =>
            {
                IDisposable finish;
                lock (syncObject)
                    finish = @this.TryCancel(ret);
                finish.Dispose();
            }, useSynchronizationContext: false);
            ret.ContinueWith(_ => registration.Dispose(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            return ret;
        }
    }

    /// <summary>
    /// The default wait queue implementation, which uses a double-ended queue.
    /// </summary>
    /// <typeparam name="T">The type of the results. If this isn't needed, use <see cref="Object"/>.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(DefaultAsyncWaitQueue<>.DebugView))]
    public sealed class DefaultAsyncWaitQueue<T> : IAsyncWaitQueue<T>
    {
        private readonly Deque<TaskCompletionSource<T>> _queue = new Deque<TaskCompletionSource<T>>();

        private int Count
        {
            get { lock (_queue) { return _queue.Count; } }
        }

        bool IAsyncWaitQueue<T>.IsEmpty
        {
            get { return Count == 0; }
        }

        Task<T> IAsyncWaitQueue<T>.Enqueue()
        {
            var tcs = new TaskCompletionSource<T>();
            lock (_queue)
                _queue.AddToBack(tcs);
            return tcs.Task;
        }

        IDisposable IAsyncWaitQueue<T>.Dequeue(T result)
        {
            TaskCompletionSource<T> tcs;
            lock (_queue)
                tcs = _queue.RemoveFromFront();
            return new CompleteDisposable(result, tcs);
        }

        IDisposable IAsyncWaitQueue<T>.DequeueAll(T result)
        {
            TaskCompletionSource<T>[] taskCompletionSources;
            lock (_queue)
            {
                taskCompletionSources = _queue.ToArray();
                _queue.Clear();
            }
            return new CompleteDisposable(result, taskCompletionSources);
        }

        IDisposable IAsyncWaitQueue<T>.TryCancel(Task task)
        {
            TaskCompletionSource<T> tcs = null;
            lock (_queue)
            {
                for (int i = 0; i != _queue.Count; ++i)
                {
                    if (_queue[i].Task == task)
                    {
                        tcs = _queue[i];
                        _queue.RemoveAt(i);
                        break;
                    }
                }
            }
            if (tcs == null)
                return new CancelDisposable();
            return new CancelDisposable(tcs);
        }

        IDisposable IAsyncWaitQueue<T>.CancelAll()
        {
            TaskCompletionSource<T>[] taskCompletionSources;
            lock (_queue)
            {
                taskCompletionSources = _queue.ToArray();
                _queue.Clear();
            }
            return new CancelDisposable(taskCompletionSources);
        }

        private sealed class CancelDisposable : IDisposable
        {
            private readonly TaskCompletionSource<T>[] _taskCompletionSources;

            public CancelDisposable(params TaskCompletionSource<T>[] taskCompletionSources)
            {
                _taskCompletionSources = taskCompletionSources;
            }

            public void Dispose()
            {
                foreach (var cts in _taskCompletionSources)
                    cts.TrySetCanceledWithBackgroundContinuations();
            }
        }

        private sealed class CompleteDisposable : IDisposable
        {
            private readonly TaskCompletionSource<T>[] _taskCompletionSources;
            private readonly T _result;

            public CompleteDisposable(T result, params TaskCompletionSource<T>[] taskCompletionSources)
            {
                _result = result;
                _taskCompletionSources = taskCompletionSources;
            }

            public void Dispose()
            {
                foreach (var cts in _taskCompletionSources)
                    cts.TrySetResultWithBackgroundContinuations(_result);
            }
        }

        [DebuggerNonUserCode]
        internal sealed class DebugView
        {
            private readonly DefaultAsyncWaitQueue<T> _queue;

            public DebugView(DefaultAsyncWaitQueue<T> queue)
            {
                _queue = queue;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Task<T>[] Tasks
            {
                get { return _queue._queue.Select(x => x.Task).ToArray(); }
            }
        }
    }
}

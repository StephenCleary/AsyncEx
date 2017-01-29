using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;

namespace Nito.AsyncEx
{
    /// <summary>
    /// An async-compatible condition variable. This type uses Mesa-style semantics (the notifying tasks do not yield).
    /// </summary>
    [DebuggerDisplay("Id = {Id}, AsyncLockId = {_asyncLock.Id}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed class AsyncConditionVariable
    {
        /// <summary>
        /// The lock associated with this condition variable.
        /// </summary>
        private readonly AsyncLock _asyncLock;

        /// <summary>
        /// The queue of waiting tasks.
        /// </summary>
        private readonly IAsyncWaitQueue<object> _queue;

        /// <summary>
        /// The semi-unique identifier for this instance. This is 0 if the id has not yet been created.
        /// </summary>
        private int _id;

        /// <summary>
        /// The object used for mutual exclusion.
        /// </summary>
        private readonly object _mutex;

        /// <summary>
        /// Creates an async-compatible condition variable associated with an async-compatible lock.
        /// </summary>
        /// <param name="asyncLock">The lock associated with this condition variable.</param>
        /// <param name="queue">The wait queue used to manage waiters. This may be <c>null</c> to use a default (FIFO) queue.</param>
        public AsyncConditionVariable(AsyncLock asyncLock, IAsyncWaitQueue<object> queue)
        {
            _asyncLock = asyncLock;
            _queue = queue ?? new DefaultAsyncWaitQueue<object>();
            _mutex = new object();
        }

        /// <summary>
        /// Creates an async-compatible condition variable associated with an async-compatible lock.
        /// </summary>
        /// <param name="asyncLock">The lock associated with this condition variable.</param>
        public AsyncConditionVariable(AsyncLock asyncLock)
            : this(asyncLock, null)
        {
        }

        /// <summary>
        /// Gets a semi-unique identifier for this asynchronous condition variable.
        /// </summary>
        public int Id
        {
            get { return IdManager<AsyncConditionVariable>.GetId(ref _id); }
        }

        /// <summary>
        /// Sends a signal to a single task waiting on this condition variable. The associated lock MUST be held when calling this method, and it will still be held when this method returns.
        /// </summary>
        public void Notify()
        {
            lock (_mutex)
            {
                if (!_queue.IsEmpty)
                    _queue.Dequeue();
            }
        }

        /// <summary>
        /// Sends a signal to all tasks waiting on this condition variable. The associated lock MUST be held when calling this method, and it will still be held when this method returns.
        /// </summary>
        public void NotifyAll()
        {
            lock (_mutex)
            {
                _queue.DequeueAll();
            }
        }

        /// <summary>
        /// Asynchronously waits for a signal on this condition variable. The associated lock MUST be held when calling this method, and it will still be held when this method returns, even if the method is cancelled.
        /// </summary>
        /// <param name="cancellationToken">The cancellation signal used to cancel this wait.</param>
        public Task WaitAsync(CancellationToken cancellationToken)
        {
            Task task;
            lock (_mutex)
            {
                // Begin waiting for either a signal or cancellation.
                task = _queue.Enqueue(_mutex, cancellationToken);

                // Attach to the signal or cancellation.
                var ret = WaitAndRetakeLockAsync(task, _asyncLock);

                // Release the lock while we are waiting.
                _asyncLock.ReleaseLock();

                return ret;
            }
        }

        private static async Task WaitAndRetakeLockAsync(Task task, AsyncLock asyncLock)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            finally
            {
                // Re-take the lock.
                await asyncLock.LockAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Asynchronously waits for a signal on this condition variable. The associated lock MUST be held when calling this method, and it will still be held when the returned task completes.
        /// </summary>
        public Task WaitAsync()
        {
            return WaitAsync(CancellationToken.None);
        }

        /// <summary>
        /// Synchronously waits for a signal on this condition variable. This method may block the calling thread. The associated lock MUST be held when calling this method, and it will still be held when this method returns, even if the method is cancelled.
        /// </summary>
        /// <param name="cancellationToken">The cancellation signal used to cancel this wait.</param>
        public void Wait(CancellationToken cancellationToken)
        {
            WaitAsync(cancellationToken).WaitAndUnwrapException();
        }

        /// <summary>
        /// Synchronously waits for a signal on this condition variable. This method may block the calling thread. The associated lock MUST be held when calling this method, and it will still be held when this method returns.
        /// </summary>
        public void Wait()
        {
            Wait(CancellationToken.None);
        }

        // ReSharper disable UnusedMember.Local
        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly AsyncConditionVariable _cv;

            public DebugView(AsyncConditionVariable cv)
            {
                _cv = cv;
            }

            public int Id { get { return _cv.Id; } }

            public AsyncLock AsyncLock { get { return _cv._asyncLock; } }

            public IAsyncWaitQueue<object> WaitQueue { get { return _cv._queue; } }
        }
        // ReSharper restore UnusedMember.Local
    }
}

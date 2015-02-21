using Nito.AsyncEx.Internal;
using Nito.AsyncEx.Internal.PlatformEnlightenment;
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
        /// <param name="queue">The wait queue used to manage waiters.</param>
        public AsyncConditionVariable(AsyncLock asyncLock, IAsyncWaitQueue<object> queue)
        {
            _asyncLock = asyncLock;
            _queue = queue;
            _mutex = new object();
        }

        /// <summary>
        /// Creates an async-compatible condition variable associated with an async-compatible lock.
        /// </summary>
        /// <param name="asyncLock">The lock associated with this condition variable.</param>
        public AsyncConditionVariable(AsyncLock asyncLock)
            : this(asyncLock, new DefaultAsyncWaitQueue<object>())
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
            IDisposable finish = null;
            lock (_mutex)
            {
                //Enlightenment.Trace.AsyncConditionVariable_NotifyOne(this, _asyncLock);
                if (!_queue.IsEmpty)
                    finish = _queue.Dequeue();
            }
            if (finish != null)
                finish.Dispose();
        }

        /// <summary>
        /// Sends a signal to all tasks waiting on this condition variable. The associated lock MUST be held when calling this method, and it will still be held when this method returns.
        /// </summary>
        public void NotifyAll()
        {
            IDisposable finish;
            lock (_mutex)
            {
                //Enlightenment.Trace.AsyncConditionVariable_NotifyAll(this, _asyncLock);
                finish = _queue.DequeueAll();
            }
            finish.Dispose();
        }

        /// <summary>
        /// Asynchronously waits for a signal on this condition variable. The associated lock MUST be held when calling this method, and it will still be held when this method returns, even if the method is cancelled.
        /// </summary>
        /// <param name="cancellationToken">The cancellation signal used to cancel this wait.</param>
        public Task WaitAsync(CancellationToken cancellationToken)
        {
            lock (_mutex)
            {
                // Begin waiting for either a signal or cancellation.
                var task = _queue.Enqueue(_mutex, cancellationToken);

                // Attach to the signal or cancellation.
                var retTcs = new TaskCompletionSource();
                task.ContinueWith(async t =>
                {
                    // Re-take the lock.
                    await _asyncLock.LockAsync().ConfigureAwait(false);

                    // Propagate the cancellation exception if necessary.
                    retTcs.TryCompleteFromCompletedTask(t);
                }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

                var ret = retTcs.Task;
                //Enlightenment.Trace.AsyncConditionVariable_TrackWait(this, _asyncLock, task, ret);

                // Release the lock while we are waiting.
                _asyncLock.ReleaseLock();

                return ret;
            }
        }

        /// <summary>
        /// Synchronously waits for a signal on this condition variable. This method may block the calling thread. The associated lock MUST be held when calling this method, and it will still be held when this method returns, even if the method is cancelled.
        /// </summary>
        /// <param name="cancellationToken">The cancellation signal used to cancel this wait.</param>
        public void Wait(CancellationToken cancellationToken)
        {
            Task enqueuedTask;
            lock (_mutex)
            {
                // Begin waiting for either a signal or cancellation.
                enqueuedTask = _queue.Enqueue(_mutex, cancellationToken);
            }

            // Release the lock while we are waiting.
            _asyncLock.ReleaseLock();

            // Wait for the signal or cancellation.
            enqueuedTask.WaitWithoutException();

            // Re-take the lock.
            _asyncLock.Lock();

            // Propagate the cancellation exception if necessary.
            enqueuedTask.WaitAndUnwrapException();
        }

        /// <summary>
        /// Asynchronously waits for a signal on this condition variable. The associated lock MUST be held when calling this method, and it will still be held when this method returns.
        /// </summary>
        public Task WaitAsync()
        {
            return WaitAsync(CancellationToken.None);
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

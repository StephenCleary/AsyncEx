using Nito.AsyncEx.Internal;
using Nito.AsyncEx.Internal.PlatformEnlightenment;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;

// Original idea from Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266988.aspx

namespace Nito.AsyncEx
{
    /// <summary>
    /// A mutual exclusion lock that is compatible with async. Note that this lock is <b>not</b> recursive!
    /// </summary>
    [DebuggerDisplay("Id = {Id}, Taken = {_taken}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed class AsyncLock
    {
        /// <summary>
        /// Whether the lock is taken by a task.
        /// </summary>
        private bool _taken;

        /// <summary>
        /// The queue of TCSs that other tasks are awaiting to acquire the lock.
        /// </summary>
        private readonly IAsyncWaitQueue<IDisposable> _queue;

        /// <summary>
        /// A task that is completed with the key object for this lock.
        /// </summary>
        private readonly Task<IDisposable> _cachedKeyTask;

        /// <summary>
        /// The semi-unique identifier for this instance. This is 0 if the id has not yet been created.
        /// </summary>
        private int _id;

        /// <summary>
        /// The object used for mutual exclusion.
        /// </summary>
        private readonly object _mutex;

        /// <summary>
        /// Creates a new async-compatible mutual exclusion lock.
        /// </summary>
        public AsyncLock()
            :this(new DefaultAsyncWaitQueue<IDisposable>())
        {
        }

        /// <summary>
        /// Creates a new async-compatible mutual exclusion lock using the specified wait queue.
        /// </summary>
        /// <param name="queue">The wait queue used to manage waiters.</param>
        public AsyncLock(IAsyncWaitQueue<IDisposable> queue)
        {
            _queue = queue;
            _cachedKeyTask = TaskShim.FromResult<IDisposable>(new Key(this));
            _mutex = new object();
        }

        /// <summary>
        /// Gets a semi-unique identifier for this asynchronous lock.
        /// </summary>
        public int Id
        {
            get { return IdManager<AsyncLock>.GetId(ref _id); }
        }

        /// <summary>
        /// Asynchronously acquires the lock. Returns a disposable that releases the lock when disposed.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public AwaitableDisposable<IDisposable> LockAsync(CancellationToken cancellationToken)
        {
            Task<IDisposable> ret;
            lock (_mutex)
            {
                if (!_taken)
                {
                    // If the lock is available, take it immediately.
                    _taken = true;
                    ret = _cachedKeyTask;
                }
                else
                {
                    // Wait for the lock to become available or cancellation.
                    ret = _queue.Enqueue(_mutex, cancellationToken);
                }

                //Enlightenment.Trace.AsyncLock_TrackLock(this, ret);
            }

            return new AwaitableDisposable<IDisposable>(ret);
        }

        /// <summary>
        /// Synchronously acquires the lock. Returns a disposable that releases the lock when disposed. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
        public IDisposable Lock(CancellationToken cancellationToken)
        {
            Task<IDisposable> enqueuedTask;
            lock (_mutex)
            {
                if (!_taken)
                {
                    _taken = true;
                    return _cachedKeyTask.Result;
                }

                enqueuedTask = _queue.Enqueue(_mutex, cancellationToken);
            }

            return enqueuedTask.WaitAndUnwrapException();
        }

        /// <summary>
        /// Asynchronously acquires the lock. Returns a disposable that releases the lock when disposed.
        /// </summary>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public AwaitableDisposable<IDisposable> LockAsync()
        {
            return LockAsync(CancellationToken.None);
        }

        /// <summary>
        /// Synchronously acquires the lock. Returns a disposable that releases the lock when disposed. This method may block the calling thread.
        /// </summary>
        public IDisposable Lock()
        {
            return Lock(CancellationToken.None);
        }

        /// <summary>
        /// Releases the lock.
        /// </summary>
        internal void ReleaseLock()
        {
            IDisposable finish = null;
            lock (_mutex)
            {
                //Enlightenment.Trace.AsyncLock_Unlocked(this);
                if (_queue.IsEmpty)
                    _taken = false;
                else
                    finish = _queue.Dequeue(_cachedKeyTask.Result);
            }
            if (finish != null)
                finish.Dispose();
        }

        /// <summary>
        /// The disposable which releases the lock.
        /// </summary>
        private sealed class Key : IDisposable
        {
            /// <summary>
            /// The lock to release.
            /// </summary>
            private readonly AsyncLock _asyncLock;

            /// <summary>
            /// Creates the key for a lock.
            /// </summary>
            /// <param name="asyncLock">The lock to release. May not be <c>null</c>.</param>
            public Key(AsyncLock asyncLock)
            {
                _asyncLock = asyncLock;
            }

            /// <summary>
            /// Release the lock.
            /// </summary>
            public void Dispose()
            {
                _asyncLock.ReleaseLock();
            }
        }

        // ReSharper disable UnusedMember.Local
        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly AsyncLock _mutex;

            public DebugView(AsyncLock mutex)
            {
                _mutex = mutex;
            }

            public int Id { get { return _mutex.Id; } }

            public bool Taken { get { return _mutex._taken; } }

            public IAsyncWaitQueue<IDisposable> WaitQueue { get { return _mutex._queue; } }
        }
        // ReSharper restore UnusedMember.Local
    }
}

using Nito.AsyncEx.Internal;
using Nito.AsyncEx.Internal.PlatformEnlightenment;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;

// Original idea from Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266983.aspx

namespace Nito.AsyncEx
{
    /// <summary>
    /// An async-compatible semaphore. Alternatively, you could use <c>SemaphoreSlim</c> on .NET 4.5 / Windows Store.
    /// </summary>
    [DebuggerDisplay("Id = {Id}, CurrentCount = {_count}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed class AsyncSemaphore
    {
        /// <summary>
        /// The queue of TCSs that other tasks are awaiting to acquire the semaphore.
        /// </summary>
        private readonly IAsyncWaitQueue<object> _queue;

        /// <summary>
        /// The number of waits that will be immediately granted.
        /// </summary>
        private int _count;

        /// <summary>
        /// The semi-unique identifier for this instance. This is 0 if the id has not yet been created.
        /// </summary>
        private int _id;

        /// <summary>
        /// The object used for mutual exclusion.
        /// </summary>
        private readonly object _mutex;

        /// <summary>
        /// Creates a new async-compatible semaphore with the specified initial count.
        /// </summary>
        /// <param name="initialCount">The initial count for this semaphore. This must be greater than or equal to zero.</param>
        /// <param name="queue">The wait queue used to manage waiters.</param>
        public AsyncSemaphore(int initialCount, IAsyncWaitQueue<object> queue)
        {
            _queue = queue;
            _count = initialCount;
            _mutex = new object();
            //Enlightenment.Trace.AsyncSemaphore_CountChanged(this, initialCount);
        }

        /// <summary>
        /// Creates a new async-compatible semaphore with the specified initial count.
        /// </summary>
        /// <param name="initialCount">The initial count for this semaphore. This must be greater than or equal to zero.</param>
        public AsyncSemaphore(int initialCount)
            : this(initialCount, new DefaultAsyncWaitQueue<object>())
        {
        }

        /// <summary>
        /// Gets a semi-unique identifier for this asynchronous semaphore.
        /// </summary>
        public int Id
        {
            get { return IdManager<AsyncSemaphore>.GetId(ref _id); }
        }

        /// <summary>
        /// Gets the number of slots currently available on this semaphore.
        /// </summary>
        public int CurrentCount
        {
            get { lock (_mutex) { return _count; } }
        }

        /// <summary>
        /// Asynchronously waits for a slot in the semaphore to be available.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this is already set, then this method will attempt to take the slot immediately (succeeding if a slot is currently available).</param>
        public Task WaitAsync(CancellationToken cancellationToken)
        {
            Task ret;
            lock (_mutex)
            {
                // If the semaphore is available, take it immediately and return.
                if (_count != 0)
                {
                    --_count;
                    //Enlightenment.Trace.AsyncSemaphore_CountChanged(this, _count);
                    ret = TaskConstants.Completed;
                }
                else
                {
                    // Wait for the semaphore to become available or cancellation.
                    ret = _queue.Enqueue(_mutex, cancellationToken);
                }
                //Enlightenment.Trace.AsyncSemaphore_TrackWait(this, ret);
            }

            return ret;
        }

        /// <summary>
        /// Synchronously waits for a slot in the semaphore to be available. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this is already set, then this method will attempt to take the slot immediately (succeeding if a slot is currently available).</param>
        public void Wait(CancellationToken cancellationToken)
        {
            WaitAsync(cancellationToken).WaitAndUnwrapException();
        }

        /// <summary>
        /// Asynchronously waits for a slot in the semaphore to be available.
        /// </summary>
        public Task WaitAsync()
        {
            return WaitAsync(CancellationToken.None);
        }

        /// <summary>
        /// Synchronously waits for a slot in the semaphore to be available. This method may block the calling thread.
        /// </summary>
        public void Wait()
        {
            Wait(CancellationToken.None);
        }

        /// <summary>
        /// Releases the semaphore.
        /// </summary>
        public void Release(int releaseCount)
        {
            if (releaseCount == 0)
                return;
            var finishes = new List<IDisposable>();
            lock (_mutex)
            {
                if (_count > int.MaxValue - releaseCount)
                    throw new InvalidOperationException("Could not release semaphore.");

                var oldCount = _count;
                while (releaseCount != 0)
                {
                    if (_queue.IsEmpty)
                        ++_count;
                    else
                        finishes.Add(_queue.Dequeue());
                    --releaseCount;
                }

                //if (_count != oldCount)
                //    Enlightenment.Trace.AsyncSemaphore_CountChanged(this, _count);
            }
            foreach (var finish in finishes)
                finish.Dispose();
        }

        /// <summary>
        /// Releases the semaphore.
        /// </summary>
        public void Release()
        {
            Release(1);
        }

        // ReSharper disable UnusedMember.Local
        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly AsyncSemaphore _semaphore;

            public DebugView(AsyncSemaphore semaphore)
            {
                _semaphore = semaphore;
            }

            public int Id { get { return _semaphore.Id; } }

            public int CurrentCount { get { return _semaphore._count; } }

            public IAsyncWaitQueue<object> WaitQueue { get { return _semaphore._queue; } }
        }
        // ReSharper restore UnusedMember.Local
    }
}

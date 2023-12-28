using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx.Internals;
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
        /// Creates an async-compatible condition variable associated with an async-compatible lock.
        /// </summary>
        /// <param name="asyncLock">The lock associated with this condition variable.</param>
        public AsyncConditionVariable(AsyncLock asyncLock)
        {
	        _asyncLock = asyncLock;
	        _queue = DefaultAsyncWaitQueue<object>.Empty;
        }

		/// <summary>
		/// Gets a semi-unique identifier for this asynchronous condition variable.
		/// </summary>
		public int Id => IdManager<AsyncConditionVariable>.GetId(ref _id);

		/// <summary>
        /// Sends a signal to a single task waiting on this condition variable. The associated lock MUST be held when calling this method, and it will still be held when this method returns.
        /// </summary>
        public void Notify()
		{
			Action? completion = null;
			InterlockedState.Transform(ref _queue, q => q switch
			{
				{ IsEmpty: false } => q.Dequeue(out completion),
				_ => q,
			});
            completion?.Invoke();
        }

        /// <summary>
        /// Sends a signal to all tasks waiting on this condition variable. The associated lock MUST be held when calling this method, and it will still be held when this method returns.
        /// </summary>
        public void NotifyAll()
        {
	        Action? completion = null;
            InterlockedState.Transform(ref _queue, q => q.DequeueAll(out completion));
            completion?.Invoke();
        }

        /// <summary>
        /// Asynchronously waits for a signal on this condition variable. The associated lock MUST be held when calling this method, and it will still be held when this method returns, even if the method is cancelled.
        /// </summary>
        /// <param name="cancellationToken">The cancellation signal used to cancel this wait.</param>
        public Task WaitAsync(CancellationToken cancellationToken)
        {
	        Task<object> task = null!;

			// Begin waiting for either a signal or cancellation.
			InterlockedState.Transform(ref _queue, q => q.Enqueue(ApplyCancel, cancellationToken, out task));

			// Attach to the signal or cancellation.
			var ret = WaitAndRetakeLockAsync(task, _asyncLock);

			// Release the lock while we are waiting.
			_asyncLock.ReleaseLock();

			return ret;

            static async Task WaitAndRetakeLockAsync(Task task, AsyncLock asyncLock)
            {
	            try
	            {
		            await task.ConfigureAwait(false);
	            }
	            finally
	            {
		            // Re-take the lock.
#pragma warning disable CA2016
		            await asyncLock.LockAsync().ConfigureAwait(false);
#pragma warning restore CA2016
	            }
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
            WaitAsync(cancellationToken).WaitAndUnwrapException(CancellationToken.None);
        }

        /// <summary>
        /// Synchronously waits for a signal on this condition variable. This method may block the calling thread. The associated lock MUST be held when calling this method, and it will still be held when this method returns.
        /// </summary>
        public void Wait()
        {
            Wait(CancellationToken.None);
        }

        private void ApplyCancel(Func<IAsyncWaitQueue<object>, IAsyncWaitQueue<object>> cancel) => InterlockedState.Transform(ref _queue, cancel);

        /// <summary>
        /// The lock associated with this condition variable.
        /// </summary>
        private readonly AsyncLock _asyncLock;

        /// <summary>
        /// The queue of waiting tasks.
        /// </summary>
        private IAsyncWaitQueue<object> _queue;

        /// <summary>
        /// The semi-unique identifier for this instance. This is 0 if the id has not yet been created.
        /// </summary>
        private int _id;

		// ReSharper disable UnusedMember.Local
		[DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly AsyncConditionVariable _cv;

            public DebugView(AsyncConditionVariable cv)
            {
                _cv = cv;
            }

            public int Id => _cv.Id;

            public AsyncLock AsyncLock => _cv._asyncLock;

            public IAsyncWaitQueue<object> WaitQueue => _cv._queue;
        }
        // ReSharper restore UnusedMember.Local
    }
}

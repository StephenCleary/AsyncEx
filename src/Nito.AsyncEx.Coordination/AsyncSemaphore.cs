using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx.Internals;
using Nito.AsyncEx.Synchronous;

// Original idea from Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266983.aspx

namespace Nito.AsyncEx
{
    /// <summary>
    /// An async-compatible semaphore. Alternatively, you could use <c>SemaphoreSlim</c>.
    /// </summary>
    [DebuggerDisplay("Id = {Id}, CurrentCount = {_state.Count}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed class AsyncSemaphore
    {
        /// <summary>
        /// Creates a new async-compatible semaphore with the specified initial count.
        /// </summary>
        /// <param name="initialCount">The initial count for this semaphore. This must be greater than or equal to zero.</param>
        public AsyncSemaphore(long initialCount) => _state = new(initialCount, DefaultAsyncWaitQueue<object>.Empty);

        /// <summary>
        /// Gets a semi-unique identifier for this asynchronous semaphore.
        /// </summary>
        public int Id => IdManager<AsyncSemaphore>.GetId(ref _id);

        /// <summary>
        /// Gets the number of slots currently available on this semaphore. This member is seldom used; code using this member has a high possibility of race conditions.
        /// </summary>
        public long CurrentCount => InterlockedState.Read(ref _state).Count;

        /// <summary>
        /// Asynchronously waits for a slot in the semaphore to be available.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this is already set, then this method will attempt to take the slot immediately (succeeding if a slot is currently available).</param>
        internal Task InternalWaitAsync(CancellationToken cancellationToken)
        {
	        Task<object>? result = null;
	        InterlockedState.Transform(ref _state, s => s switch
	        {
		        { Count: 0 } => new State(0, s.Queue.Enqueue(ApplyCancel, cancellationToken, out result)),
		        { Count: var count } => new State(count - 1, s.Queue),
	        });
	        return result ?? TaskConstants.Completed;
        }

        /// <summary>
        /// Asynchronously waits for a slot in the semaphore to be available.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this is already set, then this method will attempt to take the slot immediately (succeeding if a slot is currently available).</param>
        public Task WaitAsync(CancellationToken cancellationToken) => AsyncUtility.ForceAsync(InternalWaitAsync(cancellationToken));

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
        /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this is already set, then this method will attempt to take the slot immediately (succeeding if a slot is currently available).</param>
        public void Wait(CancellationToken cancellationToken)
        {
            InternalWaitAsync(cancellationToken).WaitAndUnwrapException(CancellationToken.None);
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
        public void Release(long releaseCount)
        {
            if (releaseCount == 0)
                return;

            Action? completion = null;
            InterlockedState.Transform(ref _state, s =>
            {
	            var localReleaseCount = releaseCount;
	            completion = null;
	            checked
	            {
		            _ = s.Count + localReleaseCount;
	            }

	            var localQueue = s.Queue;
	            while (localReleaseCount != 0 && !localQueue.IsEmpty)
	            {
		            localQueue = localQueue.Dequeue(out var itemCompletion);
                    completion += itemCompletion;
		            --localReleaseCount;
	            }

                return new State(s.Count + localReleaseCount, localQueue);
            });
            completion?.Invoke();
        }

        /// <summary>
        /// Releases the semaphore.
        /// </summary>
        public void Release()
        {
            Release(1);
        }

        private async Task<IDisposable> DoLockAsync(CancellationToken cancellationToken)
        {
            await WaitAsync(cancellationToken).ConfigureAwait(false);
            return Disposables.Disposable.Create(Release);
        }

        /// <summary>
        /// Asynchronously waits on the semaphore, and returns a disposable that releases the semaphore when disposed, thus treating this semaphore as a "multi-lock".
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this is already set, then this method will attempt to take the slot immediately (succeeding if a slot is currently available).</param>
        public AwaitableDisposable<IDisposable> LockAsync(CancellationToken cancellationToken)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            return new AwaitableDisposable<IDisposable>(DoLockAsync(cancellationToken));
#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        /// <summary>
        /// Asynchronously waits on the semaphore, and returns a disposable that releases the semaphore when disposed, thus treating this semaphore as a "multi-lock".
        /// </summary>
        public AwaitableDisposable<IDisposable> LockAsync() => LockAsync(CancellationToken.None);

        /// <summary>
        /// Synchronously waits on the semaphore, and returns a disposable that releases the semaphore when disposed, thus treating this semaphore as a "multi-lock".
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this is already set, then this method will attempt to take the slot immediately (succeeding if a slot is currently available).</param>
        public IDisposable Lock(CancellationToken cancellationToken)
        {
            Wait(cancellationToken);
            return Disposables.Disposable.Create(Release);
        }

        /// <summary>
        /// Synchronously waits on the semaphore, and returns a disposable that releases the semaphore when disposed, thus treating this semaphore as a "multi-lock".
        /// </summary>
        public IDisposable Lock() => Lock(CancellationToken.None);

        private void ApplyCancel(Func<IAsyncWaitQueue<object>, IAsyncWaitQueue<object>> cancel) =>
	        InterlockedState.Transform(ref _state, s => new State(s.Count, cancel(s.Queue)));

		/// <summary>
		/// The semi-unique identifier for this instance. This is 0 if the id has not yet been created.
		/// </summary>
		private int _id;

        private State _state;

		private sealed class State
		{
			public State(long count, IAsyncWaitQueue<object> queue)
			{
				Count = count;
				Queue = queue;
			}

			/// <summary>
			/// The number of waits that will be immediately granted.
			/// </summary>
			public long Count { get; }

			/// <summary>
			/// The queue of TCSs that other tasks are awaiting to acquire the semaphore.
			/// </summary>
			public IAsyncWaitQueue<object> Queue { get; }
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

            public int Id => _semaphore.Id;

            public long CurrentCount => _semaphore._state.Count;

            public IAsyncWaitQueue<object> WaitQueue => _semaphore._state.Queue;
        }
        // ReSharper restore UnusedMember.Local
    }
}

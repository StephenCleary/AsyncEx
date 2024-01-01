using Nito.AsyncEx.Internals;
using Nito.AsyncEx.Synchronous;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

// Original idea from Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/building-async-coordination-primitives-part-7-asyncreaderwriterlock.aspx

namespace Nito.AsyncEx
{
    /// <summary>
    /// A reader/writer lock that is compatible with async. Note that this lock is <b>not</b> recursive!
    /// </summary>
    [DebuggerDisplay("Id = {Id}, State = {GetStateForDebugger}, ReaderCount = {GetReaderCountForDebugger}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed class AsyncReaderWriterLock
    {
	    private sealed class State
	    {
			public State(int locksHeld, IAsyncWaitQueue<IDisposable> writerQueue, IAsyncWaitQueue<IDisposable> readerQueue)
			{
				LocksHeld = locksHeld;
				WriterQueue = writerQueue;
				ReaderQueue = readerQueue;
			}

			/// <summary>
			/// The queue of TCSs that other tasks are awaiting to acquire the lock as readers.
			/// </summary>
			public IAsyncWaitQueue<IDisposable> ReaderQueue { get; }

			/// <summary>
			/// The queue of TCSs that other tasks are awaiting to acquire the lock as writers.
			/// </summary>
			public IAsyncWaitQueue<IDisposable> WriterQueue { get; }

			/// <summary>
			/// Number of reader locks held; -1 if a writer lock is held; 0 if no locks are held.
			/// </summary>
			public int LocksHeld { get; }
	    }

	    private State _state;

        /// <summary>
        /// The semi-unique identifier for this instance. This is 0 if the id has not yet been created.
        /// </summary>
        private int _id;

        [DebuggerNonUserCode]
        internal LockState GetStateForDebugger => _state.LocksHeld switch
        {
	        0 => LockState.Unlocked,
	        -1 => LockState.WriteLocked,
	        _ => LockState.ReadLocked
        };

        internal enum LockState
        {
            Unlocked,
            ReadLocked,
            WriteLocked,
        }

        [DebuggerNonUserCode]
        internal int GetReaderCountForDebugger => _state.LocksHeld > 0 ? _state.LocksHeld : 0;

        /// <summary>
        /// Creates a new async-compatible reader/writer lock.
        /// </summary>
        public AsyncReaderWriterLock()
        {
	        _state = new(0, DefaultAsyncWaitQueue<IDisposable>.Empty, DefaultAsyncWaitQueue<IDisposable>.Empty);
        }

        /// <summary>
        /// Gets a semi-unique identifier for this asynchronous lock.
        /// </summary>
        public int Id => IdManager<AsyncReaderWriterLock>.GetId(ref _id);

        /// <summary>
        /// Applies a continuation to the task that will call <see cref="ReleaseWaiters"/> if the task is canceled.
        /// </summary>
        /// <param name="task">The task to observe for cancellation.</param>
        private void ReleaseWaitersWhenCanceled(Task task)
        {
            task.ContinueWith(t =>
            {
	            Action? completion = null;
	            InterlockedState.Transform(ref _state, s => ReleaseWaiters(s, out completion));
                completion?.Invoke();
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        /// <summary>
        /// Asynchronously acquires the lock as a reader. Returns a disposable that releases the lock when disposed.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        internal Task<IDisposable> InternalReaderLockAsync(CancellationToken cancellationToken)
        {
	        Task<IDisposable>? task = null;
            InterlockedState.Transform(ref _state, s => s switch
            {
	            // If the lock is available or in read mode and there are no waiting writers, upgradeable readers, or upgrading readers, take it immediately.
	            { LocksHeld: >= 0 } and { WriterQueue.IsEmpty: true } => new State(s.LocksHeld + 1, s.WriterQueue, s.ReaderQueue),
                _ => new State(s.LocksHeld, s.WriterQueue, s.ReaderQueue.Enqueue(ApplyReadCancel, cancellationToken, out task)),
            });
#pragma warning disable CA2000
            return task ?? Task.FromResult<IDisposable>(new ReaderKey(this));
#pragma warning restore CA2000
        }

		/// <summary>
		/// Asynchronously acquires the lock as a reader. Returns a disposable that releases the lock when disposed.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
		/// <returns>A disposable that releases the lock when disposed.</returns>
		public AwaitableDisposable<IDisposable> ReaderLockAsync(CancellationToken cancellationToken)
        {
            return new AwaitableDisposable<IDisposable>(AsyncUtility.ForceAsync(InternalReaderLockAsync(cancellationToken)));
        }

        /// <summary>
        /// Asynchronously acquires the lock as a reader. Returns a disposable that releases the lock when disposed.
        /// </summary>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public AwaitableDisposable<IDisposable> ReaderLockAsync()
        {
            return ReaderLockAsync(CancellationToken.None);
        }

        /// <summary>
        /// Synchronously acquires the lock as a reader. Returns a disposable that releases the lock when disposed. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public IDisposable ReaderLock(CancellationToken cancellationToken)
        {
            return InternalReaderLockAsync(cancellationToken).WaitAndUnwrapException();
        }

        /// <summary>
        /// Synchronously acquires the lock as a reader. Returns a disposable that releases the lock when disposed. This method may block the calling thread.
        /// </summary>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public IDisposable ReaderLock()
        {
            return ReaderLock(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously acquires the lock as a writer. Returns a disposable that releases the lock when disposed.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        internal Task<IDisposable> InternalWriterLockAsync(CancellationToken cancellationToken)
        {
	        Task<IDisposable>? task = null;
            InterlockedState.Transform(ref _state, s => s switch
            {
                // If the lock is available, take it immediately.
                { LocksHeld: 0 } => new State(-1, s.WriterQueue, s.ReaderQueue),
				_ => new State(s.LocksHeld, s.WriterQueue.Enqueue(ApplyWriteCancel, cancellationToken, out task), s.ReaderQueue),
			});
#pragma warning disable CA2000
            task ??= Task.FromResult<IDisposable>(new WriterKey(this));
#pragma warning restore CA2000
            ReleaseWaitersWhenCanceled(task);
            return task;
        }

        /// <summary>
        /// Asynchronously acquires the lock as a writer. Returns a disposable that releases the lock when disposed.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public AwaitableDisposable<IDisposable> WriterLockAsync(CancellationToken cancellationToken)
        {
            return new AwaitableDisposable<IDisposable>(AsyncUtility.ForceAsync(InternalWriterLockAsync(cancellationToken)));
        }

        /// <summary>
        /// Asynchronously acquires the lock as a writer. Returns a disposable that releases the lock when disposed.
        /// </summary>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public AwaitableDisposable<IDisposable> WriterLockAsync()
        {
            return WriterLockAsync(CancellationToken.None);
        }

        /// <summary>
        /// Synchronously acquires the lock as a writer. Returns a disposable that releases the lock when disposed. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public IDisposable WriterLock(CancellationToken cancellationToken)
        {
            return InternalWriterLockAsync(cancellationToken).WaitAndUnwrapException();
        }

        /// <summary>
        /// Asynchronously acquires the lock as a writer. Returns a disposable that releases the lock when disposed. This method may block the calling thread.
        /// </summary>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public IDisposable WriterLock()
        {
            return WriterLock(CancellationToken.None);
        }

        /// <summary>
        /// Grants lock(s) to waiting tasks.
        /// </summary>
        private State ReleaseWaiters(State s, out Action? completion)
        {
	        completion = null;
	        if (s.LocksHeld == -1)
		        return s;

			// Give priority to writers, then readers.
	        if (!s.WriterQueue.IsEmpty)
	        {
		        if (s.LocksHeld == 0)
#pragma warning disable CA2000 // Dispose objects before losing scope
					return new(-1, s.WriterQueue.Dequeue(out completion, new WriterKey(this)), s.ReaderQueue);
#pragma warning restore CA2000 // Dispose objects before losing scope
				return s;
	        }

	        var locksHeld = s.LocksHeld;
	        var readerQueue = s.ReaderQueue;
	        while (!readerQueue.IsEmpty)
	        {
#pragma warning disable CA2000 // Dispose objects before losing scope
				readerQueue = readerQueue.Dequeue(out var localCompletion, new ReaderKey(this));
#pragma warning restore CA2000 // Dispose objects before losing scope
				completion += localCompletion;
                ++locksHeld;
	        }

	        return new(locksHeld, s.WriterQueue, readerQueue);
        }

        /// <summary>
        /// Releases the lock as a reader.
        /// </summary>
        internal void ReleaseReaderLock()
        {
            Action? completion = null;
	        InterlockedState.Transform(ref _state, s => ReleaseWaiters(new(s.LocksHeld - 1, s.WriterQueue, s.ReaderQueue), out completion));
            completion?.Invoke();
        }

        /// <summary>
        /// Releases the lock as a writer.
        /// </summary>
        internal void ReleaseWriterLock()
        {
	        Action? completion = null;
            InterlockedState.Transform(ref _state, s => ReleaseWaiters(new State(0, s.WriterQueue, s.ReaderQueue), out completion));
            completion?.Invoke();
        }

        private void ApplyReadCancel(Func<IAsyncWaitQueue<IDisposable>, IAsyncWaitQueue<IDisposable>> cancel) =>
	        InterlockedState.Transform(ref _state, s => new State(s.LocksHeld, s.WriterQueue, cancel(s.ReaderQueue)));

        private void ApplyWriteCancel(Func<IAsyncWaitQueue<IDisposable>, IAsyncWaitQueue<IDisposable>> cancel) =>
	        InterlockedState.Transform(ref _state, s => new State(s.LocksHeld, cancel(s.WriterQueue), s.ReaderQueue));

		/// <summary>
		/// The disposable which releases the reader lock.
		/// </summary>
		private sealed class ReaderKey : Disposables.SingleDisposable<AsyncReaderWriterLock>
        {
            /// <summary>
            /// Creates the key for a lock.
            /// </summary>
            /// <param name="asyncReaderWriterLock">The lock to release. May not be <c>null</c>.</param>
            public ReaderKey(AsyncReaderWriterLock asyncReaderWriterLock)
                : base(asyncReaderWriterLock)
            {
            }

            protected override void Dispose(AsyncReaderWriterLock context)
            {
                context.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// The disposable which releases the writer lock.
        /// </summary>
        private sealed class WriterKey : Disposables.SingleDisposable<AsyncReaderWriterLock>
        {
            /// <summary>
            /// Creates the key for a lock.
            /// </summary>
            /// <param name="asyncReaderWriterLock">The lock to release. May not be <c>null</c>.</param>
            public WriterKey(AsyncReaderWriterLock asyncReaderWriterLock)
                : base(asyncReaderWriterLock)
            {
            }

            protected override void Dispose(AsyncReaderWriterLock context)
            {
                context.ReleaseWriterLock();
            }
        }

        // ReSharper disable UnusedMember.Local
        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly AsyncReaderWriterLock _rwl;

            public DebugView(AsyncReaderWriterLock rwl)
            {
                _rwl = rwl;
            }

            public int Id { get { return _rwl.Id; } }

            public LockState State { get { return _rwl.GetStateForDebugger; } }

            public int ReaderCount { get { return _rwl.GetReaderCountForDebugger; } }

            public IAsyncWaitQueue<IDisposable> ReaderWaitQueue { get { return _rwl._state.ReaderQueue; } }

            public IAsyncWaitQueue<IDisposable> WriterWaitQueue { get { return _rwl._state.WriterQueue; } }
        }
        // ReSharper restore UnusedMember.Local
    }
}

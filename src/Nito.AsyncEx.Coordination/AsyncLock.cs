﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx.Internals;
using Nito.AsyncEx.Synchronous;

// Original idea from Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266988.aspx

namespace Nito.AsyncEx
{
    /// <summary>
    /// A mutual exclusion lock that is compatible with async. Note that this lock is <b>not</b> recursive!
    /// </summary>
    /// <remarks>
    /// <para>This is the <c>async</c>-ready almost-equivalent of the <c>lock</c> keyword or the <see cref="Mutex"/> type, similar to <a href="http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266988.aspx">Stephen Toub's AsyncLock</a>. It's only <i>almost</i> equivalent because the <c>lock</c> keyword permits reentrancy, which is not currently possible to do with an <c>async</c>-ready lock.</para>
    /// <para>An <see cref="AsyncLock"/> is either taken or not. The lock can be asynchronously acquired by calling <see autoUpgrade="true" cref="LockAsync()"/>, and it is released by disposing the result of that task. <see cref="LockAsync(CancellationToken)"/> takes an optional <see cref="CancellationToken"/>, which can be used to cancel the acquiring of the lock.</para>
    /// <para>The task returned from <see autoUpgrade="true" cref="LockAsync()"/> will enter the <c>Completed</c> state when it has acquired the <see cref="AsyncLock"/>. That same task will enter the <c>Canceled</c> state if the <see cref="CancellationToken"/> is signaled before the wait is satisfied; in that case, the <see cref="AsyncLock"/> is not taken by that task.</para>
    /// <para>You can call <see cref="Lock(CancellationToken)"/> or <see cref="LockAsync(CancellationToken)"/> with an already-cancelled <see cref="CancellationToken"/> to attempt to acquire the <see cref="AsyncLock"/> immediately without actually entering the wait queue.</para>
    /// </remarks>
    /// <example>
    /// <para>The vast majority of use cases are to just replace a <c>lock</c> statement. That is, with the original code looking like this:</para>
    /// <code>
    /// private readonly object _mutex = new object();
    /// public void DoStuff()
    /// {
    ///     lock (_mutex)
    ///     {
    ///         Thread.Sleep(TimeSpan.FromSeconds(1));
    ///     }
    /// }
    /// </code>
    /// <para>If we want to replace the blocking operation <c>Thread.Sleep</c> with an asynchronous equivalent, it's not directly possible because of the <c>lock</c> block. We cannot <c>await</c> inside of a <c>lock</c>.</para>
    /// <para>So, we use the <c>async</c>-compatible <see cref="AsyncLock"/> instead:</para>
    /// <code>
    /// private readonly AsyncLock _mutex = new AsyncLock();
    /// public async Task DoStuffAsync()
    /// {
    ///     using (await _mutex.LockAsync())
    ///     {
    ///         await Task.Delay(TimeSpan.FromSeconds(1));
    ///     }
    /// }
    /// </code>
    /// </example>
    [DebuggerDisplay("Id = {Id}, Taken = {_state.Taken}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed class AsyncLock
    {
        /// <summary>
        /// Creates a new async-compatible mutual exclusion lock.
        /// </summary>
        public AsyncLock()
        {
            _state = new(false, DefaultAsyncWaitQueue<IDisposable>.Empty);
        }

        /// <summary>
        /// Gets a semi-unique identifier for this asynchronous lock.
        /// </summary>
        public int Id => IdManager<AsyncLock>.GetId(ref _id);

        /// <summary>
        /// Asynchronously acquires the lock. Returns a disposable that releases the lock when disposed.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        private Task<IDisposable> RequestLockAsync(CancellationToken cancellationToken)
        {
            Task<IDisposable>? result = null;
	        InterlockedState.Transform(ref _state, s => s switch
	        {
		        { Taken: true } => new State(true, s.Queue.Enqueue(ApplyCancel, cancellationToken, out result)),
		        { Taken: false } => new State(true, s.Queue),
	        });
#pragma warning disable CA2000 // Dispose objects before losing scope
			return result ?? Task.FromResult<IDisposable>(new Key(this));
#pragma warning restore CA2000 // Dispose objects before losing scope
		}

		/// <summary>
		/// Asynchronously acquires the lock. Returns a disposable that releases the lock when disposed.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
		/// <returns>A disposable that releases the lock when disposed.</returns>
		public AwaitableDisposable<IDisposable> LockAsync(CancellationToken cancellationToken)
        {
            return new AwaitableDisposable<IDisposable>(RequestLockAsync(cancellationToken));
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
        /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
        public IDisposable Lock(CancellationToken cancellationToken)
        {
            return RequestLockAsync(cancellationToken).WaitAndUnwrapException();
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
	        Action? completion = null;
	        InterlockedState.Transform(ref _state, s => s switch
	        {
		        { Queue.IsEmpty: true } => new State(false, s.Queue),
		        _ => new State(true, s.Queue.Dequeue(out completion, new Key(this))),
	        });
            completion?.Invoke();
        }

        private void ApplyCancel(Func<IAsyncWaitQueue<IDisposable>, IAsyncWaitQueue<IDisposable>> cancel) =>
	        InterlockedState.Transform(ref _state, s => new State(s.Taken, cancel(s.Queue)));

        private State _state;

        /// <summary>
        /// The semi-unique identifier for this instance. This is 0 if the id has not yet been created.
        /// </summary>
        private int _id;

        private sealed class State
        {
	        public State(bool taken, IAsyncWaitQueue<IDisposable> queue)
	        {
		        Taken = taken;
		        Queue = queue;
	        }

	        /// <summary>
	        /// Whether the lock is taken by a task.
	        /// </summary>
	        public bool Taken { get; }

	        /// <summary>
	        /// The queue of TCSs that other tasks are awaiting to acquire the lock.
	        /// </summary>
	        public IAsyncWaitQueue<IDisposable> Queue { get; }
        }

		/// <summary>
		/// The disposable which releases the lock.
		/// </summary>
		private sealed class Key : Disposables.SingleDisposable<AsyncLock>
        {
            /// <summary>
            /// Creates the key for a lock.
            /// </summary>
            /// <param name="asyncLock">The lock to release. May not be <c>null</c>.</param>
            public Key(AsyncLock asyncLock)
                : base(asyncLock)
            {
            }

            protected override void Dispose(AsyncLock context)
            {
                context.ReleaseLock();
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

            public int Id => _mutex.Id;

            public bool Taken => _mutex._state.Taken;

            public IAsyncWaitQueue<IDisposable> WaitQueue => _mutex._state.Queue;
        }
        // ReSharper restore UnusedMember.Local
    }
}

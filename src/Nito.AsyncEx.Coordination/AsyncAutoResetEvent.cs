using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx.Internals;
using Nito.AsyncEx.Synchronous;

// Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266923.aspx

namespace Nito.AsyncEx
{
    /// <summary>
    /// An async-compatible auto-reset event.
    /// </summary>
    [DebuggerDisplay("Id = {Id}, IsSet = {_state.IsSet}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed class AsyncAutoResetEvent
    {
        /// <summary>
        /// Creates an async-compatible auto-reset event.
        /// </summary>
        /// <param name="set">Whether the auto-reset event is initially set or unset.</param>
        public AsyncAutoResetEvent(bool set)
        {
            _state = new(set, DefaultAsyncWaitQueue<object>.Empty);
        }

		/// <summary>
		/// Creates an async-compatible auto-reset event that is initially unset.
		/// </summary>
		public AsyncAutoResetEvent()
          : this(false)
        {
        }

        /// <summary>
        /// Gets a semi-unique identifier for this asynchronous auto-reset event.
        /// </summary>
        public int Id => IdManager<AsyncAutoResetEvent>.GetId(ref _id);

        /// <summary>
        /// Whether this event is currently set. This member is seldom used; code using this member has a high possibility of race conditions.
        /// </summary>
        public bool IsSet => InterlockedState.Read(ref _state).IsSet;

        /// <summary>
        /// Asynchronously waits for this event to be set. If the event is set, this method will auto-reset it and return immediately, even if the cancellation token is already signalled. If the wait is canceled, then it will not auto-reset this event.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel this wait.</param>
        public Task WaitAsync(CancellationToken cancellationToken)
        {
	        Task<object>? result = null;
	        InterlockedState.Transform(ref _state, s => s switch
	        {
		        { IsSet: true } => new State(false, s.Queue),
                _ => new State(false, s.Queue.Enqueue(ApplyCancel, cancellationToken, out result)),
	        });
            return result ?? Task.CompletedTask;
        }

        /// <summary>
        /// Asynchronously waits for this event to be set. If the event is set, this method will auto-reset it and return immediately.
        /// </summary>
        public Task WaitAsync()
        {
            return WaitAsync(CancellationToken.None);
        }

        /// <summary>
        /// Synchronously waits for this event to be set. If the event is set, this method will auto-reset it and return immediately, even if the cancellation token is already signalled. If the wait is canceled, then it will not auto-reset this event. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel this wait.</param>
        public void Wait(CancellationToken cancellationToken)
        {
            WaitAsync(cancellationToken).WaitAndUnwrapException(CancellationToken.None);
        }

        /// <summary>
        /// Synchronously waits for this event to be set. If the event is set, this method will auto-reset it and return immediately. This method may block the calling thread.
        /// </summary>
        public void Wait()
        {
            Wait(CancellationToken.None);
        }


#pragma warning disable CA1200 // Avoid using cref tags with a prefix
        /// <summary>
        /// Sets the event, atomically completing a task returned by <see cref="o:WaitAsync"/>. If the event is already set, this method does nothing.
        /// </summary>
        public void Set()
#pragma warning restore CA1200 // Avoid using cref tags with a prefix
        {
	        Action? completion = null;
	        InterlockedState.Transform(ref _state, s => s switch
	        {
		        { Queue.IsEmpty: true } => new State(true, s.Queue),
                _ => new State(false, s.Queue.Dequeue(out completion)),
	        });
	        completion?.Invoke();
        }

        private void ApplyCancel(Func<IAsyncWaitQueue<object>, IAsyncWaitQueue<object>> cancel) =>
	        InterlockedState.Transform(ref _state, s => new State(s.IsSet, cancel(s.Queue)));

		/// <summary>
		/// The semi-unique identifier for this instance. This is 0 if the id has not yet been created.
		/// </summary>
		private int _id;

        private State _state;

        private sealed class State
        {
	        public State(bool isSet, IAsyncWaitQueue<object> queue)
	        {
		        IsSet = isSet;
		        Queue = queue;
	        }

	        /// <summary>
	        /// The current state of the event.
	        /// </summary>
	        public bool IsSet { get; }

	        /// <summary>
	        /// The queue of TCSs that other tasks are awaiting.
	        /// </summary>
	        public IAsyncWaitQueue<object> Queue { get; }
        }

		// ReSharper disable UnusedMember.Local
		[DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly AsyncAutoResetEvent _are;

            public DebugView(AsyncAutoResetEvent are)
            {
                _are = are;
            }

            public int Id { get { return _are.Id; } }

            public bool IsSet { get { return _are._state.IsSet; } }

            public IAsyncWaitQueue<object> WaitQueue { get { return _are._state.Queue; } }
        }
        // ReSharper restore UnusedMember.Local
    }
}

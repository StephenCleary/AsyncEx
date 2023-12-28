using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("AsyncEx.Coordination.UnitTests")]

namespace Nito.AsyncEx.Internals
{
    /// <summary>
    /// A collection of cancelable <see cref="TaskCompletionSource{T}"/> instances.
    /// </summary>
    /// <typeparam name="T">The type of the results. If this isn't needed, use <see cref="object"/>.</typeparam>
    internal interface IAsyncWaitQueue<T>
    {
        /// <summary>
        /// Gets whether the queue is empty.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Creates a new entry and queues it to this wait queue. The returned task must support both synchronous and asynchronous waits.
        /// </summary>
        /// <returns>The queued task.</returns>
        IAsyncWaitQueue<T> Enqueue(out Task<T> task);

		/// <summary>
		/// Removes a single entry in the wait queue and completes it. This method may only be called if <see cref="IsEmpty"/> is <c>false</c>.
		/// </summary>
		/// <param name="completion"></param>
		/// <param name="result">The result used to complete the wait queue entry. If this isn't needed, use <c>default(T)</c>.</param>
		IAsyncWaitQueue<T> Dequeue(out Action? completion, T? result = default);

		/// <summary>
		/// Removes all entries in the wait queue and completes them.
		/// </summary>
		/// <param name="completion"></param>
		/// <param name="result">The result used to complete the wait queue entries. If this isn't needed, use <c>default(T)</c>.</param>
		IAsyncWaitQueue<T> DequeueAll(out Action? completion, T? result = default);

		/// <summary>
		/// Attempts to remove an entry from the wait queue and cancels it.
		/// </summary>
		/// <param name="completion"></param>
		/// <param name="task">The task to cancel.</param>
		/// <param name="cancellationToken">The cancellation token to use to cancel the task.</param>
		IAsyncWaitQueue<T>? TryCancel(out Action? completion, Task task, CancellationToken cancellationToken);

		/// <summary>
		/// Removes all entries from the wait queue and cancels them.
		/// </summary>
		/// <param name="completion"></param>
		/// <param name="cancellationToken">The cancellation token to use to cancel the tasks.</param>
		IAsyncWaitQueue<T> CancelAll(out Action? completion, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Provides extension methods for wait queues.
    /// </summary>
    internal static class AsyncWaitQueueExtensions
    {
	    /// <summary>
	    /// Creates a new entry and queues it to this wait queue. If the cancellation token is already canceled, this method immediately returns a canceled task without modifying the wait queue.
	    /// </summary>
	    /// <param name="this">The wait queue.</param>
	    /// <param name="applyCancel"></param>
	    /// <param name="token">The token used to cancel the wait.</param>
	    /// <param name="task"></param>
	    /// <returns>The queued task.</returns>
	    public static IAsyncWaitQueue<T> Enqueue<T>(this IAsyncWaitQueue<T> @this, Action<Func<IAsyncWaitQueue<T>, IAsyncWaitQueue<T>>> applyCancel, CancellationToken token, out Task<T> task)
        {
	        if (token.IsCancellationRequested)
	        {
		        task = Task.FromCanceled<T>(token);
                return @this;
	        }

	        var ret = @this.Enqueue(out var taskResult);
	        task = taskResult;
            if (!token.CanBeCanceled)
                return ret;

            var registration = token.Register(() =>
            {
	            Action? completion = null;
                applyCancel(x => x.TryCancel(out completion, taskResult, token) ?? x);
                completion?.Invoke();
            }, useSynchronizationContext: false);
            taskResult.ContinueWith(_ => registration.Dispose(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            return ret;
        }
    }

    /// <summary>
    /// The default wait queue implementation, which uses a double-ended queue.
    /// </summary>
    /// <typeparam name="T">The type of the results. If this isn't needed, use <see cref="object"/>.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(DefaultAsyncWaitQueue<>.DebugView))]
    internal sealed class DefaultAsyncWaitQueue<T> : IAsyncWaitQueue<T>
    {
        private readonly ImmutableDeque<TaskCompletionSource<T>> _queue;

        private int Count => _queue.Count;

        public static DefaultAsyncWaitQueue<T> Empty { get; } = new(ImmutableDeque<TaskCompletionSource<T>>.Empty);

		public DefaultAsyncWaitQueue(ImmutableDeque<TaskCompletionSource<T>> queue) => _queue = queue;

		bool IAsyncWaitQueue<T>.IsEmpty => _queue.IsEmpty;

        IAsyncWaitQueue<T> IAsyncWaitQueue<T>.Enqueue(out Task<T> task)
        {
            TaskCompletionSource<T> tcs = new();
            task = tcs.Task;
            return new DefaultAsyncWaitQueue<T>(_queue.EnqueueBack(tcs));
        }

        IAsyncWaitQueue<T> IAsyncWaitQueue<T>.Dequeue(out Action? completion, T? result)
        {
	        var ret = new DefaultAsyncWaitQueue<T>(_queue.DequeueFront(out var tcs));
            completion = () => tcs.TrySetResult(result!);
            return ret;
        }

        IAsyncWaitQueue<T> IAsyncWaitQueue<T>.DequeueAll(out Action? completion, T? result)
        {
	        if (_queue.IsEmpty)
	        {
				completion = null;
				return this;
			}

            completion = () =>
            {
				foreach (var source in _queue)
					source.TrySetResult(result!);
			};
            return Empty;
        }

        IAsyncWaitQueue<T>? IAsyncWaitQueue<T>.TryCancel(out Action? completion, Task task, CancellationToken cancellationToken)
        {
	        var newQueue = _queue.RemoveOne(x => x.Task == task, out var tcs);
	        if (newQueue == null)
	        {
		        completion = null;
		        return null;
	        }

            completion = () => tcs.TrySetCanceled(cancellationToken);
            return new DefaultAsyncWaitQueue<T>(newQueue);
        }

        IAsyncWaitQueue<T> IAsyncWaitQueue<T>.CancelAll(out Action? completion, CancellationToken cancellationToken)
        {
	        if (_queue.IsEmpty)
	        {
	            completion = null;
	            return this;
			}

            completion = () =>
            {
	            foreach (var source in _queue)
		            source.TrySetCanceled(cancellationToken);
			};
            return Empty;
        }

        [DebuggerNonUserCode]
        internal sealed class DebugView
        {
            private readonly DefaultAsyncWaitQueue<T> _queue;

            public DebugView(DefaultAsyncWaitQueue<T> queue) => _queue = queue;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Task<T>[] Tasks => _queue._queue.Select(entry => entry.Task).ToArray();
        }
    }
}

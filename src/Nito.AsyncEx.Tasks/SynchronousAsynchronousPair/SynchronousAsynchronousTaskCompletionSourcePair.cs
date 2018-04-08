using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx.SynchronousAsynchronousPair
{
    /// <summary>
    /// A synchronous <see cref="TaskCompletionSource{TResult}"/> paired with an asynchronous <see cref="TaskCompletionSource{TResult}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    public sealed class SynchronousAsynchronousTaskCompletionSourcePair<T>: ISynchronousAsynchronousTaskPair<T>
    {
        private readonly TaskCompletionSource<T> _synchronous;
        private readonly TaskCompletionSource<T> _asynchronous;

        /// <summary>
        /// Creates a new pair of TCSs.
        /// </summary>
        public SynchronousAsynchronousTaskCompletionSourcePair()
        {
            _synchronous = new TaskCompletionSource<T>(TaskCreationOptions.DenyChildAttach);
            _asynchronous = new TaskCompletionSource<T>(TaskCreationOptions.DenyChildAttach | TaskCreationOptions.RunContinuationsAsynchronously);
        }

        /// <summary>
        /// The synchronously-completed task. This is appropriate for waiting on in internal code.
        /// </summary>
        public Task<T> SynchronousTask => _synchronous.Task;

        /// <summary>
        /// The asynchronously-completed task. This is appropriate for returning to end-user APIs.
        /// </summary>
        public Task<T> AsynchronousTask => _asynchronous.Task;

        /// <summary>
        /// Completes the task with the specified result.
        /// </summary>
        /// <param name="result">The task's result.</param>
        public bool TrySetResult(T result)
        {
            _synchronous.TrySetResult(result);
            return _asynchronous.TrySetResult(result);
        }

        /// <summary>
        /// Completes the task with the specified exception.
        /// </summary>
        /// <param name="exception">The task's exception.</param>
        public bool TrySetException(Exception exception)
        {
            _synchronous.TrySetException(exception);
            return _asynchronous.TrySetException(exception);
        }

        /// <summary>
        /// Completes the task as canceled by the specified cancellation token.
        /// </summary>
        /// <param name="cancellationToken">The task's cancelling token.</param>
        public bool TrySetCanceled(CancellationToken cancellationToken)
        {
            _synchronous.TrySetCanceled(cancellationToken);
            return _asynchronous.TrySetCanceled(cancellationToken);
        }

        /// <summary>
        /// Creates and completes a TCS pair from a result value.
        /// </summary>
        /// <param name="result">The result value.</param>
        /// <returns>The completed TCS pair.</returns>
        public static SynchronousAsynchronousTaskCompletionSourcePair<T> FromResult(T result)
        {
            var tcs = new SynchronousAsynchronousTaskCompletionSourcePair<T>();
            tcs.TrySetResult(result);
            return tcs;
        }

        /// <summary>
        /// Creates and completes a TCS pair from an exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>The completed TCS pair.</returns>
        public static SynchronousAsynchronousTaskCompletionSourcePair<T> FromException(Exception exception)
        {
            var tcs = new SynchronousAsynchronousTaskCompletionSourcePair<T>();
            tcs.TrySetException(exception);
            return tcs;
        }

        /// <summary>
        /// Creates and completes a TCS pair from a cancellation token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The completed TCS pair.</returns>
        public static SynchronousAsynchronousTaskCompletionSourcePair<T> FromCanceled(CancellationToken cancellationToken)
        {
            var tcs = new SynchronousAsynchronousTaskCompletionSourcePair<T>();
            tcs.TrySetCanceled(cancellationToken);
            return tcs;
        }
    }
}

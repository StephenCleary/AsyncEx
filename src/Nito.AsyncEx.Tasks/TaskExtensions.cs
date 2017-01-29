using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// Provides extension methods for the <see cref="Task"/> and <see cref="Task{T}"/> types.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Asynchronously waits for the task to complete, or for the cancellation token to be canceled.
        /// </summary>
        /// <param name="this">The task to wait for. May not be <c>null</c>.</param>
        /// <param name="cancellationToken">The cancellation token that cancels the wait.</param>
        public static Task WaitAsync(this Task @this, CancellationToken cancellationToken)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            if (!cancellationToken.CanBeCanceled)
                return @this;
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(cancellationToken);
            return DoWaitAsync(@this, cancellationToken);
        }

        private static async Task DoWaitAsync(Task task, CancellationToken cancellationToken)
        {
            using (var cancelTaskSource = new CancellationTokenTaskSource<object>(cancellationToken))
                await await Task.WhenAny(task, cancelTaskSource.Task).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously waits for any of the source tasks to complete, or for the cancellation token to be canceled.
        /// </summary>
        /// <param name="this">The tasks to wait for. May not be <c>null</c>.</param>
        /// <param name="cancellationToken">The cancellation token that cancels the wait.</param>
        public static Task<Task> WhenAny(this IEnumerable<Task> @this, CancellationToken cancellationToken)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            return Task.WhenAny(@this).WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously waits for any of the source tasks to complete.
        /// </summary>
        /// <param name="this">The tasks to wait for. May not be <c>null</c>.</param>
        public static Task<Task> WhenAny(this IEnumerable<Task> @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            return Task.WhenAny(@this);
        }

        /// <summary>
        /// Asynchronously waits for all of the source tasks to complete.
        /// </summary>
        /// <param name="this">The tasks to wait for. May not be <c>null</c>.</param>
        public static Task WhenAll(this IEnumerable<Task> @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            return Task.WhenAll(@this);
        }

        /// <summary>
        /// Asynchronously waits for the task to complete, or for the cancellation token to be canceled.
        /// </summary>
        /// <typeparam name="TResult">The type of the task result.</typeparam>
        /// <param name="this">The task to wait for. May not be <c>null</c>.</param>
        /// <param name="cancellationToken">The cancellation token that cancels the wait.</param>
        public static Task<TResult> WaitAsync<TResult>(this Task<TResult> @this, CancellationToken cancellationToken)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            if (!cancellationToken.CanBeCanceled)
                return @this;
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<TResult>(cancellationToken);
            return DoWaitAsync(@this, cancellationToken);
        }

        private static async Task<TResult> DoWaitAsync<TResult>(Task<TResult> task, CancellationToken cancellationToken)
        {
            using (var cancelTaskSource = new CancellationTokenTaskSource<TResult>(cancellationToken))
                return await await Task.WhenAny(task, cancelTaskSource.Task).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously waits for any of the source tasks to complete, or for the cancellation token to be canceled.
        /// </summary>
        /// <typeparam name="TResult">The type of the task results.</typeparam>
        /// <param name="this">The tasks to wait for. May not be <c>null</c>.</param>
        /// <param name="cancellationToken">The cancellation token that cancels the wait.</param>
        public static Task<Task<TResult>> WhenAny<TResult>(this IEnumerable<Task<TResult>> @this, CancellationToken cancellationToken)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            return Task.WhenAny(@this).WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously waits for any of the source tasks to complete.
        /// </summary>
        /// <typeparam name="TResult">The type of the task results.</typeparam>
        /// <param name="this">The tasks to wait for. May not be <c>null</c>.</param>
        public static Task<Task<TResult>> WhenAny<TResult>(this IEnumerable<Task<TResult>> @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            return Task.WhenAny(@this);
        }

        /// <summary>
        /// Asynchronously waits for all of the source tasks to complete.
        /// </summary>
        /// <typeparam name="TResult">The type of the task results.</typeparam>
        /// <param name="this">The tasks to wait for. May not be <c>null</c>.</param>
        public static Task<TResult[]> WhenAll<TResult>(this IEnumerable<Task<TResult>> @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            return Task.WhenAll(@this);
        }
    }
}
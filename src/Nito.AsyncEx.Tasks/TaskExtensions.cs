using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
                await (await Task.WhenAny(task, cancelTaskSource.Task).ConfigureAwait(false)).ConfigureAwait(false);
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
                return await (await Task.WhenAny(task, cancelTaskSource.Task).ConfigureAwait(false)).ConfigureAwait(false);
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
        /// <param name="this">The tasks to wait for. May not be <c>null</c>.</param>
        public static Task WhenAll(this IEnumerable<Task> @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            return Task.WhenAll(@this);
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

        /// <summary>
        /// DANGEROUS! Ignores the completion of this task. Also ignores exceptions.
        /// </summary>
        /// <param name="this">The task to ignore.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static async void Ignore(this Task @this)
        {
            _ = @this ?? throw new ArgumentNullException(nameof(@this));
            try
            {
                await @this.ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                // ignored
            }
        }

        /// <summary>
        /// DANGEROUS! Ignores the completion and results of this task. Also ignores exceptions.
        /// </summary>
        /// <param name="this">The task to ignore.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static async void Ignore<T>(this Task<T> @this)
        {
            _ = @this ?? throw new ArgumentNullException(nameof(@this));
            try
            {
                await @this.ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                // ignored
            }
        }

        /// <summary>
        /// Creates a new collection of tasks that complete in order.
        /// </summary>
        /// <typeparam name="T">The type of the results of the tasks.</typeparam>
        /// <param name="this">The tasks to order by completion. May not be <c>null</c>.</param>
        public static List<Task<T>> OrderByCompletion<T>(this IEnumerable<Task<T>> @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            // This is a combination of Jon Skeet's approach and Stephen Toub's approach:
            //  http://msmvps.com/blogs/jon_skeet/archive/2012/01/16/eduasync-part-19-ordering-by-completion-ahead-of-time.aspx
            //  http://blogs.msdn.com/b/pfxteam/archive/2012/08/02/processing-tasks-as-they-complete.aspx

            // Reify the source task sequence. TODO: better reification.
            var taskArray = @this.ToArray();

            // Allocate a TCS array and an array of the resulting tasks.
            var numTasks = taskArray.Length;
            var tcs = new TaskCompletionSource<T>[numTasks];
            var ret = new List<Task<T>>(numTasks);

            // As each task completes, complete the next tcs.
            var lastIndex = -1;
            // ReSharper disable once ConvertToLocalFunction
            Action<Task<T>> continuation = task =>
            {
                var index = Interlocked.Increment(ref lastIndex);
                tcs[index].TryCompleteFromCompletedTask(task);
            };

            // Fill out the arrays and attach the continuations.
            for (var i = 0; i != numTasks; ++i)
            {
                tcs[i] = new TaskCompletionSource<T>();
                ret.Add(tcs[i].Task);
                taskArray[i].ContinueWith(continuation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.DenyChildAttach, TaskScheduler.Default);
            }

            return ret;
        }

        /// <summary>
        /// Creates a new collection of tasks that complete in order.
        /// </summary>
        /// <param name="this">The tasks to order by completion. May not be <c>null</c>.</param>
        public static List<Task> OrderByCompletion(this IEnumerable<Task> @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            // This is a combination of Jon Skeet's approach and Stephen Toub's approach:
            //  http://msmvps.com/blogs/jon_skeet/archive/2012/01/16/eduasync-part-19-ordering-by-completion-ahead-of-time.aspx
            //  http://blogs.msdn.com/b/pfxteam/archive/2012/08/02/processing-tasks-as-they-complete.aspx

            // Reify the source task sequence. TODO: better reification.
            var taskArray = @this.ToArray();

            // Allocate a TCS array and an array of the resulting tasks.
            var numTasks = taskArray.Length;
            var tcs = new TaskCompletionSource<object?>[numTasks];
            var ret = new List<Task>(numTasks);

            // As each task completes, complete the next tcs.
            var lastIndex = -1;
            // ReSharper disable once ConvertToLocalFunction
            Action<Task> continuation = task =>
            {
                var index = Interlocked.Increment(ref lastIndex);
                tcs[index].TryCompleteFromCompletedTask(task, NullResultFunc);
            };

            // Fill out the arrays and attach the continuations.
            for (var i = 0; i != numTasks; ++i)
            {
                tcs[i] = new TaskCompletionSource<object?>();
                ret.Add(tcs[i].Task);
                taskArray[i].ContinueWith(continuation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.DenyChildAttach, TaskScheduler.Default);
            }

            return ret;
        }
         
        private static Func<object?> NullResultFunc { get; } = () => null;
    }
}
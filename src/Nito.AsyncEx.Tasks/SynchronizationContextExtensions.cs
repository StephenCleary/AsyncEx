using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// Provides extension methods for <see cref="SynchronizationContext"/>.
    /// </summary>
    public static class SynchronizationContextExtensions
    {
        /// <summary>
        /// Synchronously executes a delegate on this synchronization context.
        /// </summary>
        /// <param name="this">The synchronization context.</param>
        /// <param name="action">The delegate to execute.</param>
        public static void Send(this SynchronizationContext @this, Action action)
        {
            _ = @this ?? throw new ArgumentNullException(nameof(@this));
            @this.Send(state => ((Action) state)(), action);
        }

        /// <summary>
        /// Synchronously executes a delegate on this synchronization context and returns its result.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="this">The synchronization context.</param>
        /// <param name="action">The delegate to execute.</param>
        public static T Send<T>(this SynchronizationContext @this, Func<T> action)
        {
            _ = @this ?? throw new ArgumentNullException(nameof(@this));
            var result = default(T);
            @this.Send(state => { result = ((Func<T>) state)(); }, action);
            return result!;
        }

        /// <summary>
        /// Asynchronously executes a delegate on this synchronization context.
        /// </summary>
        /// <param name="this">The synchronization context.</param>
        /// <param name="action">The delegate to execute.</param>
        public static Task PostAsync(this SynchronizationContext @this, Action action)
        {
            _ = @this ?? throw new ArgumentNullException(nameof(@this));
            var tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object?>();
            @this.Post(state =>
            {
                try
                {
                    ((Action) state)();
                    tcs.TrySetResult(null);
                }
                catch(OperationCanceledException ex)
                {
                    tcs.TrySetCanceled(ex.CancellationToken);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    tcs.TrySetException(ex);
                }
            }, action);
            return tcs.Task;
        }

        /// <summary>
        /// Asynchronously executes a delegate on this synchronization context and returns its result.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="this">The synchronization context.</param>
        /// <param name="action">The delegate to execute.</param>
        public static Task<T> PostAsync<T>(this SynchronizationContext @this, Func<T> action)
        {
            _ = @this ?? throw new ArgumentNullException(nameof(@this));
            var tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<T>();
            @this.Post(state =>
            {
                try
                {
                    tcs.SetResult(((Func<T>) state)());
                }
                catch (OperationCanceledException ex)
                {
                    tcs.TrySetCanceled(ex.CancellationToken);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    tcs.TrySetException(ex);
                }
            }, action);
            return tcs.Task;
        }

        /// <summary>
        /// Asynchronously executes an asynchronous delegate on this synchronization context.
        /// </summary>
        /// <param name="this">The synchronization context.</param>
        /// <param name="action">The delegate to execute.</param>
        public static Task PostAsync(this SynchronizationContext @this, Func<Task> action)
        {
            _ = @this ?? throw new ArgumentNullException(nameof(@this));
            var tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object?>();
            @this.Post(async state =>
            {
                try
                {
                    await ((Func<Task>)state)().ConfigureAwait(false);
                    tcs.TrySetResult(null);
                }
                catch (OperationCanceledException ex)
                {
                    tcs.TrySetCanceled(ex.CancellationToken);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    tcs.TrySetException(ex);
                }
            }, action);
            return tcs.Task;
        }

        /// <summary>
        /// Asynchronously executes an asynchronous delegate on this synchronization context and returns its result.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="this">The synchronization context.</param>
        /// <param name="action">The delegate to execute.</param>
        public static Task<T> PostAsync<T>(this SynchronizationContext @this, Func<Task<T>> action)
        {
            _ = @this ?? throw new ArgumentNullException(nameof(@this));
            var tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<T>();
            @this.Post(async state =>
            {
                try
                {
                    tcs.SetResult(await ((Func<Task<T>>)state)().ConfigureAwait(false));
                }
                catch (OperationCanceledException ex)
                {
                    tcs.TrySetCanceled(ex.CancellationToken);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    tcs.TrySetException(ex);
                }
            }, action);
            return tcs.Task;
        }
    }
}

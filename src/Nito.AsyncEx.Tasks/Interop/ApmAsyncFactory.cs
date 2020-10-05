using System;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;

namespace Nito.AsyncEx.Interop
{
    /// <summary>
    /// Creation methods for tasks wrapping the Asynchronous Programming Model (APM), and APM wrapper methods around tasks.
    /// </summary>
    public static class ApmAsyncFactory
    {
        /// <summary>
        /// Wraps a <see cref="Task"/> into the Begin method of an APM pattern.
        /// </summary>
        /// <param name="task">The task to wrap.</param>
        /// <param name="callback">The callback method passed into the Begin method of the APM pattern.</param>
        /// <param name="state">The state passed into the Begin method of the APM pattern.</param>
        /// <returns>The asynchronous operation, to be returned by the Begin method of the APM pattern.</returns>
        public static IAsyncResult ToBegin(Task task, AsyncCallback callback, object state)
        {
            var tcs = new TaskCompletionSource<object?>(state, TaskCreationOptions.RunContinuationsAsynchronously);
            SynchronizationContextSwitcher.NoContext(() => CompleteAsync(task, callback, tcs));
            return tcs.Task;
        }

        // `async void` is on purpose, to raise `callback` exceptions directly on the thread pool.
        private static async void CompleteAsync(Task task, AsyncCallback callback, TaskCompletionSource<object?> tcs)
        {
            try
            {
                await task.ConfigureAwait(false);
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
            finally
            {
                callback?.Invoke(tcs.Task);
            }
        }

        /// <summary>
        /// Wraps a <see cref="Task"/> into the End method of an APM pattern.
        /// </summary>
        /// <param name="asyncResult">The asynchronous operation returned by the matching Begin method of this APM pattern.</param>
        /// <returns>The result of the asynchronous operation, to be returned by the End method of the APM pattern.</returns>
        public static void ToEnd(IAsyncResult asyncResult)
        {
            ((Task)asyncResult).WaitAndUnwrapException();
        }

        /// <summary>
        /// Wraps a <see cref="Task{TResult}"/> into the Begin method of an APM pattern.
        /// </summary>
        /// <param name="task">The task to wrap. May not be <c>null</c>.</param>
        /// <param name="callback">The callback method passed into the Begin method of the APM pattern.</param>
        /// <param name="state">The state passed into the Begin method of the APM pattern.</param>
        /// <returns>The asynchronous operation, to be returned by the Begin method of the APM pattern.</returns>
        public static IAsyncResult ToBegin<TResult>(Task<TResult> task, AsyncCallback callback, object state)
        {
            var tcs = new TaskCompletionSource<TResult>(state, TaskCreationOptions.RunContinuationsAsynchronously);
            SynchronizationContextSwitcher.NoContext(() => CompleteAsync(task, callback, tcs));
            return tcs.Task;
        }

        // `async void` is on purpose, to raise `callback` exceptions directly on the thread pool.
        private static async void CompleteAsync<TResult>(Task<TResult> task, AsyncCallback callback, TaskCompletionSource<TResult> tcs)
        {
            try
            {
                tcs.TrySetResult(await task.ConfigureAwait(false));
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
            finally
            {
                callback?.Invoke(tcs.Task);
            }
        }

        /// <summary>
        /// Wraps a <see cref="Task{TResult}"/> into the End method of an APM pattern.
        /// </summary>
        /// <param name="asyncResult">The asynchronous operation returned by the matching Begin method of this APM pattern.</param>
        /// <returns>The result of the asynchronous operation, to be returned by the End method of the APM pattern.</returns>
        public static TResult ToEnd<TResult>(IAsyncResult asyncResult)
        {
            return ((Task<TResult>)asyncResult).WaitAndUnwrapException();
        }
    }
}

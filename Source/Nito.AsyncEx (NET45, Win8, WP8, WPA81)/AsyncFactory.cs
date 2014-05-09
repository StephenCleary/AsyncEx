using System.Threading;
using Nito.AsyncEx.Internal;
using Nito.AsyncEx.Internal.PlatformEnlightenment;
using Nito.AsyncEx.Synchronous;
using System;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// Provides asynchronous wrappers.
    /// </summary>
    public static partial class AsyncFactory
    {
        private static readonly TimeSpan InfiniteTimeSpan = new TimeSpan(0, 0, 0, 0, -1);

        private static AsyncCallback Callback(Action<IAsyncResult> endMethod, TaskCompletionSource<object> tcs)
        {
            return asyncResult =>
            {
                try
                {
                    endMethod(asyncResult);
                    tcs.TrySetResult(null);
                }
                catch (OperationCanceledException)
                {
                    tcs.TrySetCanceled();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            };
        }

        /// <summary>
        /// Wraps a begin/end asynchronous method.
        /// </summary>
        /// <param name="beginMethod">The begin method.</param>
        /// <param name="endMethod">The end method.</param>
        /// <returns></returns>
        public static Task FromApm(Func<AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod)
        {
            var tcs = new TaskCompletionSource<object>();
            beginMethod(Callback(endMethod, tcs), null);
            return tcs.Task;
        }

        /// <summary>
        /// Wraps a <see cref="Task"/> into the Begin method of an APM pattern.
        /// </summary>
        /// <param name="task">The task to wrap.</param>
        /// <param name="callback">The callback method passed into the Begin method of the APM pattern.</param>
        /// <param name="state">The state passed into the Begin method of the APM pattern.</param>
        /// <returns>The asynchronous operation, to be returned by the Begin method of the APM pattern.</returns>
        public static IAsyncResult ToBegin(Task task, AsyncCallback callback, object state)
        {
            var tcs = new TaskCompletionSource(state);
            task.ContinueWith(t =>
            {
                tcs.TryCompleteFromCompletedTask(t);

                if (callback != null)
                    callback(tcs.Task);
            }, TaskScheduler.Default);

            return tcs.Task;
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
        /// Wraps a <see cref="WaitHandle"/> with a <see cref="Task"/>. When the <see cref="WaitHandle"/> is signalled, the returned <see cref="Task"/> is completed. If the handle is already signalled, this method acts synchronously.
        /// </summary>
        /// <param name="handle">The <see cref="WaitHandle"/> to observe.</param>
        public static Task FromWaitHandle(WaitHandle handle)
        {
            return FromWaitHandle(handle, InfiniteTimeSpan, CancellationToken.None);
        }

        /// <summary>
        /// Wraps a <see cref="WaitHandle"/> with a <see cref="Task{Boolean}"/>. If the <see cref="WaitHandle"/> is signalled, the returned task is completed with a <c>true</c> result. If the observation times out, the returned task is completed with a <c>false</c> result. If the handle is already signalled or the timeout is zero, this method acts synchronously.
        /// </summary>
        /// <param name="handle">The <see cref="WaitHandle"/> to observe.</param>
        /// <param name="timeout">The timeout after which the <see cref="WaitHandle"/> is no longer observed.</param>
        public static Task<bool> FromWaitHandle(WaitHandle handle, TimeSpan timeout)
        {
            return FromWaitHandle(handle, timeout, CancellationToken.None);
        }

        /// <summary>
        /// Wraps a <see cref="WaitHandle"/> with a <see cref="Task{Boolean}"/>. If the <see cref="WaitHandle"/> is signalled, the returned task is (successfully) completed. If the observation is cancelled, the returned task is cancelled. If the handle is already signalled or the cancellation token is already cancelled, this method acts synchronously.
        /// </summary>
        /// <param name="handle">The <see cref="WaitHandle"/> to observe.</param>
        /// <param name="token">The cancellation token that cancels observing the <see cref="WaitHandle"/>.</param>
        public static Task FromWaitHandle(WaitHandle handle, CancellationToken token)
        {
            return FromWaitHandle(handle, InfiniteTimeSpan, token);
        }

        /// <summary>
        /// Wraps a <see cref="WaitHandle"/> with a <see cref="Task{Boolean}"/>. If the <see cref="WaitHandle"/> is signalled, the returned task is completed with a <c>true</c> result. If the observation times out, the returned task is completed with a <c>false</c> result. If the observation is cancelled, the returned task is cancelled. If the handle is already signalled, the timeout is zero, or the cancellation token is already cancelled, then this method acts synchronously.
        /// </summary>
        /// <param name="handle">The <see cref="WaitHandle"/> to observe.</param>
        /// <param name="timeout">The timeout after which the <see cref="WaitHandle"/> is no longer observed.</param>
        /// <param name="token">The cancellation token that cancels observing the <see cref="WaitHandle"/>.</param>
        public static Task<bool> FromWaitHandle(WaitHandle handle, TimeSpan timeout, CancellationToken token)
        {
            // Handle synchronous cases.
            var alreadySignalled = handle.WaitOne(0);
            if (alreadySignalled)
                return TaskConstants.BooleanTrue;
            if (timeout == TimeSpan.Zero)
                return TaskConstants.BooleanFalse;
            if (token.IsCancellationRequested)
                return TaskConstants<bool>.Canceled;

            // Register all asynchronous cases.
            var tcs = new TaskCompletionSource<bool>();
            var threadPoolRegistration = ThreadPoolEnlightenment.RegisterWaitForSingleObject(handle,
                (state, timedOut) => ((TaskCompletionSource<bool>)state).TrySetResult(!timedOut),
                tcs, timeout);
            var tokenRegistration = token.Register(
                state => ((TaskCompletionSource<bool>)state).TrySetCanceled(),
                tcs, useSynchronizationContext: false);
            tcs.Task.ContinueWith(_ =>
            {
                threadPoolRegistration.Dispose();
                tokenRegistration.Dispose();
            }, TaskScheduler.Default);
            return tcs.Task;
        }
    }
}

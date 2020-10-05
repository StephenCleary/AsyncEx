using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx.Interop
{
    /// <summary>
    /// Provides interop utilities for <see cref="WaitHandle"/> types.
    /// </summary>
    public static class WaitHandleAsyncFactory
    {
        /// <summary>
        /// Wraps a <see cref="WaitHandle"/> with a <see cref="Task"/>. When the <see cref="WaitHandle"/> is signalled, the returned <see cref="Task"/> is completed. If the handle is already signalled, this method acts synchronously.
        /// </summary>
        /// <param name="handle">The <see cref="WaitHandle"/> to observe.</param>
        public static Task FromWaitHandle(WaitHandle handle)
        {
            return FromWaitHandle(handle, Timeout.InfiniteTimeSpan, CancellationToken.None);
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
            return FromWaitHandle(handle, Timeout.InfiniteTimeSpan, token);
        }

        /// <summary>
        /// Wraps a <see cref="WaitHandle"/> with a <see cref="Task{Boolean}"/>. If the <see cref="WaitHandle"/> is signalled, the returned task is completed with a <c>true</c> result. If the observation times out, the returned task is completed with a <c>false</c> result. If the observation is cancelled, the returned task is cancelled. If the handle is already signalled, the timeout is zero, or the cancellation token is already cancelled, then this method acts synchronously.
        /// </summary>
        /// <param name="handle">The <see cref="WaitHandle"/> to observe.</param>
        /// <param name="timeout">The timeout after which the <see cref="WaitHandle"/> is no longer observed.</param>
        /// <param name="token">The cancellation token that cancels observing the <see cref="WaitHandle"/>.</param>
        public static Task<bool> FromWaitHandle(WaitHandle handle, TimeSpan timeout, CancellationToken token)
        {
            _ = handle ?? throw new ArgumentNullException(nameof(handle));

            // Handle synchronous cases.
            var alreadySignalled = handle.WaitOne(0);
            if (alreadySignalled)
                return TaskConstants.BooleanTrue;
            if (timeout == TimeSpan.Zero)
                return TaskConstants.BooleanFalse;
            if (token.IsCancellationRequested)
                return TaskConstants<bool>.Canceled;

            // Register all asynchronous cases.
            return DoFromWaitHandle(handle, timeout, token);
        }

        private static async Task<bool> DoFromWaitHandle(WaitHandle handle, TimeSpan timeout, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (new ThreadPoolRegistration(handle, timeout, tcs))
            using (token.Register(state => ((TaskCompletionSource<bool>) state).TrySetCanceled(), tcs, useSynchronizationContext: false))
                return await tcs.Task.ConfigureAwait(false);
        }

        private sealed class ThreadPoolRegistration : IDisposable
        {
            private readonly RegisteredWaitHandle _registeredWaitHandle;

            public ThreadPoolRegistration(WaitHandle handle, TimeSpan timeout, TaskCompletionSource<bool> tcs)
            {
                _registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(handle,
                    (state, timedOut) => ((TaskCompletionSource<bool>)state).TrySetResult(!timedOut), tcs,
                    timeout, executeOnlyOnce: true);
            }

            void IDisposable.Dispose() => _registeredWaitHandle.Unregister(null);
        }
    }
}

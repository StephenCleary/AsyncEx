using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// Provides extension methods for <see cref="CancellationToken"/>.
    /// </summary>
    public static class CancellationTokenExtensions
    {
        /// <summary>
        /// Returns a <see cref="Task"/> that is canceled when this <see cref="CancellationToken"/> is canceled. This method will leak resources if the cancellation token is long-lived; use <see cref="ToCancellationTokenTaskSource"/> for a similar approach with proper resource management.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor.</param>
        /// <returns>A <see cref="Task"/> that is canceled when this <see cref="CancellationToken"/> is canceled.</returns>
        public static Task AsTask(this CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
                return TaskConstants.Never;
            if (cancellationToken.IsCancellationRequested)
                return TaskConstants.Canceled;
            var tcs = new TaskCompletionSource();
            cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
            return tcs.Task;
        }

        /// <summary>
        /// Returns a <see cref="CancellationTokenTaskSource"/> which holds the task for this cancellation token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        public static CancellationTokenTaskSource ToCancellationTokenTaskSource(this CancellationToken cancellationToken)
        {
            return new CancellationTokenTaskSource(cancellationToken);
        }

        /// <summary>
        /// Holds the task for a cancellation token, as well as the token registration. The registration is disposed when this instance is disposed.
        /// </summary>
        public sealed class CancellationTokenTaskSource : IDisposable
        {
            /// <summary>
            /// The cancellation token registration, if any. This is <c>null</c> if the registration was not necessary.
            /// </summary>
            private readonly IDisposable _registration;

            /// <summary>
            /// Creates a task for the specified cancellation token, registering with the token if necessary.
            /// </summary>
            /// <param name="cancellationToken">The cancellation token to observe.</param>
            public CancellationTokenTaskSource(CancellationToken cancellationToken)
            {
                if (!cancellationToken.CanBeCanceled)
                {
                    Task = TaskConstants.Never;
                    return;
                }
                if (cancellationToken.IsCancellationRequested)
                {
                    Task = TaskConstants.Canceled;
                    return;
                }
                var tcs = new TaskCompletionSource();
                _registration = cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
                Task = tcs.Task;
            }

            /// <summary>
            /// Gets the task for the source cancellation token.
            /// </summary>
            public Task Task { get; private set; }

            /// <summary>
            /// Disposes the cancellation token registration, if any. Note that this may cause <see cref="Task"/> to never complete.
            /// </summary>
            public void Dispose()
            {
                if (_registration != null)
                    _registration.Dispose();
            }
        }
    }
}

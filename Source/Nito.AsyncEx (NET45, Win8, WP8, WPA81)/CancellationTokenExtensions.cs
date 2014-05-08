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
        /// Returns a <see cref="Task"/> that is canceled when this <see cref="CancellationToken"/> is canceled.
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
    }
}

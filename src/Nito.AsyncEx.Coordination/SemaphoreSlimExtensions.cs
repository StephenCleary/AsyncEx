using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// Provides extension methods for <see cref="SemaphoreSlim"/>.
    /// </summary>
    public static class SemaphoreSlimExtensions
    {
        private static async Task<IDisposable> DoLockAsync(SemaphoreSlim @this, CancellationToken cancellationToken)
        {
            await @this.WaitAsync(cancellationToken).ConfigureAwait(false);
            return Disposables.AnonymousDisposable.Create(() => @this.Release());
        }

        /// <summary>
        /// Asynchronously waits on the semaphore, and returns a disposable that releases the semaphore when disposed, thus treating this semaphore as a "multi-lock".
        /// </summary>
        /// <param name="this">The semaphore to lock.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait.</param>
        public static AwaitableDisposable<IDisposable> LockAsync(this SemaphoreSlim @this, CancellationToken cancellationToken)
        {
            return new AwaitableDisposable<IDisposable>(DoLockAsync(@this, cancellationToken));
        }

        /// <summary>
        /// Asynchronously waits on the semaphore, and returns a disposable that releases the semaphore when disposed, thus treating this semaphore as a "multi-lock".
        /// </summary>
        public static AwaitableDisposable<IDisposable> LockAsync(this SemaphoreSlim @this) => @this.LockAsync(CancellationToken.None);

        /// <summary>
        /// Synchronously waits on the semaphore, and returns a disposable that releases the semaphore when disposed, thus treating this semaphore as a "multi-lock".
        /// </summary>
        /// <param name="this">The semaphore to lock.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait.</param>
        public static IDisposable Lock(this SemaphoreSlim @this, CancellationToken cancellationToken)
        {
            @this.Wait(cancellationToken);
            return Disposables.AnonymousDisposable.Create(() => @this.Release());
        }

        /// <summary>
        /// Synchronously waits on the semaphore, and returns a disposable that releases the semaphore when disposed, thus treating this semaphore as a "multi-lock".
        /// </summary>
        /// <param name="this">The semaphore to lock.</param>
        public static IDisposable Lock(this SemaphoreSlim @this) => @this.Lock(CancellationToken.None);
    }
}

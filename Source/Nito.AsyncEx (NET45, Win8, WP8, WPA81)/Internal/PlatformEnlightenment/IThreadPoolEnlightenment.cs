using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    /// <summary>
    /// A portable interface to thread pool operations.
    /// </summary>
    public interface IThreadPoolEnlightenment
    {
        /// <summary>
        /// Registers <paramref name="handle"/> with the thread pool so that when it is signalled (or when a timeout occurs), <paramref name="callback"/> is invoked passing <paramref name="state"/> as an argument.
        /// </summary>
        /// <param name="handle">The handle to observe.</param>
        /// <param name="callback">The callback to invoke when the handle is signalled or when a timeout occurs.</param>
        /// <param name="state">The state object to pass to the callback.</param>
        /// <param name="timeout">The timeout after which the thread pool will no longer listen to the handle.</param>
        /// <returns>An object that, when disposed, unregisters the handle from the thread pool. This object should be disposed even if the handle is signalled or a timeout occurs.</returns>
        IDisposable RegisterWaitForSingleObject(WaitHandle handle, Action<object, bool> callback, object state, TimeSpan timeout);
    }
}

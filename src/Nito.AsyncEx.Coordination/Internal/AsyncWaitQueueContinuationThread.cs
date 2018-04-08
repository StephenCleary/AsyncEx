using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx.Internal
{
    /// <summary>
    /// Accessors for a helper thread used to schedule internal continuations to.
    /// </summary>
    public static class AsyncWaitQueueContinuationThread
    {
        /// <summary>
        /// The actual thread.
        /// </summary>
        public static AsyncContextThread Thread { get; } = new AsyncContextThread();

    }
}

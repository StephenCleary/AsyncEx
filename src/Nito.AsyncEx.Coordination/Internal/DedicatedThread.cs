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
    internal static class DedicatedThread
    {
        /// <summary>
        /// The actual thread.
        /// </summary>
        public static AsyncContextThread Thread { get; } = new AsyncContextThread();

        /// <summary>
        /// Gets the synchronization context for the dedicated thread.
        /// </summary>
        public static SynchronizationContext SynchronizationContext => Thread.Context.SynchronizationContext;

        public static void ApplyContext(Action action) => SynchronizationContextSwitcher.ApplyContext(SynchronizationContext, action);
        public static T ApplyContext<T>(Func<T> action) => SynchronizationContextSwitcher.ApplyContext(SynchronizationContext, action);
    }
}

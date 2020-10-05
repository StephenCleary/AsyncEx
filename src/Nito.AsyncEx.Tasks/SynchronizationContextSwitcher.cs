using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// Utility class for temporarily switching <see cref="SynchronizationContext"/> implementations.
    /// </summary>
    public sealed class SynchronizationContextSwitcher : Disposables.SingleDisposable<object>
    {
        /// <summary>
        /// The previous <see cref="SynchronizationContext"/>.
        /// </summary>
        private readonly SynchronizationContext? _oldContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizationContextSwitcher"/> class, installing the new <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <param name="newContext">The new <see cref="SynchronizationContext"/>. This can be <c>null</c> to remove an existing <see cref="SynchronizationContext"/>.</param>
        private SynchronizationContextSwitcher(SynchronizationContext? newContext)
            : base(new object())
        {
            _oldContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(newContext);
        }

        /// <summary>
        /// Restores the old <see cref="SynchronizationContext"/>.
        /// </summary>
        protected override void Dispose(object context)
        {
            SynchronizationContext.SetSynchronizationContext(_oldContext);
        }

        /// <summary>
        /// Executes a synchronous delegate without the current <see cref="SynchronizationContext"/>. The current context is restored when this function returns.
        /// </summary>
        /// <param name="action">The delegate to execute.</param>
        public static void NoContext(Action action)
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            using (new SynchronizationContextSwitcher(null))
                action();
        }

        /// <summary>
        /// Executes a synchronous or asynchronous delegate without the current <see cref="SynchronizationContext"/>. The current context is restored when this function synchronously returns.
        /// </summary>
        /// <param name="action">The delegate to execute.</param>
        public static T NoContext<T>(Func<T> action)
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            using (new SynchronizationContextSwitcher(null))
                return action();
        }

        /// <summary>
        /// Executes a synchronous delegate with the specified <see cref="SynchronizationContext"/> as "current". The previous current context is restored when this function returns.
        /// </summary>
        /// <param name="context">The context to treat as "current". May be <c>null</c> to indicate the thread pool context.</param>
        /// <param name="action">The delegate to execute.</param>
        public static void ApplyContext(SynchronizationContext context, Action action)
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            using (new SynchronizationContextSwitcher(context))
                action();
        }

        /// <summary>
        /// Executes a synchronous or asynchronous delegate without the specified <see cref="SynchronizationContext"/> as "current". The previous current context is restored when this function synchronously returns.
        /// </summary>
        /// <param name="context">The context to treat as "current". May be <c>null</c> to indicate the thread pool context.</param>
        /// <param name="action">The delegate to execute.</param>
        public static T ApplyContext<T>(SynchronizationContext context, Func<T> action)
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            using (new SynchronizationContextSwitcher(context))
                return action();
        }
    }
}

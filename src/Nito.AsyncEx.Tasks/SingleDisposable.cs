using System;
using System.Threading;

namespace Nito.AsyncEx
{
    /// <summary>
    /// A base class for disposables that need exactly-once semantics in a threadsafe way.
    /// </summary>
    /// <typeparam name="T">The type of "context" for the derived disposable.</typeparam>
    [Obsolete("Use Nito.Disposables.SingleDisposable instead.")]
    public abstract class SingleDisposable<T> : Disposables.SingleDisposable<T>
        where T : class
    {
        /// <summary>
        /// Creates a disposable for the specified context.
        /// </summary>
        /// <param name="context">The context passed to <see cref="Disposables.SingleDisposable{T}.Dispose(T)"/>. If this is <c>null</c>, then <see cref="Disposables.SingleDisposable{T}.Dispose(T)"/> will never be called.</param>
        protected SingleDisposable(T context)
            : base(context)
        {
        }
    }
}

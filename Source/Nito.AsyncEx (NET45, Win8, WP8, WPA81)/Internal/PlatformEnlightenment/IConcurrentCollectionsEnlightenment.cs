using System;
using System.Collections.Generic;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    /// <summary>
    /// A blocking queue.
    /// </summary>
    /// <typeparam name="T">The type of items contained in the queue.</typeparam>
    public interface IBlockingQueue<T> : IDisposable
    {
        /// <summary>
        /// Returns an enumerable that will not complete until <see cref="CompleteAdding"/> is called.
        /// </summary>
        IEnumerable<T> GetConsumingEnumerable();

        /// <summary>
        /// Returns an enumerable that enumerates this collection without taking any locks.
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCode]
        IEnumerable<T> EnumerateForDebugger();

        /// <summary>
        /// Attempts to add an item to the queue. If <see cref="CompleteAdding"/> has already been called, then this method returns <c>false</c>.
        /// </summary>
        /// <param name="item">The item to add.</param>
        bool TryAdd(T item);

        /// <summary>
        /// Notifies the queue that no more items will be added.
        /// </summary>
        void CompleteAdding();
    }
    
    /// <summary>
    /// Creates concurrent and blocking collections.
    /// </summary>
    public interface IConcurrentCollectionsEnlightenment
    {
        /// <summary>
        /// Creates a blocking queue.
        /// </summary>
        /// <typeparam name="T">The type of items contained in the queue.</typeparam>
        IBlockingQueue<T> CreateBlockingQueue<T>();
    }
}

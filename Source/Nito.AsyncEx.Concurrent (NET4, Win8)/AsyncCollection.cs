using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx.Internal;
using Nito.AsyncEx.Synchronous;

namespace Nito.AsyncEx
{
    /// <summary>
    /// An async-compatible producer/consumer collection.
    /// </summary>
    /// <typeparam name="T">The type of elements contained in the collection.</typeparam>
    [DebuggerDisplay("Count = {_collection.Count}, MaxCount = {_maxCount}")]
    [DebuggerTypeProxy(typeof(AsyncCollection<>.DebugView))]
    public sealed class AsyncCollection<T>
    {
        /// <summary>
        /// The underlying collection.
        /// </summary>
        private readonly IProducerConsumerCollection<T> _collection;

        /// <summary>
        /// The maximum number of elements allowed in the collection.
        /// </summary>
        private readonly int _maxCount;

        /// <summary>
        /// The mutual-exclusion lock protecting the collection.
        /// </summary>
        private readonly AsyncLock _mutex;

        /// <summary>
        /// A condition variable that is signalled when the collection is not full.
        /// </summary>
        private readonly AsyncConditionVariable _notFull;

        /// <summary>
        /// A condition variable that is signalled when the collection is completed or not empty.
        /// </summary>
        private readonly AsyncConditionVariable _completedOrNotEmpty;

        /// <summary>
        /// A cancellation token source that is canceled when the collection is marked completed for adding.
        /// </summary>
        private readonly CancellationTokenSource _completed;

        /// <summary>
        /// A cached result that is common when calling <see cref="o:AsyncCollectionExtensions.TryTakeFromAnyAsync"/>.
        /// </summary>
        internal static readonly TakeResult FalseResult = new TakeResult(null, default(T));

        /// <summary>
        /// Creates a new async-compatible producer/consumer collection wrapping the specified collection and with a maximum element count.
        /// </summary>
        /// <param name="collection">The collection to wrap.</param>
        /// <param name="maxCount">The maximum element count. This must be greater than zero.</param>
        public AsyncCollection(IProducerConsumerCollection<T> collection, int maxCount)
        {
            collection = collection ?? new ConcurrentQueue<T>();
            if (maxCount <= 0)
                throw new ArgumentOutOfRangeException("maxCount", "The maximum count must be greater than zero.");
            if (maxCount < collection.Count)
                throw new ArgumentException("The maximum count cannot be less than the number of elements in the collection.", "maxCount");
            _collection = collection;
            _maxCount = maxCount;
            _mutex = new AsyncLock();
            _notFull = new AsyncConditionVariable(_mutex);
            _completedOrNotEmpty = new AsyncConditionVariable(_mutex);
            _completed = new CancellationTokenSource();
        }

        /// <summary>
        /// Creates a new async-compatible producer/consumer collection wrapping the specified collection.
        /// </summary>
        /// <param name="collection">The collection to wrap.</param>
        public AsyncCollection(IProducerConsumerCollection<T> collection)
            : this(collection, int.MaxValue)
        {
        }

        /// <summary>
        /// Creates a new async-compatible producer/consumer collection with a maximum element count.
        /// </summary>
        /// <param name="maxCount">The maximum element count. This must be greater than zero.</param>
        public AsyncCollection(int maxCount)
            : this(null, maxCount)
        {
        }

        /// <summary>
        /// Creates a new async-compatible producer/consumer collection.
        /// </summary>
        public AsyncCollection()
            : this(null, int.MaxValue)
        {
        }

        /// <summary>
        /// Whether the collection is empty.
        /// </summary>
        private bool Empty { get { return _collection.Count == 0; } }

        /// <summary>
        /// Whether the collection is full.
        /// </summary>
        private bool Full { get { return _collection.Count == _maxCount; } }

        /// <summary>
        /// Asynchronously marks the producer/consumer collection as complete for adding.
        /// </summary>
        [Obsolete("Use CompleteAdding() instead.")]
        public async Task CompleteAddingAsync()
        {
            using (await _mutex.LockAsync().ConfigureAwait(false))
            {
                if (_completed.IsCancellationRequested)
                    return;
                _completed.Cancel();
                _completedOrNotEmpty.NotifyAll();
            }
        }

        /// <summary>
        /// Synchronously marks the producer/consumer collection as complete for adding.
        /// </summary>
        public void CompleteAdding()
        {
            using (_mutex.Lock())
            {
                if (_completed.IsCancellationRequested)
                    return;
                _completed.Cancel();
                _completedOrNotEmpty.NotifyAll();
            }
        }

        /// <summary>
        /// Attempts to add an item.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the add operation. If <paramref name="abort"/> is not <c>null</c>, then this token must include signals from the <paramref name="abort"/> object.</param>
        /// <param name="abort">A synchronization object used to cancel related add operations. May be <c>null</c> if this is the only add operation.</param>
        internal async Task<AsyncCollection<T>> TryAddAsync(T item, CancellationToken cancellationToken, TaskCompletionSource abort)
        {
            try
            {
                using (var combinedToken = CancellationTokenHelpers.Normalize(_completed.Token, cancellationToken))
                using (await _mutex.LockAsync().ConfigureAwait(false))
                {
                    // Wait for the collection to be not full.
                    while (Full)
                        await _notFull.WaitAsync(combinedToken.Token).ConfigureAwait(false);

                    // Explicitly check whether the collection has been marked complete to prevent a race condition where notFull is signalled at the same time the collection is marked complete.
                    if (_completed.IsCancellationRequested)
                        return null;

                    // Set the abort signal. If another collection has already set the abort signal, then abort.
                    if (abort != null && !abort.TrySetCanceled())
                        return null;

                    if (!_collection.TryAdd(item))
                        return null;
                    _completedOrNotEmpty.Notify();
                    return this;
                }
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to add an item. This method may block the calling thread.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the add operation.</param>
        internal AsyncCollection<T> DoTryAdd(T item, CancellationToken cancellationToken)
        {
            try
            {
                using (var combinedToken = CancellationTokenHelpers.Normalize(_completed.Token, cancellationToken))
                using (_mutex.Lock())
                {
                    // Wait for the collection to be not full.
                    while (Full)
                        _notFull.Wait(combinedToken.Token);

                    // Explicitly check whether the collection has been marked complete to prevent a race condition where notFull is signalled at the same time the collection is marked complete.
                    if (_completed.IsCancellationRequested)
                        return null;

                    if (!_collection.TryAdd(item))
                        return null;
                    _completedOrNotEmpty.Notify();
                    return this;
                }
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to add an item to the producer/consumer collection. Returns <c>false</c> if the producer/consumer collection has completed adding or if the item was rejected by the underlying collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the add operation.</param>
        public async Task<bool> TryAddAsync(T item, CancellationToken cancellationToken)
        {
            var ret = await TryAddAsync(item, cancellationToken, null).ConfigureAwait(false);
            if (ret != null)
                return true;
            cancellationToken.ThrowIfCancellationRequested();
            return false;
        }

        /// <summary>
        /// Attempts to add an item to the producer/consumer collection. Returns <c>false</c> if the producer/consumer collection has completed adding or if the item was rejected by the underlying collection. This method may block the calling thread.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the add operation.</param>
        public bool TryAdd(T item, CancellationToken cancellationToken)
        {
            var ret = DoTryAdd(item, cancellationToken);
            if (ret != null)
                return true;
            cancellationToken.ThrowIfCancellationRequested();
            return false;
        }

        /// <summary>
        /// Attempts to add an item to the producer/consumer collection. Returns <c>false</c> if the producer/consumer collection has completed adding or if the item was rejected by the underlying collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public Task<bool> TryAddAsync(T item)
        {
            return TryAddAsync(item, CancellationToken.None);
        }

        /// <summary>
        /// Attempts to add an item to the producer/consumer collection. Returns <c>false</c> if the producer/consumer collection has completed adding or if the item was rejected by the underlying collection. This method may block the calling thread.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public bool TryAdd(T item)
        {
            return TryAdd(item, CancellationToken.None);
        }

        /// <summary>
        /// Adds an item to the producer/consumer collection. Throws <see cref="InvalidOperationException"/> if the producer/consumer collection has completed adding or if the item was rejected by the underlying collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the add operation.</param>
        public async Task AddAsync(T item, CancellationToken cancellationToken)
        {
            var result = await TryAddAsync(item, cancellationToken).ConfigureAwait(false);
            if (!result)
                throw new InvalidOperationException("Add failed; the producer/consumer collection has completed adding or the underlying collection refused the item.");
        }

        /// <summary>
        /// Adds an item to the producer/consumer collection. Throws <see cref="InvalidOperationException"/> if the producer/consumer collection has completed adding or if the item was rejected by the underlying collection. This method may block the calling thread.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the add operation.</param>
        public void Add(T item, CancellationToken cancellationToken)
        {
            var result = TryAdd(item, cancellationToken);
            if (!result)
                throw new InvalidOperationException("Add failed; the producer/consumer collection has completed adding or the underlying collection refused the item.");
        }

        /// <summary>
        /// Adds an item to the producer/consumer collection. Throws <see cref="InvalidOperationException"/> if the producer/consumer collection has completed adding or if the item was rejected by the underlying collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public Task AddAsync(T item)
        {
            return AddAsync(item, CancellationToken.None);
        }

        /// <summary>
        /// Adds an item to the producer/consumer collection. Throws <see cref="InvalidOperationException"/> if the producer/consumer collection has completed adding or if the item was rejected by the underlying collection. This method may block the calling thread.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item)
        {
            Add(item, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously waits until an item is available to dequeue. Returns <c>false</c> if the producer/consumer collection has completed adding and there are no more items.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the asynchronous wait.</param>
        public async Task<bool> OutputAvailableAsync(CancellationToken cancellationToken)
        {
            using (await _mutex.LockAsync().ConfigureAwait(false))
            {
                while (!_completed.IsCancellationRequested && Empty)
                    await _completedOrNotEmpty.WaitAsync(cancellationToken).ConfigureAwait(false);
                return !Empty;
            }
        }

        /// <summary>
        /// Asynchronously waits until an item is available to dequeue. Returns <c>false</c> if the producer/consumer collection has completed adding and there are no more items.
        /// </summary>
        public Task<bool> OutputAvailableAsync()
        {
            return OutputAvailableAsync(CancellationToken.None);
        }

        /// <summary>
        /// Provides a (synchronous) consuming enumerable for items in the producer/consumer collection.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the synchronous enumeration.</param>
        public IEnumerable<T> GetConsumingEnumerable(CancellationToken cancellationToken)
        {
            while (true)
            {
                var result = DoTryTake(cancellationToken);
                if (!result.Success)
                    yield break;
                yield return result.Item;
            }
        }
        
        /// <summary>
        /// Provides a (synchronous) consuming enumerable for items in the producer/consumer queue.
        /// </summary>
        public IEnumerable<T> GetConsumingEnumerable()
        {
            return GetConsumingEnumerable(CancellationToken.None);
        }

        /// <summary>
        /// Attempts to take an item.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the take operation. If <paramref name="abort"/> is not <c>null</c>, then this token must include signals from the <paramref name="abort"/> object.</param>
        /// <param name="abort">A synchronization object used to cancel related take operations. May be <c>null</c> if this is the only take operation.</param>
        internal async Task<TakeResult> TryTakeAsync(CancellationToken cancellationToken, TaskCompletionSource abort)
        {
            try
            {
                using (await _mutex.LockAsync().ConfigureAwait(false))
                {
                    while (!_completed.IsCancellationRequested && Empty)
                        await _completedOrNotEmpty.WaitAsync(cancellationToken).ConfigureAwait(false);
                    if (_completed.IsCancellationRequested && Empty)
                        return FalseResult;
                    if (abort != null && !abort.TrySetCanceled())
                        return FalseResult;
                    T item;
                    if (!_collection.TryTake(out item))
                        return FalseResult;
                    _notFull.Notify();
                    return new TakeResult(this, item);
                }
            }
            catch (OperationCanceledException)
            {
                return FalseResult;
            }
        }

        /// <summary>
        /// Attempts to take an item. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the take operation.</param>
        internal TakeResult DoTryTake(CancellationToken cancellationToken)
        {
            try
            {
                using (_mutex.Lock())
                {
                    while (!_completed.IsCancellationRequested && Empty)
                        _completedOrNotEmpty.Wait(cancellationToken);
                    if (_completed.IsCancellationRequested && Empty)
                        return FalseResult;
                    T item;
                    if (!_collection.TryTake(out item))
                        return FalseResult;
                    _notFull.Notify();
                    return new TakeResult(this, item);
                }
            }
            catch (OperationCanceledException)
            {
                return FalseResult;
            }
        }

        /// <summary>
        /// Attempts to take an item from the producer/consumer collection.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the take operation.</param>
        public async Task<TakeResult> TryTakeAsync(CancellationToken cancellationToken)
        {
            var ret = await TryTakeAsync(cancellationToken, null).ConfigureAwait(false);
            if (ret.Success)
                return ret;
            cancellationToken.ThrowIfCancellationRequested();
            return ret;
        }

        /// <summary>
        /// Attempts to take an item from the producer/consumer collection. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the take operation.</param>
        public TakeResult TryTake(CancellationToken cancellationToken)
        {
            var ret = DoTryTake(cancellationToken);
            if (ret.Success)
                return ret;
            cancellationToken.ThrowIfCancellationRequested();
            return ret;
        }

        /// <summary>
        /// Attempts to take an item from the producer/consumer collection.
        /// </summary>
        public Task<TakeResult> TryTakeAsync()
        {
            return TryTakeAsync(CancellationToken.None);
        }

        /// <summary>
        /// Attempts to take an item from the producer/consumer collection. This method may block the calling thread.
        /// </summary>
        public TakeResult TryTake()
        {
            return TryTake(CancellationToken.None);
        }

        /// <summary>
        /// Takes an item from the producer/consumer collection. Returns the item. Throws <see cref="InvalidOperationException"/> if the producer/consumer collection has completed adding and is empty, or if the take from the underlying collection failed.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the take operation.</param>
        public async Task<T> TakeAsync(CancellationToken cancellationToken)
        {
            var ret = await TryTakeAsync(cancellationToken).ConfigureAwait(false);
            if (!ret.Success)
                throw new InvalidOperationException("Take failed; the producer/consumer collection has completed adding and is empty, or the take from the underlying collection failed.");
            return ret.Item;
        }

        /// <summary>
        /// Takes an item from the producer/consumer collection. Returns the item. Throws <see cref="InvalidOperationException"/> if the producer/consumer collection has completed adding and is empty, or if the take from the underlying collection failed. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the take operation.</param>
        public T Take(CancellationToken cancellationToken)
        {
            var ret = TryTake(cancellationToken);
            if (!ret.Success)
                throw new InvalidOperationException("Take failed; the producer/consumer collection has completed adding and is empty, or the take from the underlying collection failed.");
            return ret.Item;
        }

        /// <summary>
        /// Takes an item from the producer/consumer collection. Returns the item. Throws <see cref="InvalidOperationException"/> if the producer/consumer collection has completed adding and is empty, or if the take from the underlying collection failed.
        /// </summary>
        public Task<T> TakeAsync()
        {
            return TakeAsync(CancellationToken.None);
        }

        /// <summary>
        /// Takes an item from the producer/consumer collection. Returns the item. Throws <see cref="InvalidOperationException"/> if the producer/consumer collection has completed adding and is empty, or if the take from the underlying collection failed. This method may block the calling thread.
        /// </summary>
        public T Take()
        {
            return Take(CancellationToken.None);
        }

        /// <summary>
        /// The result of a <c>TryTake</c>, <c>TakeFromAny</c>, or <c>TryTakeFromAny</c> operation.
        /// </summary>
        public sealed class TakeResult
        {
            internal TakeResult(AsyncCollection<T> collection, T item)
            {
                Collection = collection;
                Item = item;
            }

            /// <summary>
            /// The collection from which the item was taken, or <c>null</c> if the operation failed.
            /// </summary>
            public AsyncCollection<T> Collection { get; private set; }

            /// <summary>
            /// Whether the operation was successful. This is <c>true</c> if and only if <see cref="Collection"/> is not <c>null</c>.
            /// </summary>
            public bool Success { get { return Collection != null; } }

            /// <summary>
            /// The item. This is only valid if <see cref="Collection"/> is not <c>null</c>.
            /// </summary>
            public T Item { get; private set; }
        }

        [DebuggerNonUserCode]
        internal sealed class DebugView
        {
            private readonly AsyncCollection<T> _collection;

            public DebugView(AsyncCollection<T> collection)
            {
                _collection = collection;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public T[] Items
            {
                get { return _collection._collection.ToArray(); }
            }
        }
    }

    /// <summary>
    /// Provides methods for working on multiple <see cref="AsyncCollection{T}"/> instances.
    /// </summary>
    public static class AsyncCollectionExtensions
    {
        /// <summary>
        /// Attempts to add an item to any of a number of producer/consumer collections. Returns the producer/consumer collection that received the item. Returns <c>null</c> if all producer/consumer collections have completed adding, or if any add operation on an underlying collection failed.
        /// </summary>
        /// <param name="collections">The producer/consumer collections.</param>
        /// <param name="item">The item to add.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the add operation.</param>
        /// <returns>The producer/consumer collection that received the item.</returns>
        public static async Task<AsyncCollection<T>> TryAddToAnyAsync<T>(this IEnumerable<AsyncCollection<T>> collections, T item, CancellationToken cancellationToken)
        {
            var abort = new TaskCompletionSource();
            using (var abortCancellationToken = CancellationTokenHelpers.FromTask(abort.Task))
            using (var combinedToken = CancellationTokenHelpers.Normalize(abortCancellationToken.Token, cancellationToken))
            {
                var token = combinedToken.Token;
                var tasks = collections.Select(q => q.TryAddAsync(item, token, abort));
                var results = await TaskShim.WhenAll(tasks).ConfigureAwait(false);
                var ret = results.FirstOrDefault(x => x != null);
                if (ret == null)
                    cancellationToken.ThrowIfCancellationRequested();
                return ret;
            }
        }

        /// <summary>
        /// Attempts to add an item to any of a number of producer/consumer collections. Returns the producer/consumer collection that received the item. Returns <c>null</c> if all producer/consumer collections have completed adding, or if any add operation on an underlying collection failed. This method may block the calling thread.
        /// </summary>
        /// <param name="collections">The producer/consumer collections.</param>
        /// <param name="item">The item to add.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the add operation.</param>
        /// <returns>The producer/consumer collection that received the item.</returns>
        public static AsyncCollection<T> TryAddToAny<T>(this IEnumerable<AsyncCollection<T>> collections, T item, CancellationToken cancellationToken)
        {
            return TryAddToAnyAsync(collections, item, cancellationToken).WaitAndUnwrapException();
        }

        /// <summary>
        /// Attempts to add an item to any of a number of producer/consumer collections. Returns the producer/consumer collection that received the item. Returns <c>null</c> if all producer/consumer collections have completed adding, or if any add operation on an underlying collection failed.
        /// </summary>
        /// <param name="collections">The producer/consumer collections.</param>
        /// <param name="item">The item to add.</param>
        /// <returns>The producer/consumer collection that received the item.</returns>
        public static Task<AsyncCollection<T>> TryAddToAnyAsync<T>(this IEnumerable<AsyncCollection<T>> collections, T item)
        {
            return TryAddToAnyAsync(collections, item, CancellationToken.None);
        }

        /// <summary>
        /// Attempts to add an item to any of a number of producer/consumer collections. Returns the producer/consumer collection that received the item. Returns <c>null</c> if all producer/consumer collections have completed adding, or if any add operation on an underlying collection failed. This method may block the calling thread.
        /// </summary>
        /// <param name="collections">The producer/consumer collections.</param>
        /// <param name="item">The item to add.</param>
        /// <returns>The producer/consumer collection that received the item.</returns>
        public static AsyncCollection<T> TryAddToAny<T>(this IEnumerable<AsyncCollection<T>> collections, T item)
        {
            return TryAddToAny(collections, item, CancellationToken.None);
        }

        /// <summary>
        /// Adds an item to any of a number of producer/consumer collections. Returns the producer/consumer collection that received the item. Throws <see cref="InvalidOperationException"/> if all producer/consumer collections have completed adding, or if any add operation on an underlying collection failed.
        /// </summary>
        /// <param name="collections">The producer/consumer collections.</param>
        /// <param name="item">The item to add.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the add operation.</param>
        /// <returns>The producer/consumer collection that received the item.</returns>
        public static async Task<AsyncCollection<T>> AddToAnyAsync<T>(this IEnumerable<AsyncCollection<T>> collections, T item, CancellationToken cancellationToken)
        {
            var ret = await TryAddToAnyAsync(collections, item, cancellationToken).ConfigureAwait(false);
            if (ret == null)
                throw new InvalidOperationException("Add failed; all producer/consumer collections have completed adding.");
            return ret;
        }

        /// <summary>
        /// Adds an item to any of a number of producer/consumer collections. Returns the producer/consumer collection that received the item. Throws <see cref="InvalidOperationException"/> if all producer/consumer collections have completed adding, or if any add operation on an underlying collection failed. This method may block the calling thread.
        /// </summary>
        /// <param name="collections">The producer/consumer collections.</param>
        /// <param name="item">The item to add.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the add operation.</param>
        /// <returns>The producer/consumer collection that received the item.</returns>
        public static AsyncCollection<T> AddToAny<T>(this IEnumerable<AsyncCollection<T>> collections, T item, CancellationToken cancellationToken)
        {
            var ret = TryAddToAny(collections, item, cancellationToken);
            if (ret == null)
                throw new InvalidOperationException("Add failed; all producer/consumer collections have completed adding.");
            return ret;
        }

        /// <summary>
        /// Adds an item to any of a number of producer/consumer collections. Returns the producer/consumer collection that received the item. Throws <see cref="InvalidOperationException"/> if all producer/consumer collections have completed adding, or if any add operation on an underlying collection failed.
        /// </summary>
        /// <param name="collections">The producer/consumer collections.</param>
        /// <param name="item">The item to add.</param>
        /// <returns>The producer/consumer collection that received the item.</returns>
        public static Task<AsyncCollection<T>> AddToAnyAsync<T>(this IEnumerable<AsyncCollection<T>> collections, T item)
        {
            return AddToAnyAsync(collections, item, CancellationToken.None);
        }

        /// <summary>
        /// Adds an item to any of a number of producer/consumer collections. Returns the producer/consumer collection that received the item. Throws <see cref="InvalidOperationException"/> if all producer/consumer collections have completed adding, or if any add operation on an underlying collection failed. This method may block the calling thread.
        /// </summary>
        /// <param name="collections">The producer/consumer collections.</param>
        /// <param name="item">The item to add.</param>
        /// <returns>The producer/consumer collection that received the item.</returns>
        public static AsyncCollection<T> AddToAny<T>(this IEnumerable<AsyncCollection<T>> collections, T item)
        {
            return AddToAny(collections, item, CancellationToken.None);
        }

        /// <summary>
        /// Attempts to take an item from any of a number of producer/consumer collections. The operation "fails" if all the producer/consumer collections have completed adding and are empty, or if any take operation on an underlying collection fails.
        /// </summary>
        /// <param name="collections">The producer/consumer collections.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the take operation.</param>
        public static async Task<AsyncCollection<T>.TakeResult> TryTakeFromAnyAsync<T>(this IEnumerable<AsyncCollection<T>> collections, CancellationToken cancellationToken)
        {
            var abort = new TaskCompletionSource();
            using (var abortCancellationToken = CancellationTokenHelpers.FromTask(abort.Task))
            using (var combinedToken = CancellationTokenHelpers.Normalize(abortCancellationToken.Token, cancellationToken))
            {
                var token = combinedToken.Token;
                var tasks = collections.Select(q => q.TryTakeAsync(token, abort));
                var results = await TaskShim.WhenAll(tasks).ConfigureAwait(false);
                var result = results.FirstOrDefault(x => x.Success);
                if (result != null)
                    return result;
                cancellationToken.ThrowIfCancellationRequested();
                return AsyncCollection<T>.FalseResult;
            }
        }

        /// <summary>
        /// Attempts to take an item from any of a number of producer/consumer collections. The operation "fails" if all the producer/consumer collections have completed adding and are empty, or if any take operation on an underlying collection fails. This method may block the calling thread.
        /// </summary>
        /// <param name="collections">The producer/consumer collections.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the take operation.</param>
        public static AsyncCollection<T>.TakeResult TryTakeFromAny<T>(this IEnumerable<AsyncCollection<T>> collections, CancellationToken cancellationToken)
        {
            return TryTakeFromAnyAsync(collections, cancellationToken).WaitAndUnwrapException();
        }

        /// <summary>
        /// Attempts to take an item from any of a number of producer/consumer collections. The operation "fails" if all the producer/consumer collections have completed adding and are empty, or if any take operation on an underlying collection fails.
        /// </summary>
        /// <param name="collections">The producer/consumer collections.</param>
        public static Task<AsyncCollection<T>.TakeResult> TryTakeFromAnyAsync<T>(this IEnumerable<AsyncCollection<T>> collections)
        {
            return TryTakeFromAnyAsync(collections, CancellationToken.None);
        }

        /// <summary>
        /// Attempts to take an item from any of a number of producer/consumer collections. The operation "fails" if all the producer/consumer collections have completed adding and are empty, or if any take operation on an underlying collection fails. This method may block the calling thread.
        /// </summary>
        /// <param name="collections">The producer/consumer collections.</param>
        public static AsyncCollection<T>.TakeResult TryTakeFromAny<T>(this IEnumerable<AsyncCollection<T>> collections)
        {
            return TryTakeFromAny(collections, CancellationToken.None);
        }

        /// <summary>
        /// Takes an item from any of a number of producer/consumer collections. Throws <see cref="InvalidOperationException"/> if all the producer/consumer collections have completed adding and are empty, or if any take operation on an underlying collection fails.
        /// </summary>
        /// <param name="collections">The array of producer/consumer collections.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the take operation.</param>
        public static async Task<AsyncCollection<T>.TakeResult> TakeFromAnyAsync<T>(this IEnumerable<AsyncCollection<T>> collections, CancellationToken cancellationToken)
        {
            var ret = await TryTakeFromAnyAsync(collections, cancellationToken).ConfigureAwait(false);
            if (!ret.Success)
                throw new InvalidOperationException("Take failed; all producer/consumer collections have completed adding and are empty.");
            return ret;
        }

        /// <summary>
        /// Takes an item from any of a number of producer/consumer collections. Throws <see cref="InvalidOperationException"/> if all the producer/consumer collections have completed adding and are empty, or if any take operation on an underlying collection fails. This method may block the calling thread.
        /// </summary>
        /// <param name="collections">The array of producer/consumer collections.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the take operation.</param>
        public static AsyncCollection<T>.TakeResult TakeFromAny<T>(this IEnumerable<AsyncCollection<T>> collections, CancellationToken cancellationToken)
        {
            var ret = TryTakeFromAny(collections, cancellationToken);
            if (!ret.Success)
                throw new InvalidOperationException("Take failed; all producer/consumer collections have completed adding and are empty.");
            return ret;
        }

        /// <summary>
        /// Takes an item from any of a number of producer/consumer collections. Throws <see cref="InvalidOperationException"/> if all the producer/consumer collections have completed adding and are empty, or if any take operation on an underlying collection fails.
        /// </summary>
        /// <param name="collections">The array of producer/consumer collections.</param>
        public static Task<AsyncCollection<T>.TakeResult> TakeFromAnyAsync<T>(this IEnumerable<AsyncCollection<T>> collections)
        {
            return TakeFromAnyAsync(collections, CancellationToken.None);
        }

        /// <summary>
        /// Takes an item from any of a number of producer/consumer collections. Throws <see cref="InvalidOperationException"/> if all the producer/consumer collections have completed adding and are empty, or if any take operation on an underlying collection fails. This method may block the calling thread.
        /// </summary>
        /// <param name="collections">The array of producer/consumer collections.</param>
        public static AsyncCollection<T>.TakeResult TakeFromAny<T>(this IEnumerable<AsyncCollection<T>> collections)
        {
            return TakeFromAny(collections, CancellationToken.None);
        }
    }
}

using System;
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
    /// An async-compatible producer/consumer queue.
    /// </summary>
    /// <typeparam name="T">The type of elements contained in the queue.</typeparam>
    [DebuggerDisplay("Count = {_queue.Count}, MaxCount = {_maxCount}")]
    [DebuggerTypeProxy(typeof(AsyncProducerConsumerQueue<>.DebugView))]
    public sealed class AsyncProducerConsumerQueue<T> : IDisposable
    {
        /// <summary>
        /// The underlying queue.
        /// </summary>
        private readonly Queue<T> _queue;

        /// <summary>
        /// The maximum number of elements allowed in the queue.
        /// </summary>
        private readonly int _maxCount;

        /// <summary>
        /// The mutual-exclusion lock protecting the queue.
        /// </summary>
        private readonly AsyncLock _mutex;

        /// <summary>
        /// A condition variable that is signalled when the queue is not full.
        /// </summary>
        private readonly AsyncConditionVariable _notFull;

        /// <summary>
        /// A condition variable that is signalled when the queue is completed or not empty.
        /// </summary>
        private readonly AsyncConditionVariable _completedOrNotEmpty;

        /// <summary>
        /// A cancellation token source that is canceled when the queue is marked completed for adding.
        /// </summary>
        private readonly CancellationTokenSource _completed;

        /// <summary>
        /// A cached result that is common when calling <see cref="o:AsyncProducerConsumerQueueExtensions.TryDequeueFromAnyAsync"/>.
        /// </summary>
        internal static readonly DequeueResult FalseResult = new DequeueResult(null, default(T));

        /// <summary>
        /// Creates a new async-compatible producer/consumer queue with the specified initial elements and a maximum element count.
        /// </summary>
        /// <param name="collection">The initial elements to place in the queue.</param>
        /// <param name="maxCount">The maximum element count. This must be greater than zero.</param>
        public AsyncProducerConsumerQueue(IEnumerable<T> collection, int maxCount)
        {
            if (maxCount <= 0)
                throw new ArgumentOutOfRangeException("maxCount", "The maximum count must be greater than zero.");
            _queue = collection == null ? new Queue<T>() : new Queue<T>(collection);
            if (maxCount < _queue.Count)
                throw new ArgumentException("The maximum count cannot be less than the number of elements in the collection.", "maxCount");
            _maxCount = maxCount;

            _mutex = new AsyncLock();
            _notFull = new AsyncConditionVariable(_mutex);
            _completedOrNotEmpty = new AsyncConditionVariable(_mutex);
            _completed = new CancellationTokenSource();
        }

        /// <summary>
        /// Creates a new async-compatible producer/consumer queue with the specified initial elements.
        /// </summary>
        /// <param name="collection">The initial elements to place in the queue.</param>
        public AsyncProducerConsumerQueue(IEnumerable<T> collection)
            : this(collection, int.MaxValue)
        {
        }

        /// <summary>
        /// Creates a new async-compatible producer/consumer queue with a maximum element count.
        /// </summary>
        /// <param name="maxCount">The maximum element count. This must be greater than zero.</param>
        public AsyncProducerConsumerQueue(int maxCount)
            : this(null, maxCount)
        {
        }

        /// <summary>
        /// Creates a new async-compatible producer/consumer queue.
        /// </summary>
        public AsyncProducerConsumerQueue()
            : this(null, int.MaxValue)
        {
        }

        /// <summary>
        /// Whether the queue is empty.
        /// </summary>
        private bool Empty { get { return _queue.Count == 0; } }

        /// <summary>
        /// Whether the queue is full.
        /// </summary>
        private bool Full { get { return _queue.Count == _maxCount; } }

        /// <summary>
        /// Releases resources held by this instance. After disposal, any use of this instance is undefined.
        /// </summary>
        public void Dispose()
        {
            _completed.Dispose();
        }

        /// <summary>
        /// Asynchronously marks the producer/consumer queue as complete for adding.
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
        /// Synchronously marks the producer/consumer queue as complete for adding.
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
        /// Attempts to enqueue an item.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the enqueue operation. If <paramref name="abort"/> is not <c>null</c>, then this token must include signals from the <paramref name="abort"/> object.</param>
        /// <param name="abort">A synchronization object used to cancel related enqueue operations. May be <c>null</c> if this is the only enqueue operation.</param>
        internal async Task<AsyncProducerConsumerQueue<T>> TryEnqueueAsync(T item, CancellationToken cancellationToken, TaskCompletionSource abort)
        {
            try
            {
                using (var combinedToken = CancellationTokenHelpers.Normalize(_completed.Token, cancellationToken))
                using (await _mutex.LockAsync().ConfigureAwait(false))
                {
                    // Wait for the queue to be not full.
                    while (Full)
                        await _notFull.WaitAsync(combinedToken.Token).ConfigureAwait(false);

                    // Explicitly check whether the queue has been marked complete to prevent a race condition where notFull is signalled at the same time the queue is marked complete.
                    if (_completed.IsCancellationRequested)
                        return null;

                    // Set the abort signal. If another queue has already set the abort signal, then abort.
                    if (abort != null && !abort.TrySetCanceled())
                        return null;

                    _queue.Enqueue(item);
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
        /// Attempts to enqueue an item. This method may block the calling thread.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the enqueue operation.</param>
        internal AsyncProducerConsumerQueue<T> DoTryEnqueue(T item, CancellationToken cancellationToken)
        {
            try
            {
                using (var combinedToken = CancellationTokenHelpers.Normalize(_completed.Token, cancellationToken))
                using (_mutex.Lock())
                {
                    // Wait for the queue to be not full.
                    while (Full)
                        _notFull.Wait(combinedToken.Token);

                    // Explicitly check whether the queue has been marked complete to prevent a race condition where notFull is signalled at the same time the queue is marked complete.
                    if (_completed.IsCancellationRequested)
                        return null;

                    _queue.Enqueue(item);
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
        /// Attempts to enqueue an item to the producer/consumer queue. Returns <c>false</c> if the producer/consumer queue has completed adding.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the enqueue operation.</param>
        public async Task<bool> TryEnqueueAsync(T item, CancellationToken cancellationToken)
        {
            var ret = await TryEnqueueAsync(item, cancellationToken, null).ConfigureAwait(false);
            if (ret != null)
                return true;
            cancellationToken.ThrowIfCancellationRequested();
            return false;
        }

        /// <summary>
        /// Attempts to enqueue an item to the producer/consumer queue. Returns <c>false</c> if the producer/consumer queue has completed adding. This method may block the calling thread.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the enqueue operation.</param>
        public bool TryEnqueue(T item, CancellationToken cancellationToken)
        {
            var ret = DoTryEnqueue(item, cancellationToken);
            if (ret != null)
                return true;
            cancellationToken.ThrowIfCancellationRequested();
            return false;
        }

        /// <summary>
        /// Attempts to enqueue an item to the producer/consumer queue. Returns <c>false</c> if the producer/consumer queue has completed adding.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        public Task<bool> TryEnqueueAsync(T item)
        {
            return TryEnqueueAsync(item, CancellationToken.None);
        }

        /// <summary>
        /// Attempts to enqueue an item to the producer/consumer queue. Returns <c>false</c> if the producer/consumer queue has completed adding. This method may block the calling thread.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        public bool TryEnqueue(T item)
        {
            return TryEnqueue(item, CancellationToken.None);
        }

        /// <summary>
        /// Enqueues an item to the producer/consumer queue. Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the enqueue operation.</param>
        public async Task EnqueueAsync(T item, CancellationToken cancellationToken)
        {
            var result = await TryEnqueueAsync(item, cancellationToken).ConfigureAwait(false);
            if (!result)
                throw new InvalidOperationException("Enqueue failed; the producer/consumer queue has completed adding.");
        }

        /// <summary>
        /// Enqueues an item to the producer/consumer queue. Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding. This method may block the calling thread.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the enqueue operation.</param>
        public void Enqueue(T item, CancellationToken cancellationToken)
        {
            var result = TryEnqueue(item, cancellationToken);
            if (!result)
                throw new InvalidOperationException("Enqueue failed; the producer/consumer queue has completed adding.");
        }

        /// <summary>
        /// Enqueues an item to the producer/consumer queue. Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        public Task EnqueueAsync(T item)
        {
            return EnqueueAsync(item, CancellationToken.None);
        }

        /// <summary>
        /// Enqueues an item to the producer/consumer queue. This method may block the calling thread. Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        public void Enqueue(T item)
        {
            Enqueue(item, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously waits until an item is available to dequeue. Returns <c>false</c> if the producer/consumer queue has completed adding and there are no more items.
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
        /// Asynchronously waits until an item is available to dequeue. Returns <c>false</c> if the producer/consumer queue has completed adding and there are no more items.
        /// </summary>
        public Task<bool> OutputAvailableAsync()
        {
            return OutputAvailableAsync(CancellationToken.None);
        }

        /// <summary>
        /// Provides a (synchronous) consuming enumerable for items in the producer/consumer queue.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the synchronous enumeration.</param>
        public IEnumerable<T> GetConsumingEnumerable(CancellationToken cancellationToken)
        {
            while (true)
            {
                var result = DoTryDequeue(cancellationToken);
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
        /// Attempts to dequeue an item.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the dequeue operation. If <paramref name="abort"/> is not <c>null</c>, then this token must include signals from the <paramref name="abort"/> object.</param>
        /// <param name="abort">A synchronization object used to cancel related dequeue operations. May be <c>null</c> if this is the only dequeue operation.</param>
        internal async Task<DequeueResult> TryDequeueAsync(CancellationToken cancellationToken, TaskCompletionSource abort)
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
                    var item = _queue.Dequeue();
                    _notFull.Notify();
                    return new DequeueResult(this, item);
                }
            }
            catch (OperationCanceledException)
            {
                return FalseResult;
            }
        }

        /// <summary>
        /// Attempts to dequeue an item. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the dequeue operation.</param>
        internal DequeueResult DoTryDequeue(CancellationToken cancellationToken)
        {
            try
            {
                using (_mutex.Lock())
                {
                    while (!_completed.IsCancellationRequested && Empty)
                        _completedOrNotEmpty.Wait(cancellationToken);
                    if (_completed.IsCancellationRequested && Empty)
                        return FalseResult;
                    var item = _queue.Dequeue();
                    _notFull.Notify();
                    return new DequeueResult(this, item);
                }
            }
            catch (OperationCanceledException)
            {
                return FalseResult;
            }
        }

        /// <summary>
        /// Attempts to dequeue an item from the producer/consumer queue.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the dequeue operation.</param>
        public async Task<DequeueResult> TryDequeueAsync(CancellationToken cancellationToken)
        {
            var ret = await TryDequeueAsync(cancellationToken, null).ConfigureAwait(false);
            if (ret.Success)
                return ret;
            cancellationToken.ThrowIfCancellationRequested();
            return ret;
        }

        /// <summary>
        /// Attempts to dequeue an item from the producer/consumer queue. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the dequeue operation.</param>
        public DequeueResult TryDequeue(CancellationToken cancellationToken)
        {
            var ret = DoTryDequeue(cancellationToken);
            if (ret.Success)
                return ret;
            cancellationToken.ThrowIfCancellationRequested();
            return ret;
        }

        /// <summary>
        /// Attempts to dequeue an item from the producer/consumer queue.
        /// </summary>
        public Task<DequeueResult> TryDequeueAsync()
        {
            return TryDequeueAsync(CancellationToken.None);
        }

        /// <summary>
        /// Attempts to dequeue an item from the producer/consumer queue. This method may block the calling thread.
        /// </summary>
        public DequeueResult TryDequeue()
        {
            return TryDequeue(CancellationToken.None);
        }

        /// <summary>
        /// Dequeues an item from the producer/consumer queue. Returns the dequeued item. Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding and is empty.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the dequeue operation.</param>
        /// <returns>The dequeued item.</returns>
        public async Task<T> DequeueAsync(CancellationToken cancellationToken)
        {
            var ret = await TryDequeueAsync(cancellationToken).ConfigureAwait(false);
            if (!ret.Success)
                throw new InvalidOperationException("Dequeue failed; the producer/consumer queue has completed adding and is empty.");
            return ret.Item;
        }

        /// <summary>
        /// Dequeues an item from the producer/consumer queue. Returns the dequeued item. This method may block the calling thread. Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding and is empty.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the dequeue operation.</param>
        public T Dequeue(CancellationToken cancellationToken)
        {
            var ret = TryDequeue(cancellationToken);
            if (!ret.Success)
                throw new InvalidOperationException("Dequeue failed; the producer/consumer queue has completed adding and is empty.");
            return ret.Item;
        }

        /// <summary>
        /// Dequeues an item from the producer/consumer queue. Returns the dequeued item. Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding and is empty.
        /// </summary>
        /// <returns>The dequeued item.</returns>
        public Task<T> DequeueAsync()
        {
            return DequeueAsync(CancellationToken.None);
        }

        /// <summary>
        /// Dequeues an item from the producer/consumer queue. Returns the dequeued item. This method may block the calling thread. Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding and is empty.
        /// </summary>
        /// <returns>The dequeued item.</returns>
        public T Dequeue()
        {
            return Dequeue(CancellationToken.None);
        }

        /// <summary>
        /// The result of a <c>TryDequeue</c>, <c>DequeueFromAny</c>, or <c>TryDequeueFromAny</c> operation.
        /// </summary>
        public sealed class DequeueResult
        {
            internal DequeueResult(AsyncProducerConsumerQueue<T> queue, T item)
            {
                Queue = queue;
                Item = item;
            }

            /// <summary>
            /// The queue from which the item was dequeued, or <c>null</c> if the operation failed.
            /// </summary>
            public AsyncProducerConsumerQueue<T> Queue { get; private set; }

            /// <summary>
            /// Whether the operation was successful. This is <c>true</c> if and only if <see cref="Queue"/> is not <c>null</c>.
            /// </summary>
            public bool Success { get { return Queue != null; } }

            /// <summary>
            /// The dequeued item. This is only valid if <see cref="Queue"/> is not <c>null</c>.
            /// </summary>
            public T Item { get; private set; }
        }

        [DebuggerNonUserCode]
        internal sealed class DebugView
        {
            private readonly AsyncProducerConsumerQueue<T> _queue;

            public DebugView(AsyncProducerConsumerQueue<T> queue)
            {
                _queue = queue;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public T[] Items
            {
                get { return _queue._queue.ToArray(); }
            }
        }
    }

    /// <summary>
    /// Provides methods for working on multiple <see cref="AsyncProducerConsumerQueue{T}"/> instances.
    /// </summary>
    public static class AsyncProducerConsumerQueueExtensions
    {
        /// <summary>
        /// Attempts to enqueue an item to any of a number of producer/consumer queues. Returns the producer/consumer queue that received the item. Returns <c>null</c> if all producer/consumer queues have completed adding.
        /// </summary>
        /// <param name="queues">The producer/consumer queues.</param>
        /// <param name="item">The item to enqueue.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the enqueue operation.</param>
        /// <returns>The producer/consumer queue that received the item.</returns>
        public static async Task<AsyncProducerConsumerQueue<T>> TryEnqueueToAnyAsync<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues, T item, CancellationToken cancellationToken)
        {
            var abort = new TaskCompletionSource();
            using (var abortCancellationToken = CancellationTokenHelpers.FromTask(abort.Task))
            using (var combinedToken = CancellationTokenHelpers.Normalize(abortCancellationToken.Token, cancellationToken))
            {
                var token = combinedToken.Token;
                var tasks = queues.Select(q => q.TryEnqueueAsync(item, token, abort));
                var results = await TaskShim.WhenAll(tasks).ConfigureAwait(false);
                var ret = results.FirstOrDefault(x => x != null);
                if (ret == null)
                    cancellationToken.ThrowIfCancellationRequested();
                return ret;
            }
        }

        /// <summary>
        /// Attempts to enqueue an item to any of a number of producer/consumer queues. Returns the producer/consumer queue that received the item. Returns <c>null</c> if all producer/consumer queues have completed adding. This method may block the calling thread.
        /// </summary>
        /// <param name="queues">The producer/consumer queues.</param>
        /// <param name="item">The item to enqueue.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the enqueue operation.</param>
        /// <returns>The producer/consumer queue that received the item.</returns>
        public static AsyncProducerConsumerQueue<T> TryEnqueueToAny<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues, T item, CancellationToken cancellationToken)
        {
            return TryEnqueueToAnyAsync(queues, item, cancellationToken).WaitAndUnwrapException();
        }

        /// <summary>
        /// Attempts to enqueue an item to any of a number of producer/consumer queues. Returns the producer/consumer queue that received the item. Returns <c>null</c> if all producer/consumer queues have completed adding.
        /// </summary>
        /// <param name="queues">The producer/consumer queues.</param>
        /// <param name="item">The item to enqueue.</param>
        /// <returns>The producer/consumer queue that received the item.</returns>
        public static Task<AsyncProducerConsumerQueue<T>> TryEnqueueToAnyAsync<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues, T item)
        {
            return TryEnqueueToAnyAsync(queues, item, CancellationToken.None);
        }

        /// <summary>
        /// Attempts to enqueue an item to any of a number of producer/consumer queues. Returns the producer/consumer queue that received the item. Returns <c>null</c> if all producer/consumer queues have completed adding. This method may block the calling thread.
        /// </summary>
        /// <param name="queues">The producer/consumer queues.</param>
        /// <param name="item">The item to enqueue.</param>
        /// <returns>The producer/consumer queue that received the item.</returns>
        public static AsyncProducerConsumerQueue<T> TryEnqueueToAny<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues, T item)
        {
            return TryEnqueueToAny(queues, item, CancellationToken.None);
        }

        /// <summary>
        /// Enqueues an item to any of a number of producer/consumer queues. Returns the producer/consumer queue that received the item. Throws <see cref="InvalidOperationException"/> if all producer/consumer queues have completed adding.
        /// </summary>
        /// <param name="queues">The producer/consumer queues.</param>
        /// <param name="item">The item to enqueue.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the enqueue operation.</param>
        /// <returns>The producer/consumer queue that received the item.</returns>
        public static async Task<AsyncProducerConsumerQueue<T>> EnqueueToAnyAsync<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues, T item, CancellationToken cancellationToken)
        {
            var ret = await TryEnqueueToAnyAsync(queues, item, cancellationToken).ConfigureAwait(false);
            if (ret == null)
                throw new InvalidOperationException("Enqueue failed; all producer/consumer queues have completed adding.");
            return ret;
        }

        /// <summary>
        /// Enqueues an item to any of a number of producer/consumer queues. Returns the producer/consumer queue that received the item. Throws <see cref="InvalidOperationException"/> if all producer/consumer queues have completed adding. This method may block the calling thread.
        /// </summary>
        /// <param name="queues">The producer/consumer queues.</param>
        /// <param name="item">The item to enqueue.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the enqueue operation.</param>
        /// <returns>The producer/consumer queue that received the item.</returns>
        public static AsyncProducerConsumerQueue<T> EnqueueToAny<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues, T item, CancellationToken cancellationToken)
        {
            var ret = TryEnqueueToAny(queues, item, cancellationToken);
            if (ret == null)
                throw new InvalidOperationException("Enqueue failed; all producer/consumer queues have completed adding.");
            return ret;
        }

        /// <summary>
        /// Enqueues an item to any of a number of producer/consumer queues. Returns the producer/consumer queue that received the item. Throws <see cref="InvalidOperationException"/> if all producer/consumer queues have completed adding.
        /// </summary>
        /// <param name="queues">The producer/consumer queues.</param>
        /// <param name="item">The item to enqueue.</param>
        /// <returns>The producer/consumer queue that received the item.</returns>
        public static Task<AsyncProducerConsumerQueue<T>> EnqueueToAnyAsync<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues, T item)
        {
            return EnqueueToAnyAsync(queues, item, CancellationToken.None);
        }

        /// <summary>
        /// Enqueues an item to any of a number of producer/consumer queues. Returns the producer/consumer queue that received the item. Throws <see cref="InvalidOperationException"/> if all producer/consumer queues have completed adding. This method may block the calling thread.
        /// </summary>
        /// <param name="queues">The producer/consumer queues.</param>
        /// <param name="item">The item to enqueue.</param>
        /// <returns>The producer/consumer queue that received the item.</returns>
        public static AsyncProducerConsumerQueue<T> EnqueueToAny<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues, T item)
        {
            return EnqueueToAny(queues, item, CancellationToken.None);
        }

        /// <summary>
        /// Attempts to dequeue an item from any of a number of producer/consumer queues. The operation "fails" if all the producer/consumer queues have completed adding and are empty.
        /// </summary>
        /// <param name="queues">The producer/consumer queues.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the dequeue operation.</param>
        public static async Task<AsyncProducerConsumerQueue<T>.DequeueResult> TryDequeueFromAnyAsync<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues, CancellationToken cancellationToken)
        {
            var abort = new TaskCompletionSource();
            using (var abortCancellationToken = CancellationTokenHelpers.FromTask(abort.Task))
            using (var combinedToken = CancellationTokenHelpers.Normalize(abortCancellationToken.Token, cancellationToken))
            {
                var token = combinedToken.Token;
                var tasks = queues.Select(q => q.TryDequeueAsync(token, abort));
                var results = await TaskShim.WhenAll(tasks).ConfigureAwait(false);
                var result = results.FirstOrDefault(x => x.Success);
                if (result != null)
                    return result;
                cancellationToken.ThrowIfCancellationRequested();
                return AsyncProducerConsumerQueue<T>.FalseResult;
            }
        }

        /// <summary>
        /// Attempts to dequeue an item from any of a number of producer/consumer queues. The operation "fails" if all the producer/consumer queues have completed adding and are empty. This method may block the calling thread.
        /// </summary>
        /// <param name="queues">The producer/consumer queues.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the dequeue operation.</param>
        public static AsyncProducerConsumerQueue<T>.DequeueResult TryDequeueFromAny<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues, CancellationToken cancellationToken)
        {
            return TryDequeueFromAnyAsync(queues, cancellationToken).WaitAndUnwrapException();
        }

        /// <summary>
        /// Attempts to dequeue an item from any of a number of producer/consumer queues. The operation "fails" if all the producer/consumer queues have completed adding and are empty.
        /// </summary>
        /// <param name="queues">The producer/consumer queues.</param>
        public static Task<AsyncProducerConsumerQueue<T>.DequeueResult> TryDequeueFromAnyAsync<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues)
        {
            return TryDequeueFromAnyAsync(queues, CancellationToken.None);
        }

        /// <summary>
        /// Attempts to dequeue an item from any of a number of producer/consumer queues. The operation "fails" if all the producer/consumer queues have completed adding and are empty. This method may block the calling thread.
        /// </summary>
        /// <param name="queues">The producer/consumer queues.</param>
        public static AsyncProducerConsumerQueue<T>.DequeueResult TryDequeueFromAny<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues)
        {
            return TryDequeueFromAny(queues, CancellationToken.None);
        }

        /// <summary>
        /// Dequeues an item from any of a number of producer/consumer queues. Throws <see cref="InvalidOperationException"/> if all the producer/consumer queues have completed adding and are empty.
        /// </summary>
        /// <param name="queues">The producer/consumer queues.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the dequeue operation.</param>
        public static async Task<AsyncProducerConsumerQueue<T>.DequeueResult> DequeueFromAnyAsync<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues, CancellationToken cancellationToken)
        {
            var ret = await TryDequeueFromAnyAsync(queues, cancellationToken).ConfigureAwait(false);
            if (!ret.Success)
                throw new InvalidOperationException("Dequeue failed; all producer/consumer queues have completed adding and are empty.");
            return ret;
        }

        /// <summary>
        /// Dequeues an item from any of a number of producer/consumer queues. Throws <see cref="InvalidOperationException"/> if all the producer/consumer queues have completed adding and are empty. This method may block the calling thread.
        /// </summary>
        /// <param name="queues">The producer/consumer queues.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the dequeue operation.</param>
        public static AsyncProducerConsumerQueue<T>.DequeueResult DequeueFromAny<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues, CancellationToken cancellationToken)
        {
            var ret = TryDequeueFromAny(queues, cancellationToken);
            if (!ret.Success)
                throw new InvalidOperationException("Dequeue failed; all producer/consumer queues have completed adding and are empty.");
            return ret;
        }

        /// <summary>
        /// Dequeues an item from any of a number of producer/consumer queues. Throws <see cref="InvalidOperationException"/> if all the producer/consumer queues have completed adding and are empty.
        /// </summary>
        /// <param name="queues">The producer/consumer queues.</param>
        public static Task<AsyncProducerConsumerQueue<T>.DequeueResult> DequeueFromAnyAsync<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues)
        {
            return DequeueFromAnyAsync(queues, CancellationToken.None);
        }

        /// <summary>
        /// Dequeues an item from any of a number of producer/consumer queues. Throws <see cref="InvalidOperationException"/> if all the producer/consumer queues have completed adding and are empty. This method may block the calling thread.
        /// </summary>
        /// <param name="queues">The producer/consumer queues.</param>
        public static AsyncProducerConsumerQueue<T>.DequeueResult DequeueFromAny<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues)
        {
            return DequeueFromAny(queues, CancellationToken.None);
        }
    }
}

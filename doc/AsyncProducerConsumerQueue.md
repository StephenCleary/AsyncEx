## Overview

An `AsyncProducerConsumerQueue` is a queue of items that provides `async`-compatible *Enqueue* and *Dequeue* operations. [[AsyncCollection]] is more flexible than this type, but it is not available on all platforms.

## API

```C#
// An async-compatible producer/consumer queue.
public sealed class AsyncProducerConsumerQueue<T> : IDisposable
{
  // Creates a new async-compatible producer/consumer queue with the specified initial elements and an optional maximum element count.
  public AsyncProducerConsumerQueue(IEnumerable<T> collection, int maxCount = int.MaxValue);

  // Creates a new async-compatible producer/consumer queue with an optional maximum element count.
  public AsyncProducerConsumerQueue(int maxCount = int.MaxValue);

  // Marks the producer/consumer queue as complete for adding.
  public async Task CompleteAddingAsync();

  // Enqueues an item to the producer/consumer queue.
  // Returns <c>false</c> if the producer/consumer queue has completed adding.
  public Task<bool> TryEnqueueAsync(T item, CancellationToken cancellationToken = new CancellationToken());

  // Enqueues an item to the producer/consumer queue.
  // Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding.
  public Task EnqueueAsync(T item, CancellationToken cancellationToken = new CancellationToken());

  // Attempts to dequeue an item from the producer/consumer queue.
  public Task<DequeueResult> TryDequeueAsync(CancellationToken cancellationToken = new CancellationToken());

  // Dequeues an item from the producer/consumer queue.
  // Returns the dequeued item.
  // Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding and is empty.
  public Task<T> DequeueAsync(CancellationToken cancellationToken = new CancellationToken());

  // The result of a <c>TryDequeue</c>, <c>DequeueFromAny</c>, or <c>TryDequeueFromAny</c> operation.
  public sealed class DequeueResult
  {
    // The queue from which the item was dequeued, or <c>null</c> if the operation failed.
    public AsyncProducerConsumerQueue<T> Queue { get; }

    // Whether the operation was successful.
    // This is <c>true</c> if and only if <see cref="Queue"/> is not <c>null</c>.
    public bool Success { get; }

    // The dequeued item.
    // This is only valid if <see cref="Queue"/> is not <c>null</c>.
    public T Item { get; }
  }
}

// Provides methods for working on multiple <see cref="AsyncProducerConsumerQueue{T}"/> instances.
public static class AsyncProducerConsumerQueueExtensions
{
  // Attempts to enqueue an item to any of a number of producer/consumer queues.
  // Returns the producer/consumer queue that received the item.
  // Returns <c>null</c> if all producer/consumer queues have completed adding.
  public static Task<AsyncProducerConsumerQueue<T>> TryEnqueueToAnyAsync<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues, T item, CancellationToken cancellationToken = new CancellationToken());

  // Enqueues an item to any of a number of producer/consumer queues.
  // Returns the producer/consumer queue that received the item.
  // Throws <see cref="InvalidOperationException"/> if all producer/consumer queues have completed adding.
  public static Task<AsyncProducerConsumerQueue<T>> EnqueueToAnyAsync<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues, T item, CancellationToken cancellationToken = new CancellationToken());

  // Attempts to dequeue an item from any of a number of producer/consumer queues.
  // The operation "fails" if all the producer/consumer queues have completed adding and are empty.
  public static Task<AsyncProducerConsumerQueue<T>.DequeueResult> TryDequeueFromAnyAsync<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues, CancellationToken cancellationToken = new CancellationToken());

  // Dequeues an item from any of a number of producer/consumer queues.
  // Throws <see cref="InvalidOperationException"/> if all the producer/consumer queues have completed adding and are empty.
  public static Task<AsyncProducerConsumerQueue<T>.DequeueResult> DequeueFromAnyAsync<T>(this IEnumerable<AsyncProducerConsumerQueue<T>> queues, CancellationToken cancellationToken = new CancellationToken());
}
```

## Platform Support

The full API is supported on all platforms.
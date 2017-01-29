## Overview

An `AsyncCollection` is an `async`-compatible wrapper around [[IProducerConsumerCollection|http://msdn.microsoft.com/en-us/library/dd287147.aspx]] collections such as [[ConcurrentQueue|http://msdn.microsoft.com/en-us/library/vstudio/dd267265.aspx]] or [[ConcurrentBag|http://msdn.microsoft.com/en-us/library/vstudio/dd381779.aspx]].

This makes `AsyncCollection` an `async` near-equivalent of [[BlockingCollection|http://msdn.microsoft.com/en-us/library/dd267312.aspx]], which is a blocking wrapper around [[IProducerConsumerCollection|http://msdn.microsoft.com/en-us/library/dd287147.aspx]].

## API

```C#
// An async-compatible producer/consumer collection.
public sealed class AsyncCollection<T>
{
  // Creates a new async-compatible producer/consumer collection wrapping the specified collection and with an optional maximum element count.
  public AsyncCollection(IProducerConsumerCollection<T> collection, int maxCount = int.MaxValue);

  // Creates a new async-compatible producer/consumer collection with an optional maximum element count.
  public AsyncCollection(int maxCount = int.MaxValue);

  // Marks the producer/consumer collection as complete for adding.
  public Task CompleteAddingAsync();

  // Attempts to add an item to the producer/consumer collection.
  // Returns <c>false</c> if the producer/consumer collection has completed adding or if the item was rejected by the underlying collection.
  public Task<bool> TryAddAsync(T item, CancellationToken cancellationToken = new CancellationToken());

  // Adds an item to the producer/consumer collection.
  // Throws <see cref="InvalidOperationException"/> if the producer/consumer collection has completed adding or if the item was rejected by the underlying collection.
  public Task AddAsync(T item, CancellationToken cancellationToken = new CancellationToken());

  // Attempts to take an item from the producer/consumer collection.
  public Task<TakeResult> TryTakeAsync(CancellationToken cancellationToken = new CancellationToken());

  // Takes an item from the producer/consumer collection.
  // Returns the item.
  // Throws <see cref="InvalidOperationException"/> if the producer/consumer collection has completed adding and is empty, or if the take from the underlying collection failed.
  public Task<T> TakeAsync(CancellationToken cancellationToken = new CancellationToken());

  // The result of a <c>TryTake</c>, <c>TakeFromAny</c>, or <c>TryTakeFromAny</c> operation.
  public sealed class TakeResult
  {
    // The collection from which the item was taken, or <c>null</c> if the operation failed.
    public AsyncCollection<T> Collection { get; }

    // Whether the operation was successful.
    // This is <c>true</c> if and only if <see cref="Collection"/> is not <c>null</c>.
    public bool Success { get; }

    // The item. This is only valid if <see cref="Collection"/> is not <c>null</c>.
    public T Item { get; }
  }
}

// Provides methods for working on multiple <see cref="AsyncCollection{T}"/> instances.
public static class AsyncCollectionExtensions
{
  // Attempts to add an item to any of a number of producer/consumer collections.
  // Returns the producer/consumer collection that received the item.
  // Returns <c>null</c> if all producer/consumer collections have completed adding, or if any add operation on an underlying collection failed.
  public static Task<AsyncCollection<T>> TryAddToAnyAsync<T>(this IEnumerable<AsyncCollection<T>> collections, T item, CancellationToken cancellationToken = new CancellationToken());

  // Adds an item to any of a number of producer/consumer collections.
  // Returns the producer/consumer collection that received the item.
  // Throws <see cref="InvalidOperationException"/> if all producer/consumer collections have completed adding, or if any add operation on an underlying collection failed.
  public static Task<AsyncCollection<T>> AddToAnyAsync<T>(this IEnumerable<AsyncCollection<T>> collections, T item, CancellationToken cancellationToken = new CancellationToken());

  // Attempts to take an item from any of a number of producer/consumer collections.
  // The operation "fails" if all the producer/consumer collections have completed adding and are empty, or if any take operation on an underlying collection fails.
  public static Task<AsyncCollection<T>.TakeResult> TryTakeFromAnyAsync<T>(this IEnumerable<AsyncCollection<T>> collections, CancellationToken cancellationToken = new CancellationToken());

  // Takes an item from any of a number of producer/consumer collections.
  // Throws <see cref="InvalidOperationException"/> if all the producer/consumer collections have completed adding and are empty, or if any take operation on an underlying collection fails.
  public static Task<AsyncCollection<T>.TakeResult> TakeFromAnyAsync<T>(this IEnumerable<AsyncCollection<T>> collections, CancellationToken cancellationToken = new CancellationToken());
}
```

## Platform Support

This type is not supported on the following platforms:
* Windows Phone Silverlight 8.0 / 7.5.
* Silverlight 5 / 4.

On these platforms, you can use [[AsyncProducerConsumerQueue]] instead.
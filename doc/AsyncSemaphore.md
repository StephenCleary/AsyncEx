## Overview

This is the `async`-ready equivalent of [[Semaphore|http://msdn.microsoft.com/en-us/library/system.threading.semaphore.aspx]], similar to Stephen Toub's [[AsyncSempahore|http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266983.aspx]]. Alternatively, you can use the [[SemaphoreSlim|http://msdn.microsoft.com/en-us/library/system.threading.semaphoreslim.aspx]] class, which is `async`-ready on .NET 4.5, Windows Store, and Windows Phone Application 8.1 platforms.

An `AsyncSemaphore` keeps a count, which is the number of open "slots" it has available to satisfy waiters. Any thread may increase the number of slots available by calling `Release`.

The task returned from `WaitAsync` will enter the `Completed` state when the `AsyncSemaphore` has given it a slot. That same task will enter the `Canceled` state if the `CancellationToken` is signaled before the wait is satisfied; in that case, the `AsyncSemaphore` does not lose a slot.

## API

```C#
// An async-compatible semaphore.
public sealed class AsyncSemaphore
{
  // Creates a new async-compatible semaphore with the specified initial count.
  public AsyncSemaphore(int initialCount);

  // Gets a semi-unique identifier for this asynchronous semaphore.
  public int Id { get; }

  // Gets the number of slots currently available on this semaphore.
  public int CurrentCount { get; }

  // Asynchronously waits for a slot in the semaphore to be available.
  public Task WaitAsync(CancellationToken cancellationToken = new CancellationToken());

  // Releases the semaphore.
  public void Release(int releaseCount = 1);
}
```

## Advanced Usage

You can call `WaitAsync` with an already-cancelled `CancellationToken` to attempt to acquire a slot from the `AsyncSemaphore` immediately without actually entering the wait queue.

## Platform Support

The full API is supported on all platforms.
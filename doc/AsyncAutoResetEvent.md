## Overview

This is the `async`-ready equivalent of [[AutoResetEvent|http://msdn.microsoft.com/en-us/library/system.threading.autoresetevent.aspx]], similar to Stephen Toub's [[AsyncAutoResetEvent|http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266923.aspx]].

Like other "events", an `AsyncAutoResetEvent` is either **set** or **unset** at any time. An `AsyncAutoResetEvent` can be changed from **unset** to **set** by calling its `Set` method. When a `WaitAsync` operation completes, the `AsyncAutoResetEvent` is automatically changed back to the **unset** state.

Moving an `AsyncAutoResetEvent` to the **set** state can only satisfy a single waiter. If there are multiple waiters when `Set` is called, only one will be released. (If this is not the behavior you want, use [[AsyncManualResetEvent]] instead).

When an `AsyncAutoResetEvent` is in the **set** state (with no waiters), `Set` is a noop. The `AsyncAutoResetEvent` will not remember how many times `Set` is called; those extra signals are "lost". (If this is not the behavior you want, use [[AsyncSemaphore]] instead).

The task returned from `WaitAsync` will enter the `Completed` state when the wait is satisfied and the `AsyncAutoResetEvent` has been automatically reset. That same task will enter the `Canceled` state if the `CancellationToken` is signaled before the wait is satisfied; in that case, the `AsyncAutoResetEvent` has not been automatically reset.

## API

```C#
// An async-compatible auto-reset event.
public sealed class AsyncAutoResetEvent
{
  // Creates an async-compatible auto-reset event.
  public AsyncAutoResetEvent(bool set = false);

  // Gets a semi-unique identifier for this asynchronous auto-reset event.
  public int Id { get; }

  // Asynchronously waits for this event to be set.
  // If the event is set, this method will auto-reset it and return immediately, even if the cancellation token is already signalled.
  // If the wait is canceled, then it will not auto-reset this event.
  public Task WaitAsync(CancellationToken cancellationToken = new CancellationToken());

  // Sets the event, atomically completing a task returned by WaitAsync.
  // If the event is already set, this method does nothing.
  public void Set();
}
```

## Advanced Usage

You can call `WaitAsync` with an already-cancelled `CancellationToken` to attempt to acquire the `AsyncAutoResetEvent` immediately without actually entering the wait queue.

## Platform Support

The full API is supported on all platforms.
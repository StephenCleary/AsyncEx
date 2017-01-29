## Overview

This is the `async`-ready equivalent of [[ManualResetEvent|http://msdn.microsoft.com/en-us/library/system.threading.manualresetevent.aspx]], similar to Stephen Toub's [[AsyncManualResetEvent|http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266920.aspx]].

Like other "events", an `AsyncManualResetEvent` is either **set** or **unset** at any time. An `AsyncManualResetEvent` can be changed from **unset** to **set** by calling its `Set` method, and it can be changed from **set** to **unset** by calling its `Reset` method.

When an `AsyncManualResetEvent` is in the **set** state, it will satisfy all waiters. Calling `Set` or `Reset` when the `AsyncManualResetEvent` is already in that state is a noop.

The task returned from `WaitAsync` will enter the `Completed` state when the `AsyncManualResetEvent` is in the **set** state.

## API

```C#
// An async-compatible manual-reset event.
public sealed class AsyncManualResetEvent
{
  // Creates an async-compatible manual-reset event.
  public AsyncManualResetEvent(); // Defaults to "unset"
  public AsyncManualResetEvent(bool set);

  // Waits for this event to be set.
  public Task WaitAsync();
  public void Wait();
  public void Wait(CancellationToken cancellationToken);

  // Sets the event, atomically completing every task returned by WaitAsync.
  // If the event is already set, this method does nothing.
  public void Set();

  // Resets the event.
  // If the event is already reset, this method does nothing.
  public void Reset();

  // Gets a semi-unique identifier for this asynchronous manual-reset event.
  public int Id { get; }
}
```

## Advanced Usage

`AsyncManualResetEvent` also supports synchronous waiting with the `Wait` method.

You can call `Wait` with an already-cancelled `CancellationToken` to test whether the `AsyncManualResetEvent` is in the **set** state.

## Design Notes

`WaitAsync` does not have explicit `CancellationToken` support because there are no state changes when a wait is satisfied.

## Platform Support

The full API is supported on all platforms.
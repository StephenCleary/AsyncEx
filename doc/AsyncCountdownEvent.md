## Overview

This is the `async`-ready almost-equivalent of [[CountdownEvent|http://msdn.microsoft.com/en-us/library/system.threading.countdownevent.aspx]], similar to Stephen Toub's [[AsyncCountdownEvent|http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx]]. It's only an *almost* equivalent because the `AsyncCountdownEvent` does not allow itself to be reset.

An `AsyncCountdownEvent` starts out **unset** and becomes **set** only once, when its **count** reaches zero. Its current count can be manipulated by any other tasks up until the time it reaches zero. When the count reaches zero, all waiting tasks are released.

The task returned from `WaitAsync` will enter the `Completed` state when the `AsyncCountdownEvent` has counted down to zero and enters the **set** state.

## API

```C#
// An async-compatible countdown event.
public sealed class AsyncCountdownEvent
{
  // Creates an async-compatible countdown event.
  public AsyncCountdownEvent(int count);

  // Gets a semi-unique identifier for this asynchronous countdown event.
  public int Id { get; }

  // Gets the current number of remaining signals before this event becomes set.
  public int CurrentCount { get; }

  // Asynchronously waits for this event to be set.
  public Task WaitAsync();

  // Attempts to add the specified value to the current count.
  // This method throws InvalidOperationException if the count is already at zero or if the new count would be greater than Int32.MaxValue.
  public void AddCount(int signalCount = 1);

  // Attempts to add the specified value to the current count.
  // This method returns false if the count is already at zero or if the new count would be greater than Int32.MaxValue.
  public bool TryAddCount(int signalCount = 1);

  // Attempts to subtract the specified value from the current count.
  // This method throws InvalidOperationException if the count is already at zero or if the new count would be less than zero.
  public void Signal(int signalCount = 1);

  // Attempts to subtract the specified value from the current count.
  // This method returns false if the count is already at zero or if the new count would be less than zero.
  public bool TrySignal(int signalCount = 1);
}
```

## Design Notes

`WaitAsync` does not have explicit `CancellationToken` support because there are no state changes when a wait is satisfied.

## Platform Support

The full API is supported on all platforms.
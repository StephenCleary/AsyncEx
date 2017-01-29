## Overview

The `DeferralManager` type simplifies the writing of "command-style" events. Regular events don't care when their handlers complete, so they work with `async` event handlers just fine. Command-style events need to know when their handlers are complete, even if they're `async`.

The `DeferralManager` type uses a pattern similar to [[WinRT deferrals|http://stackoverflow.com/questions/13421659/is-it-true-that-deferral-should-be-added-for-any-async-operation/13422319#13422319]]: the event argument type provides a `GetDeferral` and can detect when all deferrals have completed.

## API

```C#
// Manages the deferrals for a "command" event that may have asynchonous handlers.
public sealed class DeferralManager
{
  // Gets a deferral. The deferral is complete when disposed.
  public IDisposable GetDeferral();

  // Notifies the manager that all deferrals have been requested, and returns a task that is completed
  //   when all deferrals have completed.
  public Task SignalAndWaitAsync();
}
```

## Platform Support

The full API is supported on all platforms.
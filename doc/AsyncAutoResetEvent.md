## Overview

This is the `async`-ready equivalent of [AutoResetEvent](https://docs.microsoft.com/en-us/dotnet/api/system.threading.autoresetevent), similar to Stephen Toub's [AsyncAutoResetEvent](https://blogs.msdn.microsoft.com/pfxteam/2012/02/11/building-async-coordination-primitives-part-2-asyncautoresetevent/).

Like other "events", an `AsyncAutoResetEvent` is either **set** or **unset** at any time. An `AsyncAutoResetEvent` can be changed from **unset** to **set** by calling its `Set` method. When a `WaitAsync` operation completes, the `AsyncAutoResetEvent` is automatically changed back to the **unset** state.

Moving an `AsyncAutoResetEvent` to the **set** state can only satisfy a single waiter. If there are multiple waiters when `Set` is called, only one will be released. (If this is not the behavior you want, use [AsyncManualResetEvent](AsyncManualResetEvent.md) instead).

When an `AsyncAutoResetEvent` is in the **set** state (with no waiters), `Set` is a noop. The `AsyncAutoResetEvent` will not remember how many times `Set` is called; those extra signals are "lost". (If this is not the behavior you want, use [AsyncSemaphore](AsyncSemaphore.md) instead).

The task returned from `WaitAsync` will enter the `Completed` state when the wait is satisfied and the `AsyncAutoResetEvent` has been automatically reset. That same task will enter the `Canceled` state if the `CancellationToken` is signaled before the wait is satisfied; in that case, the `AsyncAutoResetEvent` has not been automatically reset.

## Advanced Usage

You can call `WaitAsync` with an [already-cancelled `CancellationToken`](Cancellation.md) to attempt to acquire the `AsyncAutoResetEvent` immediately without actually entering the wait queue.

## Overview

This is the `async`-ready equivalent of [ManualResetEvent](https://docs.microsoft.com/en-us/dotnet/api/system.threading.manualresetevent), similar to Stephen Toub's [AsyncManualResetEvent](https://blogs.msdn.microsoft.com/pfxteam/2012/02/11/building-async-coordination-primitives-part-1-asyncmanualresetevent/).

Like other "events", an `AsyncManualResetEvent` is either **set** or **unset** at any time. An `AsyncManualResetEvent` can be changed from **unset** to **set** by calling its `Set` method, and it can be changed from **set** to **unset** by calling its `Reset` method.

When an `AsyncManualResetEvent` is in the **set** state, it will satisfy all waiters. Calling `Set` or `Reset` when the `AsyncManualResetEvent` is already in that state is a noop.

The task returned from `WaitAsync` will enter the `Completed` state when the `AsyncManualResetEvent` is in the **set** state.

## Advanced Usage

`AsyncManualResetEvent` also supports synchronous waiting with the `Wait` method.

You can call `Wait` with an [already-cancelled `CancellationToken`](Cancellation.md) to test whether the `AsyncManualResetEvent` is in the **set** state.

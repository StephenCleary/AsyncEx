## Overview

This is the `async`-ready equivalent of [Semaphore](https://docs.microsoft.com/en-us/dotnet/api/system.threading.semaphore), similar to Stephen Toub's [AsyncSempahore](https://blogs.msdn.microsoft.com/pfxteam/2012/02/12/building-async-coordination-primitives-part-5-asyncsemaphore/). Alternatively, you can use the [SemaphoreSlim](https://docs.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim) class, which is `async`-ready on modern platforms.

An `AsyncSemaphore` keeps a count, which is the number of open "slots" it has available to satisfy waiters. Any thread may increase the number of slots available by calling `Release`.

The task returned from `WaitAsync` will enter the `Completed` state when the `AsyncSemaphore` has given it a slot. That same task will enter the `Canceled` state if the `CancellationToken` is signaled before the wait is satisfied; in that case, the `AsyncSemaphore` does not lose a slot.

## Advanced Usage

You can call `WaitAsync` with an [already-cancelled `CancellationToken`](Cancellation.md) to attempt to acquire a slot from the `AsyncSemaphore` immediately without actually entering the wait queue.

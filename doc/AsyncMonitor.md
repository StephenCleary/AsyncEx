## Overview

This is the `async`-ready almost-equivalent of [Monitor](https://docs.microsoft.com/en-us/dotnet/api/system.threading.monitor). It's only _almost_ equivalent because the `Monitor` type permits reentrancy, which is not currently possible to do with an `async`-ready lock.

An `AsyncMonitor` is an [AsyncLock](AsyncLock.md) with a single associated [AsyncConditionVariable](AsyncConditionVariable.md). It is either entered or not. The `AsyncMonitor` can be asynchronously entered by calling `EnterAsync`, and you can leave it by disposing the result of that task.

While in the monitor, a task may decide to wait for a signal by calling `WaitAsync`. While waiting, it temporarily leaves the monitor until it receives a signal and re-enters the monitor.

While in the monitor, a signalling task may choose to release only a single waiter (`Pulse`) or all waiters (`PulseAll`). If there are no waiters, the notification is "lost"; it is not remembered by the `AsyncMonitor`.

The task returned from `EnterAsync` will enter the `Completed` state when it has entered the monitor. That same task will enter the `Canceled` state if the `CancellationToken` is signaled before the wait is satisfied; in that case, the monitor is not entered by that task.

The task returned from `WaitAsync` will enter the `Completed` state when it receives a signal and re-enters the monitor. That same task will enter the `Canceled` state if the `CancellationToken` is signaled before the wait is satisfied; in that case, the task will wait to enter the `Canceled` state until it re-enters the monitor.

Remember that from the time `WaitAsync` is called to the time when its returned task completes, the calling task has _left_ the monitor.

Note that the correct logic for waiting on monitor signals is to wait in a loop until the required condition is true. This is necessary because other tasks may execute between the signal and the completion of the wait.

## Advanced Usage

You can call `EnterAsync` with an [already-cancelled `CancellationToken`](Cancellation.md) to attempt to enter the monitor immediately without actually entering the wait queue.

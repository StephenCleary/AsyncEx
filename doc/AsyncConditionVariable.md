## Overview

This is an `async`-ready [condition variable](https://en.wikipedia.org/wiki/Condition_variable), a classical synchronization primitive in computer science without a direct .NET equivalent.

An `AsyncConditionVariable` is associated with a single [AsyncLock](AsyncLock.md). All methods on the `AsyncConditionVariable` type ***require*** that you hold the associated lock before calling them. There's no runtime checks for these preconditions because there is no way to check them; you'll just have to be careful. I recommend you combine the `AsyncLock` with its associated `AsyncConditionVariable` instances into a higher-level custom lock/signal system. Note that the simple case of a single `AsyncLock` with a single `AsyncConditionVariable` is equivalent to [AsyncMonitor](AsyncMonitor.md).

Waiting on an `AsyncConditionVariable` enables an `async` method to (asynchronously) wait for some condition to become true. While waiting, that `async` method gives up the `AsyncLock`. When the condition becomes true, another `async` method notifies the `AsyncConditionVariable`, which re-acquires the `AsyncLock` and releases the waiter.

When notifying the `AsyncConditionVariable`, the notifying task may choose to release only a single waiter (`Notify`) or all waiters (`NotifyAll`). If there are no waiters, the notification is "lost"; it is not remembered by the `AsyncConditionVariable`.

The task returned from `WaitAsync` will enter the `Completed` state when it receives a notification and re-acquires the `AsyncLock`. That same task will enter the `Canceled` state if the `CancellationToken` is signaled before the wait is satisfied; in that case, the task will wait to enter the `Canceled` state until it re-acquires the `AsyncLock`.

Remember that from the time `WaitAsync` is called to the time when its returned task completes, the `AsyncLock` is _not_ held by the calling task.

Note that the correct logic for condition variables is to wait in a loop until the required condition is true. This is necessary because other tasks may execute between the notification and the completion of the wait.

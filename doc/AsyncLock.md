## Overview

This is the `async`-ready almost-equivalent of the `lock` keyword or the [`Mutex` type](https://docs.microsoft.com/en-us/dotnet/api/system.threading.mutex), similar to Stephen Toub's [AsyncLock](https://blogs.msdn.microsoft.com/pfxteam/2012/02/12/building-async-coordination-primitives-part-6-asynclock/). It's only _almost_ equivalent because the `lock` keyword permits reentrancy, which is not currently possible to do with an `async`-ready lock.

An `AsyncLock` is either taken or not. The lock can be asynchronously acquired by calling `LockAsync`, and it is released by disposing the result of that task. `AsyncLock` taken an optional `CancellationToken`, which can be used to cancel the acquiring of the lock.

The task returned from `LockAsync` will enter the `Completed` state when it has acquired the `AsyncLock`. That same task will enter the `Canceled` state if the `CancellationToken` is signaled before the wait is satisfied; in that case, the `AsyncLock` is not taken by that task.

## Example Usage

The vast majority of use cases are to just replace a `lock` statement. That is, with the original code looking like this:

```C#
private readonly object _mutex = new object();
public void DoStuff()
{
  lock (_mutex)
  {
    Thread.Sleep(TimeSpan.FromSeconds(1));
  }
}
```

If we want to replace the blocking operation `Thread.Sleep` with an asynchronous equivalent, it's not directly possible because of the `lock` block. We cannot `await` inside of a `lock`.

So, we use the `async`-compatible `AsyncLock` instead:

```C#
private readonly AsyncLock _mutex = new AsyncLock();
public async Task DoStuffAsync()
{
  using (await _mutex.LockAsync())
  {
    await Task.Delay(TimeSpan.FromSeconds(1));
  }
}
```

## Advanced Usage

`AsyncLock` also supports synchronous locking with the `Lock` method.

You can call `Lock` or `LockAsync` [with an already-cancelled `CancellationToken`](Cancellation.md) to attempt to acquire the `AsyncLock` immediately without actually entering the wait queue.

## Really Advanced Usage

The `AsyncLock` constructor can take an async wait queue; pass a custom wait queue to specify your own queueing logic.

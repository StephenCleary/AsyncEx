## Overview

This is the `async`-ready almost-equivalent of the `lock` keyword or the [[Mutex type|http://msdn.microsoft.com/en-us/library/system.threading.mutex.aspx]], similar to Stephen Toub's [[AsyncLock|http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266988.aspx]]. It's only _almost_ equivalent because the `lock` keyword permits reentrancy, which is not currently possible to do with an `async`-ready lock.

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

## API

```C#
// A mutual exclusion lock that is compatible with async. Note that this lock is *not* recursive!
public sealed class AsyncLock
{
  // Creates a new async-compatible mutual exclusion lock.
  public AsyncLock();
  public AsyncLock(IAsyncWaitQueue<IDisposable> queue);

  // Acquires the lock. Returns a disposable that releases the lock when disposed.
  public AwaitableDisposable<IDisposable> LockAsync(CancellationToken cancellationToken);
  public AwaitableDisposable<IDisposable> LockAsync();
  public IDisposable Lock(CancellationToken cancellationToken);
  public IDisposable Lock();

  // Gets a semi-unique identifier for this asynchronous lock.
  public int Id { get; }
}
```

## Advanced Usage

`AsyncLock` also supports synchronous locking with the `Lock` method.

You can call `Lock` or `LockAsync` with an already-cancelled `CancellationToken` to attempt to acquire the `AsyncLock` immediately without actually entering the wait queue.

## Really Advanced Usage

The `AsyncLock` constructor can take an [[async wait queue|Async wait queues]]; pass a custom wait queue to specify your own queueing logic. There are two examples of this in the source code (in the `AdvancedExamples` unit test project): a priority lock queue, and a recursive lock queue.

## Platform Support

The full API is supported on all platforms.
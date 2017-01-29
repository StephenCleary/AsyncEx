## Overview

This is the `async`-ready almost-equivalent of the the [[ReaderWriterLockSlim type|http://msdn.microsoft.com/en-us/library/system.threading.readerwriterlockslim.aspx]], similar to Stephen Toub's [[AsyncReaderWriterLock|http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/building-async-coordination-primitives-part-7-asyncreaderwriterlock.aspx]]. It's only _almost_ equivalent because the `ReaderWriterLockSlim` can be constructed in a way which allows reentrancy, and this is not currently possible to do with an `async`-ready lock.

There are three types of locks that can be taken on an `AsyncReaderWriterLock`:
* Write locks, which are fully exclusive. They do not allow other locks of any kind.
* Read locks, which permit other read locks but exclude write locks.
* Upgradeable read locks, which permit only read locks. They do not allow other upgradeable read locks or write locks.

An upgradeable read lock is initially a read lock, but it may upgrade to a write lock without giving up its read lock. To upgrade, it must wait for all other read locks to be released. Once it has upgraded, it may downgrade back to a read lock. An upgradeable read lock may upgrade or downgrade as many times as necessary.

Write, read, and upgradeable read locks may be asynchronously acquired by calling `WriterLockAsync`, `ReaderLockAsync`, or `UpgradeableReaderLockAsync`. These locks are released by disposing the result of the returned task.

The tasks returned from `WriterLockAsync`, `ReaderLockAsync`, and `UpgradeableReaderLockAsync` will enter the `Completed` state when they have acquired the `AsyncReaderWriterLock`. That same task will enter the `Canceled` state if the `CancellationToken` is signaled before the wait is satisfied; in that case, the `AsyncReaderWriterLock` is not taken by that task.

An upgradeable read lock may asynchronously upgrade by calling `UpgradeAsync` on the disposable returned by `UpgradeableReaderLockAsync`. The lock can be downgraded by disposing the result of the task returned by `UpgradeAsync`.

The task returned from `UpgradeAsync` will enter the `Completed` state when it has upgraded to a write lock. That same task will enter the `Canceled` state if the `CancellationToken` is signaled before the wait is satisfied; in that case, the upgradeable read lock remains a read lock.

## API

```C#
// A reader/writer lock that is compatible with async. Note that this lock is NOT recursive!
public sealed class AsyncReaderWriterLock
{
  // Creates a new async-compatible reader/writer lock.
  public AsyncReaderWriterLock();

  // Gets a semi-unique identifier for this asynchronous lock.
  public int Id { get; }

  // Asynchronously acquires the lock as a writer.
  // Returns a disposable that releases the lock when disposed.
  public Task<IDisposable> WriterLockAsync(CancellationToken cancellationToken = new CancellationToken());

  // Asynchronously acquires the lock as a reader.
  // Returns a disposable that releases the lock when disposed.
  public Task<IDisposable> ReaderLockAsync(CancellationToken cancellationToken = new CancellationToken());

  // Asynchronously acquires the lock as a reader with the option to upgrade.
  // Returns a key that can be used to upgrade and downgrade the lock, and releases the lock when disposed.
  public Task<UpgradeableReaderKey> UpgradeableReaderLockAsync(CancellationToken cancellationToken = new CancellationToken());

  // The disposable which manages the upgradeable reader lock.
  public sealed class UpgradeableReaderKey : IDisposable
  {
    // Gets a value indicating whether this lock has been upgraded to a write lock.
    public bool Upgraded { get; }

    // Upgrades the reader lock to a writer lock.
    // Returns a disposable that downgrades the writer lock to a reader lock when disposed.
    public Task<IDisposable> UpgradeAsync(CancellationToken cancellationToken = new CancellationToken());

    // Release the lock.
    public void Dispose();
  }
}
```

## Advanced Usage

You can call `WriterLockAsync`, `ReaderLockAsync`, and `UpgradeableReaderLockAsync` with an already-cancelled `CancellationToken` to attempt to acquire the `AsyncReaderWriterLock` immediately without actually entering the wait queue.

You can call *UpgradeAsync* with an already-cancelled *CancellationToken* to attempt to upgrade immediately without actually entering the wait queue.

## Platform Support

The full API is supported on all platforms.
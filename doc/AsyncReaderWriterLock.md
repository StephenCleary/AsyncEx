## Overview

This is the `async`-ready almost-equivalent of the the [ReaderWriterLockSlim type](https://docs.microsoft.com/en-us/dotnet/api/system.threading.readerwriterlockslim), similar to Stephen Toub's [AsyncReaderWriterLock](https://blogs.msdn.microsoft.com/pfxteam/2012/02/12/building-async-coordination-primitives-part-7-asyncreaderwriterlock/). It's only _almost_ equivalent because the `ReaderWriterLockSlim` supports upgradeable locks and can be constructed in a way which allows reentrancy, and this is not currently possible to do with an `async`-ready lock.

There are two types of locks that can be taken on an `AsyncReaderWriterLock`:
* Write locks, which are fully exclusive. They do not allow other locks of any kind.
* Read locks, which permit other read locks but exclude write locks.

Write and read locks may be asynchronously acquired by calling `WriterLockAsync` or `ReaderLockAsync`. These locks are released by disposing the result of the returned task.

The tasks returned from `WriterLockAsync` and `ReaderLockAsync` will enter the `Completed` state when they have acquired the `AsyncReaderWriterLock`. That same task will enter the `Canceled` state if the `CancellationToken` is signaled before the wait is satisfied; in that case, the `AsyncReaderWriterLock` is not taken by that task.

## Advanced Usage

You can call `WriterLockAsync` and `ReaderLockAsync` with an [already-cancelled `CancellationToken`](Cancellation.md) to attempt to acquire the `AsyncReaderWriterLock` immediately without actually entering the wait queue.

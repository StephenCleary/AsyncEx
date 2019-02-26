using Nito.AsyncEx;
using Nito.AsyncEx.Testing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class AsyncReaderWriterLockUnitTests
    {
        [Fact]
        public async Task Unlocked_PermitsWriterLock()
        {
            var rwl = new AsyncReaderWriterLock();
            await rwl.WriterLockAsync();
        }

        [Fact]
        public async Task Unlocked_PermitsMultipleReaderLocks()
        {
            var rwl = new AsyncReaderWriterLock();
            await rwl.ReaderLockAsync();
            await rwl.ReaderLockAsync();
        }

        [Fact]
        public async Task WriteLocked_PreventsAnotherWriterLock()
        {
            var rwl = new AsyncReaderWriterLock();
            await rwl.WriterLockAsync();
            var task = rwl.WriterLockAsync().AsTask();
            await AsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task WriteLocked_PreventsReaderLock()
        {
            var rwl = new AsyncReaderWriterLock();
            await rwl.WriterLockAsync();
            var task = rwl.ReaderLockAsync().AsTask();
            await AsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task WriteLocked_Unlocked_PermitsAnotherWriterLock()
        {
            var rwl = new AsyncReaderWriterLock();
            var firstWriteLockTaken = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            var releaseFirstWriteLock = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            var task = Task.Run(async () =>
            {
                using (await rwl.WriterLockAsync())
                {
                    firstWriteLockTaken.SetResult(null);
                    await releaseFirstWriteLock.Task;
                }
            });
            await firstWriteLockTaken.Task;
            var lockTask = rwl.WriterLockAsync().AsTask();
            Assert.False(lockTask.IsCompleted);
            releaseFirstWriteLock.SetResult(null);
            await lockTask;
        }

        [Fact]
        public async Task ReadLocked_PreventsWriterLock()
        {
            var rwl = new AsyncReaderWriterLock();
            await rwl.ReaderLockAsync();
            var task = rwl.WriterLockAsync().AsTask();
            await AsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public void Id_IsNotZero()
        {
            var rwl = new AsyncReaderWriterLock();
            Assert.NotEqual(0, rwl.Id);
        }

        [Fact]
        public void WriterLock_PreCancelled_LockAvailable_SynchronouslyTakesLock()
        {
            var rwl = new AsyncReaderWriterLock();
            var token = new CancellationToken(true);

            var task = rwl.WriterLockAsync(token).AsTask();

            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.False(task.IsFaulted);
        }

        [Fact]
        public void WriterLock_PreCancelled_LockNotAvailable_SynchronouslyCancels()
        {
            var rwl = new AsyncReaderWriterLock();
            var token = new CancellationToken(true);
            rwl.WriterLockAsync();

            var task = rwl.WriterLockAsync(token).AsTask();

            Assert.True(task.IsCompleted);
            Assert.True(task.IsCanceled);
            Assert.False(task.IsFaulted);
        }

        [Fact]
        public void ReaderLock_PreCancelled_LockAvailable_SynchronouslyTakesLock()
        {
            var rwl = new AsyncReaderWriterLock();
            var token = new CancellationToken(true);

            var task = rwl.ReaderLockAsync(token).AsTask();

            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.False(task.IsFaulted);
        }

        [Fact]
        public void ReaderLock_PreCancelled_LockNotAvailable_SynchronouslyCancels()
        {
            var rwl = new AsyncReaderWriterLock();
            var token = new CancellationToken(true);
            rwl.WriterLockAsync();

            var task = rwl.ReaderLockAsync(token).AsTask();

            Assert.True(task.IsCompleted);
            Assert.True(task.IsCanceled);
            Assert.False(task.IsFaulted);
        }

        [Fact]
        public async Task WriteLocked_WriterLockCancelled_DoesNotTakeLockWhenUnlocked()
        {
            var rwl = new AsyncReaderWriterLock();
            using (await rwl.WriterLockAsync())
            {
                var cts = new CancellationTokenSource();
                var task = rwl.WriterLockAsync(cts.Token).AsTask();
                cts.Cancel();
                await AsyncAssert.ThrowsAsync<OperationCanceledException>(task);
            }

            await rwl.WriterLockAsync();
        }

        [Fact]
        public async Task WriteLocked_ReaderLockCancelled_DoesNotTakeLockWhenUnlocked()
        {
            var rwl = new AsyncReaderWriterLock();
            using (await rwl.WriterLockAsync())
            {
                var cts = new CancellationTokenSource();
                var task = rwl.ReaderLockAsync(cts.Token).AsTask();
                cts.Cancel();
                await AsyncAssert.ThrowsAsync<OperationCanceledException>(task);
            }

            await rwl.ReaderLockAsync();
        }

        [Fact]
        public async Task LockReleased_WriteTakesPriorityOverRead()
        {
            var rwl = new AsyncReaderWriterLock();
            Task writeLock, readLock;
            using (await rwl.WriterLockAsync())
            {
                readLock = rwl.ReaderLockAsync().AsTask();
                writeLock = rwl.WriterLockAsync().AsTask();
            }

            await writeLock;
            await AsyncAssert.NeverCompletesAsync(readLock);
        }

        [Fact]
        public async Task ReaderLocked_ReaderReleased_ReaderAndWriterWaiting_DoesNotReleaseReaderOrWriter()
        {
            var rwl = new AsyncReaderWriterLock();
            Task readLock, writeLock;
            await rwl.ReaderLockAsync();
            using (await rwl.ReaderLockAsync())
            {
                writeLock = rwl.WriterLockAsync().AsTask();
                readLock = rwl.ReaderLockAsync().AsTask();
            }

            await Task.WhenAll(AsyncAssert.NeverCompletesAsync(writeLock),
                AsyncAssert.NeverCompletesAsync(readLock));
        }

        [Fact]
        public async Task WaitingWriter_Canceled_SignalWaitingReaders()
        {
            var rwl = new AsyncReaderWriterLock();

            // Thread A enters a read lock.
            var readLock = rwl.ReaderLock();

            // Thread B wants to enter a write lock within 600 ms. Because Thread A holds
            // a read lock, Thread B will not get the write lock.
            var taskB = Task.Run(async () =>
            {
                using (var source = new CancellationTokenSource(600))
                {
                    var writeLock = null as IDisposable;
                    try
                    {
                        writeLock = rwl.WriterLock(source.Token);
                    }
                    catch (OperationCanceledException)
                    {
                    }

                    await Task.Delay(600);

                    writeLock?.Dispose();
                }
            });

            // Thread C wants to enter a read lock. It should get the lock after
            // 600 ms because Thread B cancels its try to get the write lock after
            // that time.
            var taskC = Task.Run(async () =>
            {
                // Wait a bit before trying to enter the lock, to ensure Thread B is already
                // in the TryEnter...() call.
                await Task.Delay(300);

                // Now try to get a read lock.
                var readLock2 = rwl.ReaderLock();

                readLock2.Dispose();
            });

            // Thread D wants to enter a read lock after Thread B canceled the try to get
            // a write lock.
            var taskD = Task.Run(async () =>
            {
                // Wait until Thread B canceled its try to get a write lock.
                await Task.Delay(1000);

                // Now try to get a read lock.
                var readLock2 = rwl.ReaderLock();

                readLock2.Dispose();
            });

            await taskB;

            Assert.True(taskD.IsCompleted);
            Assert.True(taskC.IsCompleted, "cancellation of writer lock did no signal waiting reader locks");

            readLock.Dispose();
        }

        [Fact]
        public async Task LoadTest()
        {
            var rwl = new AsyncReaderWriterLock();
            var readKeys = new List<IDisposable>();
            for (int i = 0; i != 1000; ++i)
                readKeys.Add(rwl.ReaderLock());
            var writeTask = Task.Run(() => { rwl.WriterLock().Dispose(); });
            var readTasks = new List<Task>();
            for (int i = 0; i != 100; ++i)
                readTasks.Add(Task.Run(() => rwl.ReaderLock().Dispose()));
            await Task.Delay(1000);
            foreach (var readKey in readKeys)
                readKey.Dispose();
            await writeTask;
            foreach (var readTask in readTasks)
                await readTask;
        }
    }
}

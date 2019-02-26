using Nito.AsyncEx;
using Nito.AsyncEx.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [Fact]
        public async Task ReadLock_WriteLockCanceled_TakesLock()
        {
            var rwl = new AsyncReaderWriterLock();
            var readKey = rwl.ReaderLock();
            var cts = new CancellationTokenSource();

            var writerLockReady = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var writerLockTask = Task.Run(async () =>
            {
                var writeKeyTask = rwl.WriterLockAsync(cts.Token);
                writerLockReady.SetResult(null);
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => writeKeyTask);
            });
            await writerLockReady.Task;

            var readerLockReady = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var readerLockTask = Task.Run(async () =>
            {
                var readKeyTask = rwl.ReaderLockAsync();
                readerLockReady.SetResult(null);
                await readKeyTask;
            });

            await readerLockReady.Task;
            cts.Cancel();

            await readerLockTask;
        }
    }
}

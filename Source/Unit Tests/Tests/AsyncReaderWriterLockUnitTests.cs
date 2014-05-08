using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

#if NET40
#if NO_ENLIGHTENMENT
namespace Tests_NET4_NE
#else
namespace Tests_NET4
#endif
#else
#if NO_ENLIGHTENMENT
namespace Tests_NE
#else
namespace Tests
#endif
#endif
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class AsyncReaderWriterLockUnitTests
    {
        [Test]
        public void Unlocked_PermitsWriterLock()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                await rwl.WriterLockAsync();
            });
        }

        [Test]
        public void Unlocked_PermitsMultipleReaderLocks()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                await rwl.ReaderLockAsync();
                await rwl.ReaderLockAsync();
            });
        }

        [Test]
        public void WriteLocked_PreventsAnotherWriterLock()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                await rwl.WriterLockAsync();
                var task = rwl.WriterLockAsync();
                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void WriteLocked_PreventsReaderLock()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                await rwl.WriterLockAsync();
                var task = rwl.ReaderLockAsync();
                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void WriteLocked_PreventsUpgradeableReaderLock()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                await rwl.WriterLockAsync();
                var task = rwl.UpgradeableReaderLockAsync();
                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void WriteLocked_Unlocked_PermitsAnotherWriterLock()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                var firstWriteLockTaken = new TaskCompletionSource();
                var releaseFirstWriteLock = new TaskCompletionSource();
                var task = TaskShim.Run(async () =>
                {
                    using (await rwl.WriterLockAsync())
                    {
                        firstWriteLockTaken.SetResult();
                        await releaseFirstWriteLock.Task;
                    }
                });
                await firstWriteLockTaken.Task;
                var lockTask = rwl.WriterLockAsync();
                Assert.IsFalse(lockTask.IsCompleted);
                releaseFirstWriteLock.SetResult();
                await lockTask;
            });
        }

        [Test]
        public void ReadLocked_PreventsWriterLock()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                await rwl.ReaderLockAsync();
                var task = rwl.WriterLockAsync();
                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void ReadLocked_AllowsUpgradeableReaderLock()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                await rwl.ReaderLockAsync();
                await rwl.UpgradeableReaderLockAsync();
            });
        }

        [Test]
        public void UpgradeableReadLocked_PreventsWriterLock()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                var key = await rwl.UpgradeableReaderLockAsync();
                Assert.IsFalse(key.Upgraded);
                var task = rwl.WriterLockAsync();
                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void UpgradeableReadLocked_AllowsMultipleReaderLocks()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                var key = await rwl.UpgradeableReaderLockAsync();
                Assert.IsFalse(key.Upgraded);
                await rwl.ReaderLockAsync();
                await rwl.ReaderLockAsync();
            });
        }

        [Test]
        public void UpgradeableReadLocked_Unlocked_AllowsWriterLock()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                using (var key = await rwl.UpgradeableReaderLockAsync())
                {
                    Assert.IsFalse(key.Upgraded);
                }
                await rwl.WriterLockAsync();
            });
        }

        [Test]
        public void UpgradeableReadLocked_Upgraded_PreventsReaderLock()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                var key = await rwl.UpgradeableReaderLockAsync();
                await key.UpgradeAsync();
                Assert.IsTrue(key.Upgraded);
                var task = rwl.WriterLockAsync();
                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void UpgradeableReadAndReadLocked_Upgrade_WaitsForReaderLockToUnlock()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                var readLockTaken = new TaskCompletionSource();
                var releaseReadLock = new TaskCompletionSource();
                var task = TaskShim.Run(async () =>
                {
                    using (await rwl.ReaderLockAsync())
                    {
                        readLockTaken.SetResult();
                        await releaseReadLock.Task;
                    }
                });
                await readLockTaken.Task;
                using (var key = await rwl.UpgradeableReaderLockAsync())
                {
                    Assert.IsFalse(key.Upgraded);
                    var lockTask = key.UpgradeAsync();
                    Assert.IsFalse(lockTask.IsCompleted);
                    releaseReadLock.SetResult();
                    await lockTask;
                    Assert.IsTrue(key.Upgraded);
                }
            });
        }

        [Test]
        public void UpgradeableReadAndReadLocked_UpgradeAborted_CompletesAsCanceled()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                var readLockTaken = new TaskCompletionSource();
                var task = TaskShim.Run(async () =>
                {
                    using (await rwl.ReaderLockAsync())
                    {
                        readLockTaken.SetResult();
                        await TaskConstants.Never;
                    }
                });
                await readLockTaken.Task;
                Task upgradeTask;
                using (var key = await rwl.UpgradeableReaderLockAsync())
                {
                    upgradeTask = key.UpgradeAsync();
                    Assert.IsFalse(key.Upgraded);
                }

                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(upgradeTask);
            });
        }

        [Test]
        public void UpgradeableReadLocked_Downgrade_AllowsReaderLock()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                var key = await rwl.UpgradeableReaderLockAsync();
                var upgradeKey = await key.UpgradeAsync();
                upgradeKey.Dispose();
                await rwl.ReaderLockAsync();
            });
        }

        [Test]
        public void Id_IsNotZero()
        {
            var rwl = new AsyncReaderWriterLock();
            Assert.AreNotEqual(0, rwl.Id);
        }

        [Test]
        public void WriterLock_PreCancelled_LockAvailable_SynchronouslyTakesLock()
        {
            var rwl = new AsyncReaderWriterLock();
            var token = new CancellationToken(true);
            
            var task = rwl.WriterLockAsync(token);

            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(task.IsCanceled);
            Assert.IsFalse(task.IsFaulted);
        }

        [Test]
        public void WriterLock_PreCancelled_LockNotAvailable_SynchronouslyCancels()
        {
            var rwl = new AsyncReaderWriterLock();
            var token = new CancellationToken(true);
            rwl.WriterLockAsync();

            var task = rwl.WriterLockAsync(token);

            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(task.IsCanceled);
            Assert.IsFalse(task.IsFaulted);
        }

        [Test]
        public void ReaderLock_PreCancelled_LockAvailable_SynchronouslyTakesLock()
        {
            var rwl = new AsyncReaderWriterLock();
            var token = new CancellationToken(true);

            var task = rwl.ReaderLockAsync(token);

            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(task.IsCanceled);
            Assert.IsFalse(task.IsFaulted);
        }

        [Test]
        public void ReaderLock_PreCancelled_LockNotAvailable_SynchronouslyCancels()
        {
            var rwl = new AsyncReaderWriterLock();
            var token = new CancellationToken(true);
            rwl.WriterLockAsync();

            var task = rwl.ReaderLockAsync(token);

            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(task.IsCanceled);
            Assert.IsFalse(task.IsFaulted);
        }

        [Test]
        public void UpgradeableReaderLock_PreCancelled_LockAvailable_SynchronouslyTakesLock()
        {
            var rwl = new AsyncReaderWriterLock();
            var token = new CancellationToken(true);

            var task = rwl.UpgradeableReaderLockAsync(token);

            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(task.IsCanceled);
            Assert.IsFalse(task.IsFaulted);
        }

        [Test]
        public void UpgradeableReaderLock_PreCancelled_LockNotAvailable_SynchronouslyCancels()
        {
            var rwl = new AsyncReaderWriterLock();
            var token = new CancellationToken(true);
            rwl.WriterLockAsync();

            var task = rwl.UpgradeableReaderLockAsync(token);

            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(task.IsCanceled);
            Assert.IsFalse(task.IsFaulted);
        }

        [Test]
        public void Upgrade_PreCancelled_LockAvailable_SynchronouslyTakesLock()
        {
            var rwl = new AsyncReaderWriterLock();
            var token = new CancellationToken(true);
            var key = rwl.UpgradeableReaderLockAsync().Result;

            var task = key.UpgradeAsync(token);

            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(task.IsCanceled);
            Assert.IsFalse(task.IsFaulted);
        }

        [Test]
        public void Upgrade_PreCancelled_LockNotAvailable_SynchronouslyCancels()
        {
            var rwl = new AsyncReaderWriterLock();
            var token = new CancellationToken(true);
            var key = rwl.UpgradeableReaderLockAsync().Result;
            rwl.ReaderLockAsync();

            var task = key.UpgradeAsync(token);

            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(task.IsCanceled);
            Assert.IsFalse(task.IsFaulted);
        }

        [Test]
        public void WriteLocked_WriterLockCancelled_DoesNotTakeLockWhenUnlocked()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                using (await rwl.WriterLockAsync())
                {
                    var cts = new CancellationTokenSource();
                    var task = rwl.WriterLockAsync(cts.Token);
                    cts.Cancel();
                    await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => task);
                }

                await rwl.WriterLockAsync();
            });
        }

        [Test]
        public void WriteLocked_ReaderLockCancelled_DoesNotTakeLockWhenUnlocked()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                using (await rwl.WriterLockAsync())
                {
                    var cts = new CancellationTokenSource();
                    var task = rwl.ReaderLockAsync(cts.Token);
                    cts.Cancel();
                    await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => task);
                }

                await rwl.ReaderLockAsync();
            });
        }

        [Test]
        public void WriteLocked_UpgradeableReaderLockCancelled_DoesNotTakeLockWhenUnlocked()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                using (await rwl.WriterLockAsync())
                {
                    var cts = new CancellationTokenSource();
                    var task = rwl.UpgradeableReaderLockAsync(cts.Token);
                    cts.Cancel();
                    await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => task);
                }

                await rwl.UpgradeableReaderLockAsync();
            });
        }

        [Test]
        public void LockReleased_WriteTakesPriorityOverUpgradeableRead()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                Task writeLock, upgradeableReadLock;
                using (await rwl.WriterLockAsync())
                {
                    upgradeableReadLock = rwl.UpgradeableReaderLockAsync();
                    writeLock = rwl.WriterLockAsync();
                }

                await writeLock;
                await AssertEx.NeverCompletesAsync(upgradeableReadLock);
            });
        }

        [Test]
        public void LockReleased_WriteTakesPriorityOverRead()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                Task writeLock, readLock;
                using (await rwl.WriterLockAsync())
                {
                    readLock = rwl.ReaderLockAsync();
                    writeLock = rwl.WriterLockAsync();
                }

                await writeLock;
                await AssertEx.NeverCompletesAsync(readLock);
            });
        }

        [Test]
        public void LockReleased_AllowsUpgradeableReadAndMultipleReaders()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                Task upgradeableReadLock, readLock1, readLock2;
                using (await rwl.WriterLockAsync())
                {
                    upgradeableReadLock = rwl.UpgradeableReaderLockAsync();
                    readLock1 = rwl.ReaderLockAsync();
                    readLock2 = rwl.ReaderLockAsync();
                }

                await TaskShim.WhenAll(upgradeableReadLock, readLock1, readLock2);
            });
        }

        [Test]
        public void ReaderLocked_ReaderReleased_ReaderAndWriterWaiting_DoesNotReleaseReaderOrWriter()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                Task readLock, writeLock;
                await rwl.ReaderLockAsync();
                using (await rwl.ReaderLockAsync())
                {
                    writeLock = rwl.WriterLockAsync();
                    readLock = rwl.ReaderLockAsync();
                }

                await TaskShim.WhenAll(AssertEx.NeverCompletesAsync(writeLock),
                    AssertEx.NeverCompletesAsync(readLock));
            });
        }

        [Test]
        public void ReaderLocked_ReaderReleased_ReaderAndUpgradingReaderWaiting_DoesNotReleaseReaderOrUpgradingReader()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                Task readLock, upgradingReadLock;
                await rwl.ReaderLockAsync();
                using (await rwl.ReaderLockAsync())
                {
                    var upgradeableReadLock = await rwl.UpgradeableReaderLockAsync();
                    upgradingReadLock = upgradeableReadLock.UpgradeAsync();
                    readLock = rwl.ReaderLockAsync();
                }

                await TaskShim.WhenAll(AssertEx.NeverCompletesAsync(upgradingReadLock),
                    AssertEx.NeverCompletesAsync(readLock));
            });
        }

        [Test]
        public void ReaderLocked_UpgradableReaderReleased_UpgradableReaderWaiting_ReleasesUpgrableReader()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                Task upgradableReadLock;
                using (await rwl.ReaderLockAsync())
                using (await rwl.UpgradeableReaderLockAsync())
                {
                    upgradableReadLock = rwl.UpgradeableReaderLockAsync();
                    Assert.IsFalse(upgradableReadLock.IsCompleted);
                }

                await upgradableReadLock;
            });
        }

        [Test]
        public void ReaderKey_MultiDispose_DoesNothing()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                var key = await rwl.ReaderLockAsync();
                key.Dispose();
                key.Dispose();
                await rwl.ReaderLockAsync();
            });
        }

        [Test]
        public void WriterKey_MultiDispose_DoesNothing()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                var key = await rwl.WriterLockAsync();
                key.Dispose();
                key.Dispose();
                await rwl.WriterLockAsync();
            });
        }

        [Test]
        public void UpgradableKey_MultiDispose_DoesNothing()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                var key = await rwl.UpgradeableReaderLockAsync();
                key.Dispose();
                key.Dispose();
                await rwl.UpgradeableReaderLockAsync();
            });
        }

        [Test]
        public void UpgradeKey_MultiDispose_DoesNothing()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                var upgradeable = await rwl.UpgradeableReaderLockAsync();
                var key = await upgradeable.UpgradeAsync();
                key.Dispose();
                key.Dispose();
                await upgradeable.UpgradeAsync();
            });
        }

        [Test]
        public void UpgradeableKey_MultiUpgrade_ThrowsException()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                var key = await rwl.UpgradeableReaderLockAsync();
                await key.UpgradeAsync();
                await AssertEx.ThrowsExceptionAsync<InvalidOperationException>(async () => { await key.UpgradeAsync(); });
            });
        }

        [Test]
        public void UpgradeableKey_MultiUpgradeWhenFirstUpgradeIsIncomplete_ThrowsSynchronousException()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                await rwl.ReaderLockAsync();
                var key = await rwl.UpgradeableReaderLockAsync();
                var _ = key.UpgradeAsync();
                AssertEx.ThrowsException<InvalidOperationException>(() => key.UpgradeAsync());
            });
        }

        [Test]
        public void LoadTest()
        {
            Test.Async(async () =>
            {
                var rwl = new AsyncReaderWriterLock();
                var readKeys = new List<IDisposable>();
                for (int i = 0; i != 1000; ++i)
                    readKeys.Add(rwl.ReaderLock());
                var writeTask = TaskShim.Run(() => { rwl.WriterLock().Dispose(); });
                var readTasks = new List<Task>();
                for (int i = 0; i != 100; ++i)
                    readTasks.Add(TaskShim.Run(() => rwl.ReaderLock().Dispose()));
                await TaskShim.Delay(1000);
                foreach (var readKey in readKeys)
                    readKey.Dispose();
                await writeTask;
                foreach (var readTask in readTasks)
                    await readTask;
            });
        }
    }
}
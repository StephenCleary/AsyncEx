using System;
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
    public class AsyncLockUnitTests
    {
        [Test]
        public void AsyncLock_Unlocked_SynchronouslyPermitsLock()
        {
            var mutex = new AsyncLock();

            var lockTask = mutex.LockAsync().AsTask();

            Assert.IsTrue(lockTask.IsCompleted);
            Assert.IsFalse(lockTask.IsFaulted);
            Assert.IsFalse(lockTask.IsCanceled);
        }

        [Test]
        public void AsyncLock_Locked_PreventsLockUntilUnlocked()
        {
            Test.Async(async () =>
            {
                var mutex = new AsyncLock();
                var task1HasLock = new TaskCompletionSource();
                var task1Continue = new TaskCompletionSource();

                var task1 = TaskShim.Run(async () =>
                {
                    using (await mutex.LockAsync())
                    {
                        task1HasLock.SetResult();
                        await task1Continue.Task;
                    }
                });
                await task1HasLock.Task;

                var task2Start = Task.Factory.StartNew(async () =>
                {
                    await mutex.LockAsync();
                });
                var task2 = await task2Start;

                Assert.IsFalse(task2.IsCompleted);
                task1Continue.SetResult();
                await task2;
            });
        }

        [Test]
        public void AsyncLock_DoubleDispose_OnlyPermitsOneTask()
        {
            Test.Async(async () =>
            {
                var mutex = new AsyncLock();
                var task1HasLock = new TaskCompletionSource();
                var task1Continue = new TaskCompletionSource();

                await TaskShim.Run(async () =>
                {
                    var key = await mutex.LockAsync();
                    key.Dispose();
                    key.Dispose();
                });

                var task1 = TaskShim.Run(async () =>
                {
                    using (await mutex.LockAsync())
                    {
                        task1HasLock.SetResult();
                        await task1Continue.Task;
                    }
                });
                await task1HasLock.Task;

                var task2Start = Task.Factory.StartNew(async () =>
                {
                    await mutex.LockAsync();
                });
                var task2 = await task2Start;

                Assert.IsFalse(task2.IsCompleted);
                task1Continue.SetResult();
                await task2;
            });
        }

        [Test]
        public void AsyncLock_Locked_OnlyPermitsOneLockerAtATime()
        {
            Test.Async(async () =>
            {
                var mutex = new AsyncLock();
                var task1HasLock = new TaskCompletionSource();
                var task1Continue = new TaskCompletionSource();
                var task2HasLock = new TaskCompletionSource();
                var task2Continue = new TaskCompletionSource();

                var task1 = TaskShim.Run(async () =>
                {
                    using (await mutex.LockAsync())
                    {
                        task1HasLock.SetResult();
                        await task1Continue.Task;
                    }
                });
                await task1HasLock.Task;

                var task2Start = Task.Factory.StartNew(async () =>
                {
                    using (await mutex.LockAsync())
                    {
                        task2HasLock.SetResult();
                        await task2Continue.Task;
                    }
                });
                var task2 = await task2Start;

                var task3Start = Task.Factory.StartNew(async () =>
                {
                    await mutex.LockAsync();
                });
                var task3 = await task3Start;

                task1Continue.SetResult();
                await task2HasLock.Task;

                Assert.IsFalse(task3.IsCompleted);
                task2Continue.SetResult();
                await task2;
                await task3;
            });
        }

        [Test]
        public void AsyncLock_PreCancelled_Unlocked_SynchronouslyTakesLock()
        {
            var mutex = new AsyncLock();
            var token = new CancellationToken(true);

            var task = mutex.LockAsync(token).AsTask();

            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(task.IsCanceled);
            Assert.IsFalse(task.IsFaulted);
        }

        [Test]
        public void AsyncLock_PreCancelled_Locked_SynchronouslyCancels()
        {
            var mutex = new AsyncLock();
            var lockTask = mutex.LockAsync();
            var token = new CancellationToken(true);

            var task = mutex.LockAsync(token).AsTask();

            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(task.IsCanceled);
            Assert.IsFalse(task.IsFaulted);
        }

        [Test]
        public void AsyncLock_CancelledLock_LeavesLockUnlocked()
        {
            Test.Async(async () =>
            {
                var mutex = new AsyncLock();
                var cts = new CancellationTokenSource();
                var taskReady = new TaskCompletionSource();

                var unlock = await mutex.LockAsync();
                var task = TaskShim.Run(async () =>
                {
                    var lockTask = mutex.LockAsync(cts.Token);
                    taskReady.SetResult();
                    await lockTask;
                });
                await taskReady.Task;
                cts.Cancel();
                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(task);
                Assert.IsTrue(task.IsCanceled);
                unlock.Dispose();

                var finalLockTask = mutex.LockAsync();
                await finalLockTask;
            });
        }

        [Test]
        public void AsyncLock_CanceledLock_ThrowsException()
        {
            Test.Async(async () =>
            {
                var mutex = new AsyncLock();
                var cts = new CancellationTokenSource();

                await mutex.LockAsync();
                var canceledLockTask = mutex.LockAsync(cts.Token).AsTask();
                cts.Cancel();

                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(canceledLockTask);
            });
        }

        [Test]
        public void AsyncLock_CanceledTooLate_StillTakesLock()
        {
            Test.Async(async () =>
            {
                var mutex = new AsyncLock();
                var cts = new CancellationTokenSource();

                AwaitableDisposable<IDisposable> cancelableLockTask;
                using (await mutex.LockAsync())
                {
                    cancelableLockTask = mutex.LockAsync(cts.Token);
                }

                var key = await cancelableLockTask;
                cts.Cancel();

                var nextLocker = mutex.LockAsync().AsTask();
                Assert.IsFalse(nextLocker.IsCompleted);

                key.Dispose();
                await nextLocker;
            });
        }

        [Test]
        public void Id_IsNotZero()
        {
            var mutex = new AsyncLock();
            Assert.AreNotEqual(0, mutex.Id);
        }
    }
}

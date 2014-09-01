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
    public class AsyncMonitorUnitTests
    {
        [Test]
        public void Unlocked_PermitsLock()
        {
            Test.Async(async () =>
            {
                var monitor = new AsyncMonitor();
            
                var task = monitor.EnterAsync();
                await task;
            });
        }

        [Test]
        public void Locked_PreventsLockUntilUnlocked()
        {
            Test.Async(async () =>
            {
                var monitor = new AsyncMonitor();
                var task1HasLock = new TaskCompletionSource();
                var task1Continue = new TaskCompletionSource();

                var task1 = TaskShim.Run(async () =>
                {
                    using (await monitor.EnterAsync())
                    {
                        task1HasLock.SetResult();
                        await task1Continue.Task;
                    }
                });
                await task1HasLock.Task;

                var lockTask = monitor.EnterAsync().AsTask();
                Assert.IsFalse(lockTask.IsCompleted);
                task1Continue.SetResult();
                await lockTask;
            });
        }

        [Test]
        public void Pulse_ReleasesOneWaiter()
        {
            Test.Async(async () =>
            {
                var monitor = new AsyncMonitor();
                int completed = 0;
                var task1Ready = new TaskCompletionSource();
                var task2Ready = new TaskCompletionSource();
                var task1 = TaskShim.Run(async () =>
                {
                    using (await monitor.EnterAsync())
                    {
                        var waitTask1 = monitor.WaitAsync();
                        task1Ready.SetResult();
                        await waitTask1;
                        Interlocked.Increment(ref completed);
                    }
                });
                await task1Ready.Task;
                var task2 = TaskShim.Run(async () =>
                {
                    using (await monitor.EnterAsync())
                    {
                        var waitTask2 = monitor.WaitAsync();
                        task2Ready.SetResult();
                        await waitTask2;
                        Interlocked.Increment(ref completed);
                    }
                });
                await task2Ready.Task;

                using (await monitor.EnterAsync())
                {
                    monitor.Pulse();
                }
                await TaskShim.WhenAny(task1, task2);
                var result = Interlocked.CompareExchange(ref completed, 0, 0);

                Assert.AreEqual(1, result);
            });
        }

        [Test]
        public void PulseAll_ReleasesAllWaiters()
        {
            Test.Async(async () =>
            {
                var monitor = new AsyncMonitor();
                int completed = 0;
                var task1Ready = new TaskCompletionSource();
                var task2Ready = new TaskCompletionSource();
                Task waitTask1 = null;
                var task1 = TaskShim.Run(async () =>
                {
                    using (await monitor.EnterAsync())
                    {
                        waitTask1 = monitor.WaitAsync();
                        task1Ready.SetResult();
                        await waitTask1;
                        Interlocked.Increment(ref completed);
                    }
                });
                await task1Ready.Task;
                Task waitTask2 = null;
                var task2 = TaskShim.Run(async () =>
                {
                    using (await monitor.EnterAsync())
                    {
                        waitTask2 = monitor.WaitAsync();
                        task2Ready.SetResult();
                        await waitTask2;
                        Interlocked.Increment(ref completed);
                    }
                });
                await task2Ready.Task;

                var lockTask3 = monitor.EnterAsync();
                using (await lockTask3)
                {
                    monitor.PulseAll();
                }
                await TaskShim.WhenAll(task1, task2);
                var result = Interlocked.CompareExchange(ref completed, 0, 0);
            
                Assert.AreEqual(2, result);
            });
        }

        [Test]
        public void Id_IsNotZero()
        {
            var monitor = new AsyncMonitor();
            Assert.AreNotEqual(0, monitor.Id);
        }
    }
}

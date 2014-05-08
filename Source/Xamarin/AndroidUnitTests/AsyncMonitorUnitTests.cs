using System;
using NUnit.Framework;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

namespace Tests
{
    [TestFixture]
    public class AsyncMonitorUnitTests_NET40
    {
        [Test]
        public void Unlocked_PermitsLock()
        {
            AsyncContext.Run(async () =>
            {
                var monitor = new AsyncMonitor();

                var task = monitor.EnterAsync();
                await task;
            });
        }

        [Test]
        public void Locked_PreventsLockUntilUnlocked()
        {
            AsyncContext.Run(async () =>
            {
                var monitor = new AsyncMonitor();
                var task1HasLock = new TaskCompletionSource();
                var task1Continue = new TaskCompletionSource();
                Task<IDisposable> initialLockTask = null;

                var task1 = Task.Run(async () =>
                {
                    initialLockTask = monitor.EnterAsync();
                    using (await initialLockTask)
                    {
                        task1HasLock.SetResult();
                        await task1Continue.Task;
                    }
                });
                await task1HasLock.Task;

                var lockTask = monitor.EnterAsync();
                Assert.IsFalse(lockTask.IsCompleted);
                task1Continue.SetResult();
                await lockTask;
            });
        }

        [Test]
        public void Pulse_ReleasesOneWaiter()
        {
            AsyncContext.Run(async () =>
            {
                var monitor = new AsyncMonitor();
                int completed = 0;
                var task1Ready = new TaskCompletionSource();
                var task2Ready = new TaskCompletionSource();
                Task<IDisposable> lockTask1 = null;
                Task waitTask1 = null;
                var task1 = Task.Run(async () =>
                {
                    lockTask1 = monitor.EnterAsync();
                    using (await lockTask1)
                    {
                        waitTask1 = monitor.WaitAsync();
                        task1Ready.SetResult();
                        await waitTask1;
                        Interlocked.Increment(ref completed);
                    }
                });
                await task1Ready.Task;
                Task<IDisposable> lockTask2 = null;
                Task waitTask2 = null;
                var task2 = Task.Run(async () =>
                {
                    lockTask2 = monitor.EnterAsync();
                    using (await lockTask2)
                    {
                        waitTask2 = monitor.WaitAsync();
                        task2Ready.SetResult();
                        await waitTask2;
                        Interlocked.Increment(ref completed);
                    }
                });
                await task2Ready.Task;

                Task<IDisposable> lockTask3 = monitor.EnterAsync();
                using (await lockTask3)
                {
                    monitor.Pulse();
                }
                await Task.WhenAny(task1, task2);
                var result = Interlocked.CompareExchange(ref completed, 0, 0);

                Assert.AreEqual(1, result);
            });
        }

        [Test]
        public void PulseAll_ReleasesAllWaiters()
        {
            AsyncContext.Run(async () =>
            {
                var monitor = new AsyncMonitor();
                int completed = 0;
                var task1Ready = new TaskCompletionSource();
                var task2Ready = new TaskCompletionSource();
                Task<IDisposable> lockTask1 = null;
                Task waitTask1 = null;
                var task1 = Task.Run(async () =>
                {
                    lockTask1 = monitor.EnterAsync();
                    using (await lockTask1)
                    {
                        waitTask1 = monitor.WaitAsync();
                        task1Ready.SetResult();
                        await waitTask1;
                        Interlocked.Increment(ref completed);
                    }
                });
                await task1Ready.Task;
                Task<IDisposable> lockTask2 = null;
                Task waitTask2 = null;
                var task2 = Task.Run(async () =>
                {
                    lockTask2 = monitor.EnterAsync();
                    using (await lockTask2)
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
                await Task.WhenAll(task1, task2);
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

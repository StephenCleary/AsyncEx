using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace UnitTests
{
    public class AsyncMonitorUnitTests
    {
        [Fact]
        public async Task Unlocked_PermitsLock()
        {
            var monitor = new AsyncMonitor();

            var task = monitor.EnterAsync();
            await task;
        }

        [Fact]
        public async Task Locked_PreventsLockUntilUnlocked()
        {
            var monitor = new AsyncMonitor();
            var task1HasLock = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            var task1Continue = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();

            var task1 = Task.Run(async () =>
            {
                using (await monitor.EnterAsync())
                {
                    task1HasLock.SetResult(null);
                    await task1Continue.Task;
                }
            });
            await task1HasLock.Task;

            var lockTask = monitor.EnterAsync().AsTask();
            Assert.False(lockTask.IsCompleted);
            task1Continue.SetResult(null);
            await lockTask;
        }

        [Fact]
        public async Task Pulse_ReleasesOneWaiter()
        {
            var monitor = new AsyncMonitor();
            int completed = 0;
            var task1Ready = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            var task2Ready = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            var task1 = Task.Run(async () =>
            {
                using (await monitor.EnterAsync())
                {
                    var waitTask1 = monitor.WaitAsync();
                    task1Ready.SetResult(null);
                    await waitTask1;
                    Interlocked.Increment(ref completed);
                }
            });
            await task1Ready.Task;
            var task2 = Task.Run(async () =>
            {
                using (await monitor.EnterAsync())
                {
                    var waitTask2 = monitor.WaitAsync();
                    task2Ready.SetResult(null);
                    await waitTask2;
                    Interlocked.Increment(ref completed);
                }
            });
            await task2Ready.Task;

            using (await monitor.EnterAsync())
            {
                monitor.Pulse();
            }
            await Task.WhenAny(task1, task2);
            var result = Interlocked.CompareExchange(ref completed, 0, 0);

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task PulseAll_ReleasesAllWaiters()
        {
            var monitor = new AsyncMonitor();
            int completed = 0;
            var task1Ready = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            var task2Ready = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            Task waitTask1 = null;
            var task1 = Task.Run(async () =>
            {
                using (await monitor.EnterAsync())
                {
                    waitTask1 = monitor.WaitAsync();
                    task1Ready.SetResult(null);
                    await waitTask1;
                    Interlocked.Increment(ref completed);
                }
            });
            await task1Ready.Task;
            Task waitTask2 = null;
            var task2 = Task.Run(async () =>
            {
                using (await monitor.EnterAsync())
                {
                    waitTask2 = monitor.WaitAsync();
                    task2Ready.SetResult(null);
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

            Assert.Equal(2, result);
        }

        [Fact]
        public void Id_IsNotZero()
        {
            var monitor = new AsyncMonitor();
            Assert.NotEqual(0, monitor.Id);
        }
    }
}

using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Nito.AsyncEx.Testing;

namespace UnitTests
{
    public class AsyncConditionVariableUnitTests
    {
        [Fact]
        public async Task WaitAsync_WithoutNotify_IsNotCompleted()
        {
            var mutex = new AsyncLock();
            var cv = new AsyncConditionVariable(mutex);

            await mutex.LockAsync();
            var task = cv.WaitAsync();

            await AsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task WaitAsync_Notified_IsCompleted()
        {
            var mutex = new AsyncLock();
            var cv = new AsyncConditionVariable(mutex);
            await mutex.LockAsync();
            var task = cv.WaitAsync();

            await Task.Run(async () =>
            {
                using (await mutex.LockAsync())
                {
                    cv.Notify();
                }
            });
            await task;
        }

        [Fact]
        public async Task WaitAsync_AfterNotify_IsNotCompleted()
        {
            var mutex = new AsyncLock();
            var cv = new AsyncConditionVariable(mutex);
            await Task.Run(async () =>
            {
                using (await mutex.LockAsync())
                {
                    cv.Notify();
                }
            });

            await mutex.LockAsync();
            var task = cv.WaitAsync();

            await AsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task MultipleWaits_NotifyAll_AllAreCompleted()
        {
            var mutex = new AsyncLock();
            var cv = new AsyncConditionVariable(mutex);
            var key1 = await mutex.LockAsync();
            var task1 = cv.WaitAsync();
            var __ = task1.ContinueWith(_ => key1.Dispose());
            var key2 = await mutex.LockAsync();
            var task2 = cv.WaitAsync();
            var ___ = task2.ContinueWith(_ => key2.Dispose());

            await Task.Run(async () =>
            {
                using (await mutex.LockAsync())
                {
                    cv.NotifyAll();
                }
            });

            await task1;
            await task2;
        }

        [Fact]
        public async Task MultipleWaits_Notify_OneIsCompleted()
        {
            var mutex = new AsyncLock();
            var cv = new AsyncConditionVariable(mutex);
            var key = await mutex.LockAsync();
            var task1 = cv.WaitAsync();
            var __ = task1.ContinueWith(_ => key.Dispose());
            await mutex.LockAsync();
            var task2 = cv.WaitAsync();

            await Task.Run(async () =>
            {
                using (await mutex.LockAsync())
                {
                    cv.Notify();
                }
            });

            await task1;
            await AsyncAssert.NeverCompletesAsync(task2);
        }

        [Fact]
        public void Id_IsNotZero()
        {
            var mutex = new AsyncLock();
            var cv = new AsyncConditionVariable(mutex);
            Assert.NotEqual(0, cv.Id);
        }
    }
}

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
    public class AsyncConditionVariableUnitTests_NET40
    {
        [Test]
        public void WaitAsync_WithoutNotify_IsNotCompleted()
        {
            AsyncContext.Run(async () =>
            {
                var mutex = new AsyncLock();
                var cv = new AsyncConditionVariable(mutex);

                await mutex.LockAsync();
                var task = cv.WaitAsync();

                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void WaitAsync_Notified_IsCompleted()
        {
            AsyncContext.Run(async () =>
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
            });
        }

        [Test]
        public void WaitAsync_AfterNotify_IsNotCompleted()
        {
            AsyncContext.Run(async () =>
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

                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void MultipleWaits_NotifyAll_AllAreCompleted()
        {
            AsyncContext.Run(async () =>
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
            });
        }

        [Test]
        public void MultipleWaits_Notify_OneIsCompleted()
        {
            AsyncContext.Run(async () =>
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
                await AssertEx.NeverCompletesAsync(task2);
            });
        }

        [Test]
        public void Id_IsNotZero()
        {
            var mutex = new AsyncLock();
            var cv = new AsyncConditionVariable(mutex);
            Assert.AreNotEqual(0, cv.Id);
        }
    }
}

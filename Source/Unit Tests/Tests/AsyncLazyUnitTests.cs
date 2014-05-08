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
    public class AsyncLazyUnitTests
    {
        [Test]
        public void AsyncLazy_NeverAwaited_DoesNotCallFunc()
        {
            Func<int> func = () =>
            { 
                Assert.Fail();
                return 13;
            };
            
            var lazy = new AsyncLazy<int>(func);
        }

        [Test]
        public void AsyncLazy_CallsFuncOnThreadPool()
        {
            var testThread = Thread.CurrentThread.ManagedThreadId;
            var funcThread = testThread;
            Func<int> func = () =>
            {
                funcThread = Thread.CurrentThread.ManagedThreadId;
                return 13;
            };
            var lazy = new AsyncLazy<int>(func);

            TaskShim.Run(async () => await lazy).Wait();

            Assert.AreNotEqual(testThread, funcThread);
        }

        [Test]
        public void AsyncLazy_CallsAsyncFuncOnThreadPool()
        {
            var testThread = Thread.CurrentThread.ManagedThreadId;
            var funcThread = testThread;
            Func<Task<int>> func = async () =>
            {
                funcThread = Thread.CurrentThread.ManagedThreadId;
                await TaskShim.Yield();
                return 13;
            };
            var lazy = new AsyncLazy<int>(func);

            TaskShim.Run(async () => await lazy).Wait();

            Assert.AreNotEqual(testThread, funcThread);
        }

        [Test]
        public void AsyncLazy_Start_CallsFunc()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource();
                Func<int> func = () =>
                {
                    tcs.SetResult();
                    return 13;
                };
                var lazy = new AsyncLazy<int>(func);

                lazy.Start();
                await tcs.Task;
            });
        }

        [Test]
        public void AsyncLazy_Await_ReturnsFuncValue()
        {
            Test.Async(async () =>
            {
                Func<int> func = () =>
                {
                    return 13;
                };
                var lazy = new AsyncLazy<int>(func);
            
                var result = await lazy;
                Assert.AreEqual(13, result);
            });
        }

        [Test]
        public void AsyncLazy_Await_ReturnsAsyncFuncValue()
        {
            Test.Async(async () =>
            {
                Func<Task<int>> func = async () =>
                {
                    await TaskShim.Yield();
                    return 13;
                };
                var lazy = new AsyncLazy<int>(func);

                var result = await lazy;
                Assert.AreEqual(13, result);
            });
        }

        [Test]
        public void AsyncLazy_MultipleAwaiters_OnlyInvokeFuncOnce()
        {
            Test.Async(async () =>
            {
                int invokeCount = 0;
                var mre = new ManualResetEvent(false);
                Func<int> func = () =>
                {
                    Interlocked.Increment(ref invokeCount);
                    mre.WaitOne();
                    return 13;
                };
                var lazy = new AsyncLazy<int>(func);

                var task1 = Task.Factory.StartNew(async () => await lazy).Result;
                var task2 = Task.Factory.StartNew(async () => await lazy).Result;

                Assert.IsFalse(task1.IsCompleted);
                Assert.IsFalse(task2.IsCompleted);
                mre.Set();
                var results = await TaskShim.WhenAll(task1, task2);
                Assert.IsTrue(results.SequenceEqual(new[] { 13, 13 }));
                Assert.AreEqual(1, invokeCount);
            });
        }

        [Test]
        public void AsyncLazy_MultipleAwaiters_OnlyInvokeAsyncFuncOnce()
        {
            Test.Async(async () =>
            {
                int invokeCount = 0;
                var tcs = new TaskCompletionSource();
                Func<Task<int>> func = async () =>
                {
                    Interlocked.Increment(ref invokeCount);
                    await tcs.Task;
                    return 13;
                };
                var lazy = new AsyncLazy<int>(func);

                var task1 = Task.Factory.StartNew(async () => await lazy).Result;
                var task2 = Task.Factory.StartNew(async () => await lazy).Result;

                Assert.IsFalse(task1.IsCompleted);
                Assert.IsFalse(task2.IsCompleted);
                tcs.SetResult();
                var results = await TaskShim.WhenAll(task1, task2);
                Assert.IsTrue(results.SequenceEqual(new[] { 13, 13 }));
                Assert.AreEqual(1, invokeCount);
            });
        }

        [Test]
        public void Id_IsNotZero()
        {
            var lazy = new AsyncLazy<object>(() => new object());
            Assert.AreNotEqual(0, lazy.Id);
        }
    }
}

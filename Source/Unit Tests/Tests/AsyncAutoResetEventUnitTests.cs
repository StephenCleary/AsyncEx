using System;
using NUnit.Framework;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
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
    public class AsyncAutoResetEventUnitTests
    {
        [Test]
        public void WaitAsync_Unset_IsNotCompleted()
        {
            Test.Async(async () =>
            {
                var are = new AsyncAutoResetEvent();

                var task = are.WaitAsync();

                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void WaitAsync_AfterSet_CompletesSynchronously()
        {
            var are = new AsyncAutoResetEvent();
            
            are.Set();
            var task = are.WaitAsync();
            
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void WaitAsync_Set_CompletesSynchronously()
        {
            var are = new AsyncAutoResetEvent(true);

            var task = are.WaitAsync();
            
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void MultipleWaitAsync_AfterSet_OnlyOneIsCompleted()
        {
            Test.Async(async () =>
            {
                var are = new AsyncAutoResetEvent();

                are.Set();
                var task1 = are.WaitAsync();
                var task2 = are.WaitAsync();
            
                Assert.IsTrue(task1.IsCompleted);
                await AssertEx.NeverCompletesAsync(task2);
            });
        }

        [Test]
        public void MultipleWaitAsync_Set_OnlyOneIsCompleted()
        {
            Test.Async(async () =>
            {
                var are = new AsyncAutoResetEvent(true);

                var task1 = are.WaitAsync();
                var task2 = are.WaitAsync();
            
                Assert.IsTrue(task1.IsCompleted);
                await AssertEx.NeverCompletesAsync(task2);
            });
        }

        [Test]
        public void MultipleWaitAsync_AfterMultipleSet_OnlyOneIsCompleted()
        {
            Test.Async(async () =>
            {
                var are = new AsyncAutoResetEvent();

                are.Set();
                are.Set();
                var task1 = are.WaitAsync();
                var task2 = are.WaitAsync();
            
                Assert.IsTrue(task1.IsCompleted);
                await AssertEx.NeverCompletesAsync(task2);
            });
        }

        [Test]
        public void WaitAsync_PreCancelled_Set_SynchronouslyCompletesWait()
        {
            var are = new AsyncAutoResetEvent(true);
            var token = new CancellationToken(true);
            
            var task = are.WaitAsync(token);

            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(task.IsCanceled);
            Assert.IsFalse(task.IsFaulted);
        }

        [Test]
        public void WaitAsync_Cancelled_DoesNotAutoReset()
        {
            Test.Async(async () =>
            {
                var are = new AsyncAutoResetEvent();
                var cts = new CancellationTokenSource();

                cts.Cancel();
                var task1 = are.WaitAsync(cts.Token);
                task1.WaitWithoutException();
                are.Set();
                var task2 = are.WaitAsync();
            
                await task2;
            });
        }

        [Test]
        public void WaitAsync_PreCancelled_Unset_SynchronouslyCancels()
        {
            var are = new AsyncAutoResetEvent(false);
            var token = new CancellationToken(true);

            var task = are.WaitAsync(token);

            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(task.IsCanceled);
            Assert.IsFalse(task.IsFaulted);
        }

        [Test]
        public void WaitAsyncFromCustomSynchronizationContext_PreCancelled_Unset_SynchronouslyCancels()
        {
            AsyncContext.Run(() =>
            {
                var are = new AsyncAutoResetEvent(false);
                var token = new CancellationToken(true);

                var task = are.WaitAsync(token);

                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsCanceled);
                Assert.IsFalse(task.IsFaulted);
            });
        }

        [Test]
        public void WaitAsync_Cancelled_ThrowsException()
        {
            Test.Async(async () =>
            {
                var are = new AsyncAutoResetEvent();
                var cts = new CancellationTokenSource();
                cts.Cancel();
                var task = are.WaitAsync(cts.Token);
                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(task);
            });
        }

        [Test]
        public void Id_IsNotZero()
        {
            var are = new AsyncAutoResetEvent();
            Assert.AreNotEqual(0, are.Id);
        }
    }
}

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
    public class AsyncManualResetEventUnitTests
    {
        [Test]
        public void WaitAsync_Unset_IsNotCompleted()
        {
            Test.Async(async () =>
            {
                var mre = new AsyncManualResetEvent();

                var task = mre.WaitAsync();

                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void Wait_Unset_IsNotCompleted()
        {
            Test.Async(async () =>
            {
                var mre = new AsyncManualResetEvent();

                var task = TaskShim.Run(() => mre.Wait());

                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void WaitAsync_AfterSet_IsCompleted()
        {
            var mre = new AsyncManualResetEvent();

            mre.Set();
            var task = mre.WaitAsync();
            
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void Wait_AfterSet_IsCompleted()
        {
            var mre = new AsyncManualResetEvent();

            mre.Set();
            mre.Wait();
        }

        [Test]
        public void WaitAsync_Set_IsCompleted()
        {
            var mre = new AsyncManualResetEvent(true);

            var task = mre.WaitAsync();
            
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void Wait_Set_IsCompleted()
        {
            var mre = new AsyncManualResetEvent(true);

            mre.Wait();
        }

        [Test]
        public void MultipleWaitAsync_AfterSet_IsCompleted()
        {
            var mre = new AsyncManualResetEvent();

            mre.Set();
            var task1 = mre.WaitAsync();
            var task2 = mre.WaitAsync();
            
            Assert.IsTrue(task1.IsCompleted);
            Assert.IsTrue(task2.IsCompleted);
        }

        [Test]
        public void MultipleWait_AfterSet_IsCompleted()
        {
            var mre = new AsyncManualResetEvent();

            mre.Set();
            mre.Wait();
            mre.Wait();
        }

        [Test]
        public void MultipleWaitAsync_Set_IsCompleted()
        {
            var mre = new AsyncManualResetEvent(true);

            var task1 = mre.WaitAsync();
            var task2 = mre.WaitAsync();
            
            Assert.IsTrue(task1.IsCompleted);
            Assert.IsTrue(task2.IsCompleted);
        }

        [Test]
        public void MultipleWait_Set_IsCompleted()
        {
            var mre = new AsyncManualResetEvent(true);

            mre.Wait();
            mre.Wait();
        }

        [Test]
        public void WaitAsync_AfterReset_IsNotCompleted()
        {
            Test.Async(async () =>
            {
                var mre = new AsyncManualResetEvent();

                mre.Set();
                mre.Reset();
                var task = mre.WaitAsync();

                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void Wait_AfterReset_IsNotCompleted()
        {
            Test.Async(async () =>
            {
                var mre = new AsyncManualResetEvent();

                mre.Set();
                mre.Reset();
                var task = TaskShim.Run(() => mre.Wait());

                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void Id_IsNotZero()
        {
            var mre = new AsyncManualResetEvent();
            Assert.AreNotEqual(0, mre.Id);
        }
    }
}

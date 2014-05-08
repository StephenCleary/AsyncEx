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
    public class AsyncManualResetEventUnitTests_NET40
    {
        [Test]
        public void WaitAsync_Unset_IsNotCompleted()
        {
            AsyncContext.Run(async () =>
            {
                var mre = new AsyncManualResetEvent();

                var task = mre.WaitAsync();

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
        public void WaitAsync_Set_IsCompleted()
        {
            var mre = new AsyncManualResetEvent(true);

            var task = mre.WaitAsync();
            
            Assert.IsTrue(task.IsCompleted);
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
        public void MultipleWaitAsync_Set_IsCompleted()
        {
            var mre = new AsyncManualResetEvent(true);

            var task1 = mre.WaitAsync();
            var task2 = mre.WaitAsync();
            
            Assert.IsTrue(task1.IsCompleted);
            Assert.IsTrue(task2.IsCompleted);
        }

        [Test]
        public void WaitAsync_AfterReset_IsNotCompleted()
        {
            AsyncContext.Run(async () =>
            {
                var mre = new AsyncManualResetEvent();

                mre.Set();
                mre.Reset();
                var task = mre.WaitAsync();

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

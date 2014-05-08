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
    public class AsyncCountdownEventUnitTests
    {
        [Test]
        public void WaitAsync_Unset_IsNotCompleted()
        {
            Test.Async(async () =>
            {
                var ce = new AsyncCountdownEvent(1);
                var task = ce.WaitAsync();
                Assert.AreEqual(1, ce.CurrentCount);
                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void WaitAsync_AfterCountingDown_IsCompleted()
        {
            var ce = new AsyncCountdownEvent(2);
            Assert.AreEqual(2, ce.CurrentCount);
            ce.Signal();
            var task = ce.WaitAsync();
            Assert.AreEqual(1, ce.CurrentCount);
            Assert.IsFalse(task.IsCompleted);
            ce.Signal();
            Assert.AreEqual(0, ce.CurrentCount);
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void AddCount_IncrementsCount()
        {
            var ce = new AsyncCountdownEvent(1);
            var task = ce.WaitAsync();
            Assert.AreEqual(1, ce.CurrentCount);
            Assert.IsFalse(task.IsCompleted);
            ce.AddCount();
            Assert.AreEqual(2, ce.CurrentCount);
            Assert.IsFalse(task.IsCompleted);
            ce.Signal();
            Assert.AreEqual(1, ce.CurrentCount);
            Assert.IsFalse(task.IsCompleted);
            ce.Signal();
            Assert.AreEqual(0, ce.CurrentCount);
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void Signal_AfterSet_ThrowsException()
        {
            var ce = new AsyncCountdownEvent(1);
            ce.Signal();
            AssertEx.ThrowsException<InvalidOperationException>(() => ce.Signal());
        }

        [Test]
        public void TrySignal_AfterSet_ReturnsFalse()
        {
            var ce = new AsyncCountdownEvent(1);
            ce.Signal();
            var result = ce.TrySignal();
            Assert.IsFalse(result);
        }

        [Test]
        public void AddCount_AfterSet_ThrowsException()
        {
            var ce = new AsyncCountdownEvent(1);
            ce.Signal();
            AssertEx.ThrowsException<InvalidOperationException>(() => ce.AddCount());
        }

        [Test]
        public void TryAddCount_AfterSet_ReturnsFalse()
        {
            var ce = new AsyncCountdownEvent(1);
            ce.Signal();
            var result = ce.TryAddCount();
            Assert.IsFalse(result);
        }

        [Test]
        public void AddCount_Overflow_ThrowsException()
        {
            var ce = new AsyncCountdownEvent(int.MaxValue);
            AssertEx.ThrowsException<InvalidOperationException>(() => ce.AddCount());
        }

        [Test]
        public void Id_EqualsTaskId()
        {
            var ce = new AsyncCountdownEvent(1);
            var task = ce.WaitAsync();
            Assert.AreEqual(task.Id, ce.Id);
        }
    }
}

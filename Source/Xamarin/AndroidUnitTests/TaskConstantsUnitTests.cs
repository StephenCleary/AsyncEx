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
    public class TaskConstantsUnitTests_NET40
    {
        [Test]
        public void BooleanTrue_IsCompletedWithValueOfTrue()
        {
            var task = TaskConstants.BooleanTrue;
            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(task.Result);
        }

        [Test]
        public void BooleanTrue_IsCached()
        {
            var task1 = TaskConstants.BooleanTrue;
            var task2 = TaskConstants.BooleanTrue;
            Assert.AreSame(task1, task2);
        }

        [Test]
        public void BooleanFalse_IsCompletedWithValueOfFalse()
        {
            var task = TaskConstants.BooleanFalse;
            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(task.Result);
        }

        [Test]
        public void BooleanFalse_IsCached()
        {
            var task1 = TaskConstants.BooleanFalse;
            var task2 = TaskConstants.BooleanFalse;
            Assert.AreSame(task1, task2);
        }

        [Test]
        public void Int32Zero_IsCompletedWithValueOfZero()
        {
            var task = TaskConstants.Int32Zero;
            Assert.IsTrue(task.IsCompleted);
            Assert.AreEqual(0, task.Result);
        }

        [Test]
        public void Int32Zero_IsCached()
        {
            var task1 = TaskConstants.Int32Zero;
            var task2 = TaskConstants.Int32Zero;
            Assert.AreSame(task1, task2);
        }

        [Test]
        public void Int32NegativeOne_IsCompletedWithValueOfNegativeOne()
        {
            var task = TaskConstants.Int32NegativeOne;
            Assert.IsTrue(task.IsCompleted);
            Assert.AreEqual(-1, task.Result);
        }

        [Test]
        public void Int32NegativeOne_IsCached()
        {
            var task1 = TaskConstants.Int32NegativeOne;
            var task2 = TaskConstants.Int32NegativeOne;
            Assert.AreSame(task1, task2);
        }

        [Test]
        public void Completed_IsCompleted()
        {
            var task = TaskConstants.Completed;
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void Completed_IsCached()
        {
            var task1 = TaskConstants.Completed;
            var task2 = TaskConstants.Completed;
            Assert.AreSame(task1, task2);
        }

        [Test]
        public void Canceled_IsCanceled()
        {
            var task = TaskConstants.Canceled;
            Assert.IsTrue(task.IsCanceled);
        }

        [Test]
        public void Canceled_IsCached()
        {
            var task1 = TaskConstants.Canceled;
            var task2 = TaskConstants.Canceled;
            Assert.AreSame(task1, task2);
        }

        [Test]
        public void Never_NeverCompletes()
        {
            var task = TaskConstants.Never;
            task.ContinueWith(_ => { Assert.Fail("TaskConstants.Never completed."); });
        }

        [Test]
        public void Never_IsCached()
        {
            var task1 = TaskConstants.Never;
            var task2 = TaskConstants.Never;
            Assert.AreSame(task1, task2);
        }

        [Test]
        public void Default_ReferenceType_IsCompletedWithValueOfNull()
        {
            var task = TaskConstants<object>.Default;
            Assert.IsTrue(task.IsCompleted);
            Assert.IsNull(task.Result);
        }

        [Test]
        public void Default_ValueType_IsCompletedWithValueOfZero()
        {
            var task = TaskConstants<byte>.Default;
            Assert.IsTrue(task.IsCompleted);
            Assert.AreEqual(0, task.Result);
        }

        [Test]
        public void Default_IsCached()
        {
            var task1 = TaskConstants<object>.Default;
            var task2 = TaskConstants<object>.Default;
            Assert.AreSame(task1, task2);
        }

        [Test]
        public void NeverOfT_NeverCompletes()
        {
            var task = TaskConstants<object>.Never;
            task.ContinueWith(_ => { Assert.Fail("TaskConstants<object>.Never completed."); });
        }

        [Test]
        public void NeverOfT_IsCached()
        {
            var task1 = TaskConstants<object>.Never;
            var task2 = TaskConstants<object>.Never;
            Assert.AreSame(task1, task2);
        }

        [Test]
        public void CanceledOfT_IsCanceled()
        {
            var task = TaskConstants<object>.Canceled;
            Assert.IsTrue(task.IsCanceled);
        }

        [Test]
        public void CanceledOfT_IsCached()
        {
            var task1 = TaskConstants<object>.Canceled;
            var task2 = TaskConstants<object>.Canceled;
            Assert.AreSame(task1, task2);
        }
    }
}

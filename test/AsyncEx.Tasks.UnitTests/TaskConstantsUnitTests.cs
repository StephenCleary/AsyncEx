using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace UnitTests
{
    public class TaskConstantsUnitTests
    {
        [Fact]
        public void BooleanTrue_IsCompletedWithValueOfTrue()
        {
            var task = TaskConstants.BooleanTrue;
            Assert.True(task.IsCompleted);
            Assert.True(task.Result);
        }

        [Fact]
        public void BooleanTrue_IsCached()
        {
            var task1 = TaskConstants.BooleanTrue;
            var task2 = TaskConstants.BooleanTrue;
            Assert.Same(task1, task2);
        }

        [Fact]
        public void BooleanFalse_IsCompletedWithValueOfFalse()
        {
            var task = TaskConstants.BooleanFalse;
            Assert.True(task.IsCompleted);
            Assert.False(task.Result);
        }

        [Fact]
        public void BooleanFalse_IsCached()
        {
            var task1 = TaskConstants.BooleanFalse;
            var task2 = TaskConstants.BooleanFalse;
            Assert.Same(task1, task2);
        }

        [Fact]
        public void Int32Zero_IsCompletedWithValueOfZero()
        {
            var task = TaskConstants.Int32Zero;
            Assert.True(task.IsCompleted);
            Assert.Equal(0, task.Result);
        }

        [Fact]
        public void Int32Zero_IsCached()
        {
            var task1 = TaskConstants.Int32Zero;
            var task2 = TaskConstants.Int32Zero;
            Assert.Same(task1, task2);
        }

        [Fact]
        public void Int32NegativeOne_IsCompletedWithValueOfNegativeOne()
        {
            var task = TaskConstants.Int32NegativeOne;
            Assert.True(task.IsCompleted);
            Assert.Equal(-1, task.Result);
        }

        [Fact]
        public void Int32NegativeOne_IsCached()
        {
            var task1 = TaskConstants.Int32NegativeOne;
            var task2 = TaskConstants.Int32NegativeOne;
            Assert.Same(task1, task2);
        }

        [Fact]
        public void Completed_IsCompleted()
        {
            var task = TaskConstants.Completed;
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void Completed_IsCached()
        {
            var task1 = TaskConstants.Completed;
            var task2 = TaskConstants.Completed;
            Assert.Same(task1, task2);
        }

        [Fact]
        public void Canceled_IsCanceled()
        {
            var task = TaskConstants.Canceled;
            Assert.True(task.IsCanceled);
        }

        [Fact]
        public void Canceled_IsCached()
        {
            var task1 = TaskConstants.Canceled;
            var task2 = TaskConstants.Canceled;
            Assert.Same(task1, task2);
        }

        [Fact]
        public void Default_ReferenceType_IsCompletedWithValueOfNull()
        {
            var task = TaskConstants<object>.Default;
            Assert.True(task.IsCompleted);
            Assert.Null(task.Result);
        }

        [Fact]
        public void Default_ValueType_IsCompletedWithValueOfZero()
        {
            var task = TaskConstants<byte>.Default;
            Assert.True(task.IsCompleted);
            Assert.Equal(0, task.Result);
        }

        [Fact]
        public void Default_IsCached()
        {
            var task1 = TaskConstants<object>.Default;
            var task2 = TaskConstants<object>.Default;
            Assert.Same(task1, task2);
        }

        [Fact]
        public void CanceledOfT_IsCanceled()
        {
            var task = TaskConstants<object>.Canceled;
            Assert.True(task.IsCanceled);
        }

        [Fact]
        public void CanceledOfT_IsCached()
        {
            var task1 = TaskConstants<object>.Canceled;
            var task2 = TaskConstants<object>.Canceled;
            Assert.Same(task1, task2);
        }
    }
}

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
    public class AsyncManualResetEventUnitTests
    {
        [Fact]
        public async Task WaitAsync_Unset_IsNotCompleted()
        {
            var mre = new AsyncManualResetEvent();

            var task = mre.WaitAsync();

            await AsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task Wait_Unset_IsNotCompleted()
        {
            var mre = new AsyncManualResetEvent();

            var task = Task.Run(() => mre.Wait());

            await AsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public void WaitAsync_AfterSet_IsCompleted()
        {
            var mre = new AsyncManualResetEvent();

            mre.Set();
            var task = mre.WaitAsync();
            
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void Wait_AfterSet_IsCompleted()
        {
            var mre = new AsyncManualResetEvent();

            mre.Set();
            mre.Wait();
        }

        [Fact]
        public void WaitAsync_Set_IsCompleted()
        {
            var mre = new AsyncManualResetEvent(true);

            var task = mre.WaitAsync();
            
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void Wait_Set_IsCompleted()
        {
            var mre = new AsyncManualResetEvent(true);

            mre.Wait();
        }

        [Fact]
        public void MultipleWaitAsync_AfterSet_IsCompleted()
        {
            var mre = new AsyncManualResetEvent();

            mre.Set();
            var task1 = mre.WaitAsync();
            var task2 = mre.WaitAsync();
            
            Assert.True(task1.IsCompleted);
            Assert.True(task2.IsCompleted);
        }

        [Fact]
        public void MultipleWait_AfterSet_IsCompleted()
        {
            var mre = new AsyncManualResetEvent();

            mre.Set();
            mre.Wait();
            mre.Wait();
        }

        [Fact]
        public void MultipleWaitAsync_Set_IsCompleted()
        {
            var mre = new AsyncManualResetEvent(true);

            var task1 = mre.WaitAsync();
            var task2 = mre.WaitAsync();
            
            Assert.True(task1.IsCompleted);
            Assert.True(task2.IsCompleted);
        }

        [Fact]
        public void MultipleWait_Set_IsCompleted()
        {
            var mre = new AsyncManualResetEvent(true);

            mre.Wait();
            mre.Wait();
        }

        [Fact]
        public async Task WaitAsync_AfterReset_IsNotCompleted()
        {
            var mre = new AsyncManualResetEvent();

            mre.Set();
            mre.Reset();
            var task = mre.WaitAsync();

            await AsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task Wait_AfterReset_IsNotCompleted()
        {
            var mre = new AsyncManualResetEvent();

            mre.Set();
            mre.Reset();
            var task = Task.Run(() => mre.Wait());

            await AsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public void Id_IsNotZero()
        {
            var mre = new AsyncManualResetEvent();
            Assert.NotEqual(0, mre.Id);
        }
    }
}

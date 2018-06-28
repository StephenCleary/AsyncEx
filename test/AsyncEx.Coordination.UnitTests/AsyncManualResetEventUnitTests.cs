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

        [Fact]
        public void Try_Wait1() {

            /// We'll be testing the TryWaitAsync extension methods on this manual reset event.
            var mre = new AsyncManualResetEvent(false);
            
            /// Setup a task to set the event in 100ms from now
            var t1 = Task.Run(async () => {
                await Task.Delay(100);
                mre.Set();
            });
            
            using (var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200))) {
                /// result should be true because we waited up to 200ms for something that would happen in 100ms
                var result = mre.TryWaitAsync(cts.Token).Result;
                Assert.True(result);
            }
        }

        [Fact]
        public void Try_Wait2() {

            /// We'll be testing the TryWaitAsync extension methods on this manual reset event.
            var mre = new AsyncManualResetEvent(false);
            
            /// Setup a task to set the event in 100ms from now
            var t3 = Task.Run(async () => {
                await Task.Delay(200);
                mre.Set();
            });

            using (var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100))) {
                /// result should be false because we waited only 100ms for something that would happen in 200ms
                var result = mre.TryWaitAsync(cts.Token).Result;
                Assert.False(result);
            }
        }
    }
}

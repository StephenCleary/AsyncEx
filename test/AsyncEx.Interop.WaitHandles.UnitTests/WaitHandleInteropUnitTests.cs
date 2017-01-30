using System;
using System.Threading.Tasks;
using Nito.AsyncEx.Interop;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Nito.AsyncEx.Testing;

namespace UnitTests
{
    public class WaitHandleInteropUnitTests
    {
        [Fact]
        public void FromWaitHandle_SignaledHandle_SynchronouslyCompletes()
        {
            var mre = new ManualResetEvent(true);
            var task = WaitHandleAsyncFactory.FromWaitHandle(mre);
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void FromWaitHandle_SignaledHandleWithZeroTimeout_SynchronouslyCompletesWithTrueResult()
        {
            var mre = new ManualResetEvent(true);
            var task = WaitHandleAsyncFactory.FromWaitHandle(mre, TimeSpan.Zero);
            Assert.True(task.IsCompleted);
            Assert.True(task.Result);
        }

        [Fact]
        public void FromWaitHandle_UnsignaledHandleWithZeroTimeout_SynchronouslyCompletesWithFalseResult()
        {
            var mre = new ManualResetEvent(false);
            var task = WaitHandleAsyncFactory.FromWaitHandle(mre, TimeSpan.Zero);
            Assert.True(task.IsCompleted);
            Assert.False(task.Result);
        }

        [Fact]
        public void FromWaitHandle_SignaledHandleWithCanceledToken_SynchronouslyCompletes()
        {
            var mre = new ManualResetEvent(true);
            var task = WaitHandleAsyncFactory.FromWaitHandle(mre, new CancellationToken(true));
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void FromWaitHandle_UnsignaledHandleWithCanceledToken_SynchronouslyCancels()
        {
            var mre = new ManualResetEvent(false);
            var task = WaitHandleAsyncFactory.FromWaitHandle(mre, new CancellationToken(true));
            Assert.True(task.IsCompleted);
            Assert.True(task.IsCanceled);
        }

        [Fact]
        public void FromWaitHandle_SignaledHandleWithZeroTimeoutAndCanceledToken_SynchronouslyCompletesWithTrueResult()
        {
            var mre = new ManualResetEvent(true);
            var task = WaitHandleAsyncFactory.FromWaitHandle(mre, TimeSpan.Zero, new CancellationToken(true));
            Assert.True(task.IsCompleted);
            Assert.True(task.Result);
        }

        [Fact]
        public void FromWaitHandle_UnsignaledHandleWithZeroTimeoutAndCanceledToken_SynchronouslyCompletesWithFalseResult()
        {
            var mre = new ManualResetEvent(false);
            var task = WaitHandleAsyncFactory.FromWaitHandle(mre, TimeSpan.Zero, new CancellationToken(true));
            Assert.True(task.IsCompleted);
            Assert.False(task.Result);
        }

        [Fact]
        public async Task FromWaitHandle_HandleSignalled_Completes()
        {
            var mre = new ManualResetEvent(false);
            var task = WaitHandleAsyncFactory.FromWaitHandle(mre);
            Assert.False(task.IsCompleted);
            mre.Set();
            await task;
        }

        [Fact]
        public async Task FromWaitHandle_HandleSignalledBeforeTimeout_CompletesWithTrueResult()
        {
            var mre = new ManualResetEvent(false);
            var task = WaitHandleAsyncFactory.FromWaitHandle(mre, Timeout.InfiniteTimeSpan);
            Assert.False(task.IsCompleted);
            mre.Set();
            var result = await task;
            Assert.True(result);
        }

        [Fact]
        public async Task FromWaitHandle_TimeoutBeforeHandleSignalled_CompletesWithFalseResult()
        {
            var mre = new ManualResetEvent(false);
            var task = WaitHandleAsyncFactory.FromWaitHandle(mre, TimeSpan.FromMilliseconds(10));
            var result = await task;
            Assert.False(result);
        }

        [Fact]
        public async Task FromWaitHandle_HandleSignalledBeforeCanceled_CompletesSuccessfully()
        {
            var mre = new ManualResetEvent(false);
            var cts = new CancellationTokenSource();
            var task = WaitHandleAsyncFactory.FromWaitHandle(mre, cts.Token);
            Assert.False(task.IsCompleted);
            mre.Set();
            await task;
        }

        [Fact]
        public async Task FromWaitHandle_CanceledBeforeHandleSignalled_CompletesCanceled()
        {
            var mre = new ManualResetEvent(false);
            var cts = new CancellationTokenSource();
            var task = WaitHandleAsyncFactory.FromWaitHandle(mre, cts.Token);
            Assert.False(task.IsCompleted);
            cts.Cancel();
            await AsyncAssert.CancelsAsync(task);
        }

        [Fact]
        public async Task FromWaitHandle_HandleSignalledBeforeTimeoutOrCanceled_CompletesWithTrueResult()
        {
            var mre = new ManualResetEvent(false);
            var cts = new CancellationTokenSource();
            var task = WaitHandleAsyncFactory.FromWaitHandle(mre, Timeout.InfiniteTimeSpan, cts.Token);
            Assert.False(task.IsCompleted);
            mre.Set();
            var result = await task;
            Assert.True(result);
        }

        [Fact]
        public async Task FromWaitHandle_TimeoutBeforeHandleSignalledOrCanceled_CompletesWithFalseResult()
        {
            var mre = new ManualResetEvent(false);
            var cts = new CancellationTokenSource();
            var task = WaitHandleAsyncFactory.FromWaitHandle(mre, TimeSpan.FromMilliseconds(10), cts.Token);
            var result = await task;
            Assert.False(result);
        }

        [Fact]
        public async Task FromWaitHandle_CanceledBeforeTimeoutOrHandleSignalled_CompletesCanceled()
        {
            var mre = new ManualResetEvent(false);
            var cts = new CancellationTokenSource();
            var task = WaitHandleAsyncFactory.FromWaitHandle(mre, Timeout.InfiniteTimeSpan, cts.Token);
            Assert.False(task.IsCompleted);
            cts.Cancel();
            await AsyncAssert.CancelsAsync(task);
        }
    }
}

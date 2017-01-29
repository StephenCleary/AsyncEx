using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using Nito.AsyncEx.Testing;
using Xunit;

namespace UnitTests
{
    public class TaskExtensionsUnitTests
    {
        [Fact]
        public void WaitAsyncTResult_TokenThatCannotCancel_ReturnsSourceTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var task = tcs.Task.WaitAsync(CancellationToken.None);

            Assert.Same(tcs.Task, task);
        }

        [Fact]
        public void WaitAsyncTResult_AlreadyCanceledToken_ReturnsSynchronouslyCanceledTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var token = new CancellationToken(true);
            var task = tcs.Task.WaitAsync(token);

            Assert.True(task.IsCanceled);
            Assert.Equal(token, GetCancellationTokenFromTask(task));
        }

        [Fact]
        public async Task WaitAsyncTResult_TokenCanceled_CancelsTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var cts = new CancellationTokenSource();
            var task = tcs.Task.WaitAsync(cts.Token);
            Assert.False(task.IsCompleted);

            cts.Cancel();

            await AsyncAssert.ThrowsAsync<OperationCanceledException>(task);
            Assert.Equal(cts.Token, GetCancellationTokenFromTask(task));
        }

        [Fact]
        public void WaitAsync_TokenThatCannotCancel_ReturnsSourceTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var task = ((Task)tcs.Task).WaitAsync(CancellationToken.None);

            Assert.Same(tcs.Task, task);
        }

        [Fact]
        public void WaitAsync_AlreadyCanceledToken_ReturnsSynchronouslyCanceledTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var token = new CancellationToken(true);
            var task = ((Task)tcs.Task).WaitAsync(token);

            Assert.True(task.IsCanceled);
            Assert.Equal(token, GetCancellationTokenFromTask(task));
        }

        [Fact]
        public async Task WaitAsync_TokenCanceled_CancelsTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var cts = new CancellationTokenSource();
            var task = ((Task)tcs.Task).WaitAsync(cts.Token);
            Assert.False(task.IsCompleted);

            cts.Cancel();

            await AsyncAssert.ThrowsAsync<OperationCanceledException>(task);
            Assert.Equal(cts.Token, GetCancellationTokenFromTask(task));
        }

        [Fact]
        public void WhenAnyTResult_AlreadyCanceledToken_ReturnsSynchronouslyCanceledTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var token = new CancellationToken(true);
            var task = new[] { tcs.Task }.WhenAny(token);

            Assert.True(task.IsCanceled);
            Assert.Equal(token, GetCancellationTokenFromTask(task));
        }

        [Fact]
        public async Task WhenAnyTResult_TaskCompletes_CompletesTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var cts = new CancellationTokenSource();
            var task = new[] { tcs.Task }.WhenAny(cts.Token);
            Assert.False(task.IsCompleted);

            tcs.SetResult(null);

            var result = await task;
            Assert.Same(tcs.Task, result);
        }

        [Fact]
        public async Task WhenAnyTResult_TokenCanceled_CancelsTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var cts = new CancellationTokenSource();
            var task = new[] { tcs.Task }.WhenAny(cts.Token);
            Assert.False(task.IsCompleted);

            cts.Cancel();

            await AsyncAssert.ThrowsAsync<OperationCanceledException>(task);
            Assert.Equal(cts.Token, GetCancellationTokenFromTask(task));
        }

        [Fact]
        public void WhenAny_AlreadyCanceledToken_ReturnsSynchronouslyCanceledTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var token = new CancellationToken(true);
            var task = new Task[] { tcs.Task }.WhenAny(token);

            Assert.True(task.IsCanceled);
            Assert.Equal(token, GetCancellationTokenFromTask(task));
        }

        [Fact]
        public async Task WhenAny_TaskCompletes_CompletesTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var cts = new CancellationTokenSource();
            var task = new Task[] { tcs.Task }.WhenAny(cts.Token);
            Assert.False(task.IsCompleted);

            tcs.SetResult(null);

            var result = await task;
            Assert.Same(tcs.Task, result);
        }

        [Fact]
        public async Task WhenAny_TokenCanceled_CancelsTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var cts = new CancellationTokenSource();
            var task = new Task[] { tcs.Task }.WhenAny(cts.Token);
            Assert.False(task.IsCompleted);

            cts.Cancel();

            await AsyncAssert.ThrowsAsync<OperationCanceledException>(task);
            Assert.Equal(cts.Token, GetCancellationTokenFromTask(task));
        }

        [Fact]
        public async Task WhenAnyTResultWithoutToken_TaskCompletes_CompletesTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var task = new[] { tcs.Task }.WhenAny();
            Assert.False(task.IsCompleted);

            tcs.SetResult(null);

            var result = await task;
            Assert.Same(tcs.Task, result);
        }

        [Fact]
        public async Task WhenAnyWithoutToken_TaskCompletes_CompletesTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var task = new Task[] { tcs.Task }.WhenAny();
            Assert.False(task.IsCompleted);

            tcs.SetResult(null);

            var result = await task;
            Assert.Same(tcs.Task, result);
        }

        [Fact]
        public async Task WhenAllTResult_TaskCompletes_CompletesTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var task = new[] { tcs.Task }.WhenAll();
            Assert.False(task.IsCompleted);

            var expectedResult = new object();
            tcs.SetResult(expectedResult);

            var result = await task;
            Assert.Equal(new[] { expectedResult }, result);
        }

        [Fact]
        public async Task WhenAll_TaskCompletes_CompletesTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var task = new Task[] { tcs.Task }.WhenAll();
            Assert.False(task.IsCompleted);

            var expectedResult = new object();
            tcs.SetResult(expectedResult);

            await task;
        }

        private static CancellationToken GetCancellationTokenFromTask(Task task)
        {
            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                var oce = ex.InnerException as OperationCanceledException;
                if (oce != null)
                    return oce.CancellationToken;
            }
            return CancellationToken.None;
        }
    }
}

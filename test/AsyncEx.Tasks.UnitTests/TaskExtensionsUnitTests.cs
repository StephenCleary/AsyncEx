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

        [Fact]
        public async Task OrderByCompletion_OrdersByCompletion()
        {
            var tcs = new TaskCompletionSource<int>[] {new TaskCompletionSource<int>(), new TaskCompletionSource<int>()};
            var results = tcs.Select(x => x.Task).OrderByCompletion();

            Assert.False(results[0].IsCompleted);
            Assert.False(results[1].IsCompleted);

            tcs[1].SetResult(13);
            var result0 = await results[0];
            Assert.False(results[1].IsCompleted);
            Assert.Equal(13, result0);

            tcs[0].SetResult(17);
            var result1 = await results[1];
            Assert.Equal(13, result0);
            Assert.Equal(17, result1);
        }

        [Fact]
        public async Task OrderByCompletion_PropagatesFaultOnFirstCompletion()
        {
            var tcs = new TaskCompletionSource<int>[] {new TaskCompletionSource<int>(), new TaskCompletionSource<int>()};
            var results = tcs.Select(x => x.Task).OrderByCompletion();

            tcs[1].SetException(new InvalidOperationException("test message"));
            try
            {
                await results[0];
            }
            catch (InvalidOperationException ex)
            {
                Assert.Equal("test message", ex.Message);
                return;
            }

            Assert.True(false);
        }

        [Fact]
        public async Task OrderByCompletion_PropagatesFaultOnSecondCompletion()
        {
            var tcs = new TaskCompletionSource<int>[] {new TaskCompletionSource<int>(), new TaskCompletionSource<int>()};
            var results = tcs.Select(x => x.Task).OrderByCompletion();

            tcs[0].SetResult(13);
            tcs[1].SetException(new InvalidOperationException("test message"));
            await results[0];
            try
            {
                await results[1];
            }
            catch (InvalidOperationException ex)
            {
                Assert.Equal("test message", ex.Message);
                return;
            }

            Assert.True(false);
        }

        [Fact]
        public async Task OrderByCompletion_PropagatesCancelOnFirstCompletion()
        {
            var tcs = new TaskCompletionSource<int>[] {new TaskCompletionSource<int>(), new TaskCompletionSource<int>()};
            var results = tcs.Select(x => x.Task).OrderByCompletion();

            tcs[1].SetCanceled();
            try
            {
                await results[0];
            }
            catch (OperationCanceledException)
            {
                return;
            }

            Assert.True(false);
        }

        [Fact]
        public async Task OrderByCompletion_PropagatesCancelOnSecondCompletion()
        {
            var tcs = new TaskCompletionSource<int>[] {new TaskCompletionSource<int>(), new TaskCompletionSource<int>()};
            var results = tcs.Select(x => x.Task).OrderByCompletion();

            tcs[0].SetResult(13);
            tcs[1].SetCanceled();
            await results[0];
            try
            {
                await results[1];
            }
            catch (OperationCanceledException)
            {
                return;
            }

            Assert.True(false);
        }

        private static CancellationToken GetCancellationTokenFromTask(Task task)
        {
            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is OperationCanceledException oce)
                    return oce.CancellationToken;
            }
            return CancellationToken.None;
        }
    }
}

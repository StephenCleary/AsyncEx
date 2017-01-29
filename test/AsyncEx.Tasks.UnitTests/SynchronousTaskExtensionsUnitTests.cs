using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Nito.AsyncEx.Testing;

namespace UnitTests
{
    public class SynchronousTaskExtensionsUnitTests
    {
        [Fact]
        public void WaitAndUnwrapException_Completed_DoesNotBlock()
        {
            TaskConstants.Completed.WaitAndUnwrapException();
        }

        [Fact]
        public void WaitAndUnwrapException_Faulted_UnwrapsException()
        {
            var task = Task.Run(() => { throw new NotImplementedException(); });
            AsyncAssert.Throws<NotImplementedException>(() => task.WaitAndUnwrapException());
        }

        [Fact]
        public void WaitAndUnwrapExceptionWithCT_Completed_DoesNotBlock()
        {
            var cts = new CancellationTokenSource();
            TaskConstants.Completed.WaitAndUnwrapException(cts.Token);
        }

        [Fact]
        public void WaitAndUnwrapExceptionWithCT_Faulted_UnwrapsException()
        {
            var cts = new CancellationTokenSource();
            var task = Task.Run(() => { throw new NotImplementedException(); });
            AsyncAssert.Throws<NotImplementedException>(() => task.WaitAndUnwrapException(cts.Token));
        }

        [Fact]
        public void WaitAndUnwrapExceptionWithCT_CancellationTokenCancelled_Cancels()
        {
            var tcs = new TaskCompletionSource<object>();
            Task task = tcs.Task;
            var cts = new CancellationTokenSource();
            cts.Cancel();
            AsyncAssert.Throws<OperationCanceledException>(() => task.WaitAndUnwrapException(cts.Token));
        }

        [Fact]
        public void WaitAndUnwrapExceptionResult_Completed_DoesNotBlock()
        {
            TaskConstants.Int32Zero.WaitAndUnwrapException();
        }

        [Fact]
        public void WaitAndUnwrapExceptionResult_Faulted_UnwrapsException()
        {
            var task = Task.Run((Func<int>)(() => { throw new NotImplementedException(); }));
            AsyncAssert.Throws<NotImplementedException>(() => task.WaitAndUnwrapException(), allowDerivedTypes: false);
        }

        [Fact]
        public void WaitAndUnwrapExceptionResultWithCT_Completed_DoesNotBlock()
        {
            var cts = new CancellationTokenSource();
            TaskConstants.Int32Zero.WaitAndUnwrapException(cts.Token);
        }

        [Fact]
        public void WaitAndUnwrapExceptionResultWithCT_Faulted_UnwrapsException()
        {
            var cts = new CancellationTokenSource();
            var task = Task.Run((Func<int>)(() => { throw new NotImplementedException(); }));
            AsyncAssert.Throws<NotImplementedException>(() => task.WaitAndUnwrapException(cts.Token), allowDerivedTypes: false);
        }

        [Fact]
        public void WaitAndUnwrapExceptionResultWithCT_CancellationTokenCancelled_Cancels()
        {
            var tcs = new TaskCompletionSource<int>();
            var cts = new CancellationTokenSource();
            cts.Cancel();
            AsyncAssert.Throws<OperationCanceledException>(() => tcs.Task.WaitAndUnwrapException(cts.Token));
        }

        [Fact]
        public void WaitWithoutException_Completed_DoesNotBlock()
        {
            TaskConstants.Completed.WaitWithoutException();
        }

        [Fact]
        public void WaitWithoutException_Canceled_DoesNotBlockOrThrow()
        {
            TaskConstants.Canceled.WaitWithoutException();
        }

        [Fact]
        public void WaitWithoutException_Faulted_DoesNotBlockOrThrow()
        {
            var task = Task.Run(() => { throw new NotImplementedException(); });
            task.WaitWithoutException();
        }

        [Fact]
        public void WaitWithoutExceptionResult_Completed_DoesNotBlock()
        {
            TaskConstants.Int32Zero.WaitWithoutException();
        }

        [Fact]
        public void WaitWithoutExceptionResult_Canceled_DoesNotBlockOrThrow()
        {
            TaskConstants<int>.Canceled.WaitWithoutException();
        }

        [Fact]
        public void WaitWithoutExceptionResult_Faulted_DoesNotBlockOrThrow()
        {
            var task = Task.Run((Func<int>)(() => { throw new NotImplementedException(); }));
            task.WaitWithoutException();
        }

        [Fact]
        public void WaitWithoutExceptionWithCancellationToken_Completed_DoesNotBlock()
        {
            TaskConstants.Completed.WaitWithoutException(new CancellationToken());
        }

        [Fact]
        public void WaitWithoutExceptionWithCancellationToken_Canceled_DoesNotBlockOrThrow()
        {
            TaskConstants.Canceled.WaitWithoutException(new CancellationToken());
        }

        [Fact]
        public void WaitWithoutExceptionWithCancellationToken_Faulted_DoesNotBlockOrThrow()
        {
            var task = Task.Run(() => { throw new NotImplementedException(); });
            task.WaitWithoutException(new CancellationToken());
        }

        [Fact]
        public void WaitWithoutExceptionResultWithCancellationToken_Completed_DoesNotBlock()
        {
            TaskConstants.Int32Zero.WaitWithoutException(new CancellationToken());
        }

        [Fact]
        public void WaitWithoutExceptionResultWithCancellationToken_Canceled_DoesNotBlockOrThrow()
        {
            TaskConstants<int>.Canceled.WaitWithoutException(new CancellationToken());
        }

        [Fact]
        public void WaitWithoutExceptionResultWithCancellationToken_Faulted_DoesNotBlockOrThrow()
        {
            var task = Task.Run((Func<int>)(() => { throw new NotImplementedException(); }));
            task.WaitWithoutException(new CancellationToken());
        }

        [Fact]
        public void WaitWithoutExceptionWithCancellationToken_CanceledToken_DoesNotBlockButThrowsException()
        {
            Task task = new TaskCompletionSource<object>().Task;
            var cts = new CancellationTokenSource();
            cts.Cancel();
            AsyncAssert.Throws<OperationCanceledException>(() => task.WaitWithoutException(cts.Token));
        }

        [Fact]
        public async Task WaitWithoutExceptionWithCancellationToken_TokenCanceled_ThrowsException()
        {
            Task sourceTask = new TaskCompletionSource<object>().Task;
            var cts = new CancellationTokenSource();
            var task = Task.Run(() => sourceTask.WaitWithoutException(cts.Token));
            var result = task.Wait(500);
            Assert.False(result);
            cts.Cancel();
            await AsyncAssert.ThrowsAsync<OperationCanceledException>(() => task);
        }
    }
}

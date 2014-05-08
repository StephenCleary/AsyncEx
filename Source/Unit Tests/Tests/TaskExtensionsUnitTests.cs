using System;
using NUnit.Framework;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Nito.AsyncEx.Internal;
using Nito.AsyncEx.Synchronous;
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
    public class TaskExtensionsUnitTests
    {
        [Test]
        public void WaitAndUnwrapException_Completed_DoesNotBlock()
        {
            TaskConstants.Completed.WaitAndUnwrapException();
        }

        [Test]
        public void WaitAndUnwrapException_Faulted_UnwrapsException()
        {
            var task = TaskShim.Run(() => { throw new NotImplementedException(); });
            AssertEx.ThrowsException<NotImplementedException>(() => task.WaitAndUnwrapException());
        }

        [Test]
        public void WaitAndUnwrapExceptionWithCT_Completed_DoesNotBlock()
        {
            var cts = new CancellationTokenSource();
            TaskConstants.Completed.WaitAndUnwrapException(cts.Token);
        }

        [Test]
        public void WaitAndUnwrapExceptionWithCT_Faulted_UnwrapsException()
        {
            var cts = new CancellationTokenSource();
            var task = TaskShim.Run(() => { throw new NotImplementedException(); });
            AssertEx.ThrowsException<NotImplementedException>(() => task.WaitAndUnwrapException(cts.Token));
        }

        [Test]
        public void WaitAndUnwrapExceptionWithCT_CancellationTokenCancelled_Cancels()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            AssertEx.ThrowsException<OperationCanceledException>(() => TaskConstants.Never.WaitAndUnwrapException(cts.Token));
        }

        [Test]
        public void WaitAndUnwrapExceptionResult_Completed_DoesNotBlock()
        {
            TaskConstants.Int32Zero.WaitAndUnwrapException();
        }

        [Test]
        public void WaitAndUnwrapExceptionResult_Faulted_UnwrapsException()
        {
            var task = TaskShim.Run((Func<int>)(() => { throw new NotImplementedException(); }));
            AssertEx.ThrowsException<NotImplementedException>(() => task.WaitAndUnwrapException(), allowDerivedTypes: false);
        }

        [Test]
        public void WaitAndUnwrapExceptionResultWithCT_Completed_DoesNotBlock()
        {
            var cts = new CancellationTokenSource();
            TaskConstants.Int32Zero.WaitAndUnwrapException(cts.Token);
        }

        [Test]
        public void WaitAndUnwrapExceptionResultWithCT_Faulted_UnwrapsException()
        {
            var cts = new CancellationTokenSource();
            var task = TaskShim.Run((Func<int>)(() => { throw new NotImplementedException(); }));
            AssertEx.ThrowsException<NotImplementedException>(() => task.WaitAndUnwrapException(cts.Token), allowDerivedTypes: false);
        }

        [Test]
        public void WaitAndUnwrapExceptionResultWithCT_CancellationTokenCancelled_Cancels()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            AssertEx.ThrowsException<OperationCanceledException>(() => TaskConstants<int>.Never.WaitAndUnwrapException(cts.Token));
        }

        [Test]
        public void WaitWithoutException_Completed_DoesNotBlock()
        {
            TaskConstants.Completed.WaitWithoutException();
        }

        [Test]
        public void WaitWithoutException_Canceled_DoesNotBlockOrThrow()
        {
            TaskConstants.Canceled.WaitWithoutException();
        }

        [Test]
        public void WaitWithoutException_Faulted_DoesNotBlockOrThrow()
        {
            var task = TaskShim.Run(() => { throw new NotImplementedException(); });
            task.WaitWithoutException();
        }

        [Test]
        public void WaitWithoutExceptionResult_Completed_DoesNotBlock()
        {
            TaskConstants.Int32Zero.WaitWithoutException();
        }

        [Test]
        public void WaitWithoutExceptionResult_Canceled_DoesNotBlockOrThrow()
        {
            TaskConstants<int>.Canceled.WaitWithoutException();
        }

        [Test]
        public void WaitWithoutExceptionResult_Faulted_DoesNotBlockOrThrow()
        {
            var task = TaskShim.Run((Func<int>)(() => { throw new NotImplementedException(); }));
            task.WaitWithoutException();
        }

        [Test]
        public void WaitWithoutExceptionWithCancellationToken_Completed_DoesNotBlock()
        {
            TaskConstants.Completed.WaitWithoutException(new CancellationToken());
        }

        [Test]
        public void WaitWithoutExceptionWithCancellationToken_Canceled_DoesNotBlockOrThrow()
        {
            TaskConstants.Canceled.WaitWithoutException(new CancellationToken());
        }

        [Test]
        public void WaitWithoutExceptionWithCancellationToken_Faulted_DoesNotBlockOrThrow()
        {
            var task = TaskShim.Run(() => { throw new NotImplementedException(); });
            task.WaitWithoutException(new CancellationToken());
        }

        [Test]
        public void WaitWithoutExceptionResultWithCancellationToken_Completed_DoesNotBlock()
        {
            TaskConstants.Int32Zero.WaitWithoutException(new CancellationToken());
        }

        [Test]
        public void WaitWithoutExceptionResultWithCancellationToken_Canceled_DoesNotBlockOrThrow()
        {
            TaskConstants<int>.Canceled.WaitWithoutException(new CancellationToken());
        }

        [Test]
        public void WaitWithoutExceptionResultWithCancellationToken_Faulted_DoesNotBlockOrThrow()
        {
            var task = TaskShim.Run((Func<int>)(() => { throw new NotImplementedException(); }));
            task.WaitWithoutException(new CancellationToken());
        }

        [Test]
        public void WaitWithoutExceptionWithCancellationToken_CanceledToken_DoesNotBlockButThrowsException()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            AssertEx.ThrowsException<OperationCanceledException>(() => TaskConstants.Never.WaitWithoutException(cts.Token));
        }

        [Test]
        public void WaitWithoutExceptionWithCancellationToken_TokenCanceled_ThrowsException()
        {
            Test.Async(async () =>
            {
                var cts = new CancellationTokenSource();
                var task = TaskShim.Run(() => TaskConstants.Never.WaitWithoutException(cts.Token));
                var result = task.Wait(500);
                Assert.IsFalse(result);
                cts.Cancel();
                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => task);
            });
        }

        [Test]
        public void OrderByCompletion_OrdersByCompletion()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource<int>[] { new TaskCompletionSource<int>(), new TaskCompletionSource<int>() };
                var results = tcs.Select(x => x.Task).OrderByCompletion();
            
                Assert.IsFalse(results[0].IsCompleted);
                Assert.IsFalse(results[1].IsCompleted);
            
                tcs[1].SetResult(13);
                var result0 = await results[0];
                Assert.IsFalse(results[1].IsCompleted);
                Assert.AreEqual(13, result0);
            
                tcs[0].SetResult(17);
                var result1 = await results[1];
                Assert.AreEqual(13, result0);
                Assert.AreEqual(17, result1);
            });
        }

        [Test]
        public void OrderByCompletion_PropagatesFaultOnFirstCompletion()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource<int>[] { new TaskCompletionSource<int>(), new TaskCompletionSource<int>() };
                var results = tcs.Select(x => x.Task).OrderByCompletion();

                tcs[1].SetException(new InvalidOperationException("test message"));
                try
                {
                    await results[0];
                }
                catch (InvalidOperationException ex)
                {
                    Assert.AreEqual("test message", ex.Message);
                    return;
                }

                Assert.Fail();
            });
        }

        [Test]
        public void OrderByCompletion_PropagatesFaultOnSecondCompletion()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource<int>[] { new TaskCompletionSource<int>(), new TaskCompletionSource<int>() };
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
                    Assert.AreEqual("test message", ex.Message);
                    return;
                }

                Assert.Fail();
            });
        }

        [Test]
        public void OrderByCompletion_PropagatesCancelOnFirstCompletion()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource<int>[] { new TaskCompletionSource<int>(), new TaskCompletionSource<int>() };
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

                Assert.Fail();
            });
        }

        [Test]
        public void OrderByCompletion_PropagatesCancelOnSecondCompletion()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource<int>[] { new TaskCompletionSource<int>(), new TaskCompletionSource<int>() };
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

                Assert.Fail();
            });
        }
    }
}

using System;
using NUnit.Framework;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;

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
    public class TaskCompletionSourceExtensionsUnitTests
    {
        [Test]
        public void TryCompleteFromCompletedTaskTResult_PropagatesResult()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource<int>();
                tcs.TryCompleteFromCompletedTask(TaskConstants.Int32NegativeOne);
                var result = await tcs.Task;
                Assert.AreEqual(-1, result);
            });
        }

        [Test]
        public void TryCompleteFromCompletedTask_WithDifferentTResult_PropagatesResult()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource<object>();
                tcs.TryCompleteFromCompletedTask(TaskConstants.Int32NegativeOne);
                var result = await tcs.Task;
                Assert.AreEqual(-1, result);
            });
        }

        [Test]
        public void TryCompleteFromCompletedTaskTResult_PropagatesCancellation()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource<int>();
                tcs.TryCompleteFromCompletedTask(TaskConstants<int>.Canceled);
                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => tcs.Task);
            });
        }

        [Test]
        public void TryCompleteFromCompletedTaskTResult_PropagatesException()
        {
            Test.Async(async () =>
            {
                var source = new TaskCompletionSource<int>();
                source.TrySetException(new NotImplementedException());

                var tcs = new TaskCompletionSource<int>();
                tcs.TryCompleteFromCompletedTask(source.Task);
                await AssertEx.ThrowsExceptionAsync<NotImplementedException>(() => tcs.Task);
            });
        }

        [Test]
        public void TryCompleteFromCompletedTask_PropagatesResult()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource();
                tcs.TryCompleteFromCompletedTask(TaskConstants.Completed);
                await tcs.Task;
            });
        }

        [Test]
        public void TryCompleteFromCompletedTask_PropagatesCancellation()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource();
                tcs.TryCompleteFromCompletedTask(TaskConstants.Canceled);
                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => tcs.Task);
            });
        }

        [Test]
        public void TryCompleteFromCompletedTask_PropagatesException()
        {
            Test.Async(async () =>
            {
                var source = new TaskCompletionSource();
                source.TrySetException(new NotImplementedException());

                var tcs = new TaskCompletionSource();
                tcs.TryCompleteFromCompletedTask(source.Task);
                await AssertEx.ThrowsExceptionAsync<NotImplementedException>(() => tcs.Task);
            });
        }

        [Test]
        public void TryCompleteFromEventArgsTResult_PropagatesResult()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource<int>();
                tcs.TryCompleteFromEventArgs(new AsyncCompletedEventArgs(null, false, null), () => 13);
                var result = await tcs.Task;
                Assert.AreEqual(13, result);
            });
        }

        [Test]
        public void TryCompleteFromEventArgsTResult_PropagatesCancellation()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource<int>();
                tcs.TryCompleteFromEventArgs(new AsyncCompletedEventArgs(null, true, null), null);
                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => tcs.Task);
            });
        }

        [Test]
        public void TryCompleteFromEventArgsTResult_PropagatesException()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource<int>();
                tcs.TryCompleteFromEventArgs(new AsyncCompletedEventArgs(new NotImplementedException(), false, null), null);
                await AssertEx.ThrowsExceptionAsync<NotImplementedException>(() => tcs.Task);
            });
        }

        [Test]
        public void TryCompleteFromEventArgs_PropagatesResult()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource();
                tcs.TryCompleteFromEventArgs(new AsyncCompletedEventArgs(null, false, null));
                await tcs.Task;
            });
        }

        [Test]
        public void TryCompleteFromEventArgs_PropagatesCancellation()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource();
                tcs.TryCompleteFromEventArgs(new AsyncCompletedEventArgs(null, true, null));
                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => tcs.Task);
            });
        }

        [Test]
        public void TryCompleteFromEventArgs_PropagatesException()
        {
            Test.Async(async () =>
            {
                var tcs = new TaskCompletionSource();
                tcs.TryCompleteFromEventArgs(new AsyncCompletedEventArgs(new NotImplementedException(), false, null));
                await AssertEx.ThrowsExceptionAsync<NotImplementedException>(() => tcs.Task);
            });
        }

        [Test]
        public void TrySetResultWithBackgroundContinuationsTResult_RunsOnAnotherThread()
        {
            Test.Async(async () =>
            {
                var unitTestThreadId = Thread.CurrentThread.ManagedThreadId;
                int continuationThreadId = unitTestThreadId;
                var tcs = new TaskCompletionSource<int>();
                var continuation = tcs.Task.ContinueWith(_ => { continuationThreadId = Thread.CurrentThread.ManagedThreadId; },
                    TaskContinuationOptions.ExecuteSynchronously);

                tcs.TrySetResultWithBackgroundContinuations(13);
                await continuation;

                Assert.AreNotEqual(unitTestThreadId, continuationThreadId);
            });
        }

        [Test]
        public void TrySetResultWithBackgroundContinuations_RunsOnAnotherThread()
        {
            Test.Async(async () =>
            {
                var unitTestThreadId = Thread.CurrentThread.ManagedThreadId;
                int continuationThreadId = unitTestThreadId;
                var tcs = new TaskCompletionSource();
                var continuation = tcs.Task.ContinueWith(_ => { continuationThreadId = Thread.CurrentThread.ManagedThreadId; },
                    TaskContinuationOptions.ExecuteSynchronously);

                tcs.TrySetResultWithBackgroundContinuations();
                await continuation;

                Assert.AreNotEqual(unitTestThreadId, continuationThreadId);
            });
        }

        [Test]
        public void TrySetCanceledWithBackgroundContinuationsTResult_RunsOnAnotherThread()
        {
            Test.Async(async () =>
            {
                var unitTestThreadId = Thread.CurrentThread.ManagedThreadId;
                int continuationThreadId = unitTestThreadId;
                var tcs = new TaskCompletionSource<int>();
                var continuation = tcs.Task.ContinueWith(_ => { continuationThreadId = Thread.CurrentThread.ManagedThreadId; },
                    TaskContinuationOptions.ExecuteSynchronously);

                tcs.TrySetCanceledWithBackgroundContinuations();
                await continuation;

                Assert.AreNotEqual(unitTestThreadId, continuationThreadId);
            });
        }

        [Test]
        public void TrySetCanceledWithBackgroundContinuations_RunsOnAnotherThread()
        {
            Test.Async(async () =>
            {
                var unitTestThreadId = Thread.CurrentThread.ManagedThreadId;
                int continuationThreadId = unitTestThreadId;
                var tcs = new TaskCompletionSource();
                var continuation = tcs.Task.ContinueWith(_ => { continuationThreadId = Thread.CurrentThread.ManagedThreadId; },
                    TaskContinuationOptions.ExecuteSynchronously);

                tcs.TrySetCanceledWithBackgroundContinuations();
                await continuation;

                Assert.AreNotEqual(unitTestThreadId, continuationThreadId);
            });
        }

        [Test]
        public void TrySetExceptionWithBackgroundContinuationsTResult_RunsOnAnotherThread()
        {
            Test.Async(async () =>
            {
                var unitTestThreadId = Thread.CurrentThread.ManagedThreadId;
                int continuationThreadId = unitTestThreadId;
                var tcs = new TaskCompletionSource<int>();
                var continuation = tcs.Task.ContinueWith(_ => { continuationThreadId = Thread.CurrentThread.ManagedThreadId; },
                    TaskContinuationOptions.ExecuteSynchronously);

                tcs.TrySetExceptionWithBackgroundContinuations(new NotImplementedException());
                await continuation;

                Assert.AreNotEqual(unitTestThreadId, continuationThreadId);
            });
        }

        [Test]
        public void TrySetExceptionWithBackgroundContinuations_RunsOnAnotherThread()
        {
            Test.Async(async () =>
            {
                var unitTestThreadId = Thread.CurrentThread.ManagedThreadId;
                int continuationThreadId = unitTestThreadId;
                var tcs = new TaskCompletionSource();
                var continuation = tcs.Task.ContinueWith(_ => { continuationThreadId = Thread.CurrentThread.ManagedThreadId; },
                    TaskContinuationOptions.ExecuteSynchronously);

                tcs.TrySetExceptionWithBackgroundContinuations(new NotImplementedException());
                await continuation;

                Assert.AreNotEqual(unitTestThreadId, continuationThreadId);
            });
        }

        [Test]
        public void TrySetExceptionsWithBackgroundContinuationsTResult_RunsOnAnotherThread()
        {
            Test.Async(async () =>
            {
                var unitTestThreadId = Thread.CurrentThread.ManagedThreadId;
                int continuationThreadId = unitTestThreadId;
                var tcs = new TaskCompletionSource<int>();
                var continuation = tcs.Task.ContinueWith(_ => { continuationThreadId = Thread.CurrentThread.ManagedThreadId; },
                    TaskContinuationOptions.ExecuteSynchronously);

                tcs.TrySetExceptionWithBackgroundContinuations(new[] { new NotImplementedException() });
                await continuation;

                Assert.AreNotEqual(unitTestThreadId, continuationThreadId);
            });
        }

        [Test]
        public void TrySetExceptionsWithBackgroundContinuations_RunsOnAnotherThread()
        {
            Test.Async(async () =>
            {
                var unitTestThreadId = Thread.CurrentThread.ManagedThreadId;
                int continuationThreadId = unitTestThreadId;
                var tcs = new TaskCompletionSource();
                var continuation = tcs.Task.ContinueWith(_ => { continuationThreadId = Thread.CurrentThread.ManagedThreadId; },
                    TaskContinuationOptions.ExecuteSynchronously);

                tcs.TrySetExceptionWithBackgroundContinuations(new[] { new NotImplementedException() });
                await continuation;

                Assert.AreNotEqual(unitTestThreadId, continuationThreadId);
            });
        }
    }
}

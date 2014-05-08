using System;
using NUnit.Framework;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;

namespace Tests
{
    [TestFixture]
    public class TaskCompletionSourceExtensionsUnitTests_NET40
    {
        [Test]
        public void TryCompleteFromCompletedTaskTResult_PropagatesResult()
        {
            AsyncContext.Run(async () =>
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
            AsyncContext.Run(async () =>
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
            AsyncContext.Run(async () =>
            {
                var tcs = new TaskCompletionSource<int>();
                tcs.TryCompleteFromCompletedTask(TaskConstants<int>.Canceled);
                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => tcs.Task);
            });
        }

        [Test]
        public void TryCompleteFromCompletedTaskTResult_PropagatesException()
        {
            AsyncContext.Run(async () =>
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
            AsyncContext.Run(async () =>
            {
                var tcs = new TaskCompletionSource();
                tcs.TryCompleteFromCompletedTask(TaskConstants.Completed);
                await tcs.Task;
            });
        }

        [Test]
        public void TryCompleteFromCompletedTask_PropagatesCancellation()
        {
            AsyncContext.Run(async () =>
            {
                var tcs = new TaskCompletionSource();
                tcs.TryCompleteFromCompletedTask(TaskConstants.Canceled);
                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => tcs.Task);
            });
        }

        [Test]
        public void TryCompleteFromCompletedTask_PropagatesException()
        {
            AsyncContext.Run(async () =>
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
            AsyncContext.Run(async () =>
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
            AsyncContext.Run(async () =>
            {
                var tcs = new TaskCompletionSource<int>();
                tcs.TryCompleteFromEventArgs(new AsyncCompletedEventArgs(null, true, null), null);
                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => tcs.Task);
            });
        }

        [Test]
        public void TryCompleteFromEventArgsTResult_PropagatesException()
        {
            AsyncContext.Run(async () =>
            {
                var tcs = new TaskCompletionSource<int>();
                tcs.TryCompleteFromEventArgs(new AsyncCompletedEventArgs(new NotImplementedException(), false, null), null);
                await AssertEx.ThrowsExceptionAsync<NotImplementedException>(() => tcs.Task);
            });
        }

        [Test]
        public void TryCompleteFromEventArgs_PropagatesResult()
        {
            AsyncContext.Run(async () =>
            {
                var tcs = new TaskCompletionSource();
                tcs.TryCompleteFromEventArgs(new AsyncCompletedEventArgs(null, false, null));
                await tcs.Task;
            });
        }

        [Test]
        public void TryCompleteFromEventArgs_PropagatesCancellation()
        {
            AsyncContext.Run(async () =>
            {
                var tcs = new TaskCompletionSource();
                tcs.TryCompleteFromEventArgs(new AsyncCompletedEventArgs(null, true, null));
                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => tcs.Task);
            });
        }

        [Test]
        public void TryCompleteFromEventArgs_PropagatesException()
        {
            AsyncContext.Run(async () =>
            {
                var tcs = new TaskCompletionSource();
                tcs.TryCompleteFromEventArgs(new AsyncCompletedEventArgs(new NotImplementedException(), false, null));
                await AssertEx.ThrowsExceptionAsync<NotImplementedException>(() => tcs.Task);
            });
        }

        [Test]
        public void TrySetResultWithBackgroundContinuationsTResult_RunsOnAnotherThread()
        {
            AsyncContext.Run(async () =>
            {
                var unitTestThreadId = Thread.CurrentThread.ManagedThreadId;
                int continuationThreadId = unitTestThreadId;
                var tcs = new TaskCompletionSource<int>();
                var continuation = tcs.Task.ContinueWith(_ => { continuationThreadId = Thread.CurrentThread.ManagedThreadId; },
                    CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

                tcs.TrySetResultWithBackgroundContinuations(13);
                await continuation;

                Assert.AreNotEqual(unitTestThreadId, continuationThreadId);
            });
        }

        [Test]
        public void TrySetResultWithBackgroundContinuations_RunsOnAnotherThread()
        {
            AsyncContext.Run(async () =>
            {
                var unitTestThreadId = Thread.CurrentThread.ManagedThreadId;
                int continuationThreadId = unitTestThreadId;
                var tcs = new TaskCompletionSource();
                var continuation = tcs.Task.ContinueWith(_ => { continuationThreadId = Thread.CurrentThread.ManagedThreadId; },
                    CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

                tcs.TrySetResultWithBackgroundContinuations();
                await continuation;

                Assert.AreNotEqual(unitTestThreadId, continuationThreadId);
            });
        }

        [Test]
        public void TrySetCanceledWithBackgroundContinuationsTResult_RunsOnAnotherThread()
        {
            AsyncContext.Run(async () =>
            {
                var unitTestThreadId = Thread.CurrentThread.ManagedThreadId;
                int continuationThreadId = unitTestThreadId;
                var tcs = new TaskCompletionSource<int>();
                var continuation = tcs.Task.ContinueWith(_ => { continuationThreadId = Thread.CurrentThread.ManagedThreadId; },
                    CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

                tcs.TrySetCanceledWithBackgroundContinuations();
                await continuation;

                Assert.AreNotEqual(unitTestThreadId, continuationThreadId);
            });
        }

        [Test]
        public void TrySetCanceledWithBackgroundContinuations_RunsOnAnotherThread()
        {
            AsyncContext.Run(async () =>
            {
                var unitTestThreadId = Thread.CurrentThread.ManagedThreadId;
                int continuationThreadId = unitTestThreadId;
                var tcs = new TaskCompletionSource();
                var continuation = tcs.Task.ContinueWith(_ => { continuationThreadId = Thread.CurrentThread.ManagedThreadId; },
                    CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

                tcs.TrySetCanceledWithBackgroundContinuations();
                await continuation;

                Assert.AreNotEqual(unitTestThreadId, continuationThreadId);
            });
        }

        [Test]
        public void TrySetExceptionWithBackgroundContinuationsTResult_RunsOnAnotherThread()
        {
            AsyncContext.Run(async () =>
            {
                var unitTestThreadId = Thread.CurrentThread.ManagedThreadId;
                int continuationThreadId = unitTestThreadId;
                var tcs = new TaskCompletionSource<int>();
                var continuation = tcs.Task.ContinueWith(_ => { continuationThreadId = Thread.CurrentThread.ManagedThreadId; },
                    CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

                tcs.TrySetExceptionWithBackgroundContinuations(new NotImplementedException());
                await continuation;

                Assert.AreNotEqual(unitTestThreadId, continuationThreadId);
            });
        }

        [Test]
        public void TrySetExceptionWithBackgroundContinuations_RunsOnAnotherThread()
        {
            AsyncContext.Run(async () =>
            {
                var unitTestThreadId = Thread.CurrentThread.ManagedThreadId;
                int continuationThreadId = unitTestThreadId;
                var tcs = new TaskCompletionSource();
                var continuation = tcs.Task.ContinueWith(_ => { continuationThreadId = Thread.CurrentThread.ManagedThreadId; },
                    CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

                tcs.TrySetExceptionWithBackgroundContinuations(new NotImplementedException());
                await continuation;

                Assert.AreNotEqual(unitTestThreadId, continuationThreadId);
            });
        }

        [Test]
        public void TrySetExceptionsWithBackgroundContinuationsTResult_RunsOnAnotherThread()
        {
            AsyncContext.Run(async () =>
            {
                var unitTestThreadId = Thread.CurrentThread.ManagedThreadId;
                int continuationThreadId = unitTestThreadId;
                var tcs = new TaskCompletionSource<int>();
                var continuation = tcs.Task.ContinueWith(_ => { continuationThreadId = Thread.CurrentThread.ManagedThreadId; },
                    CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

                tcs.TrySetExceptionWithBackgroundContinuations(new[] { new NotImplementedException() });
                await continuation;

                Assert.AreNotEqual(unitTestThreadId, continuationThreadId);
            });
        }

        [Test]
        public void TrySetExceptionsWithBackgroundContinuations_RunsOnAnotherThread()
        {
            AsyncContext.Run(async () =>
            {
                var unitTestThreadId = Thread.CurrentThread.ManagedThreadId;
                int continuationThreadId = unitTestThreadId;
                var tcs = new TaskCompletionSource();
                var continuation = tcs.Task.ContinueWith(_ => { continuationThreadId = Thread.CurrentThread.ManagedThreadId; },
                    CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

                tcs.TrySetExceptionWithBackgroundContinuations(new[] { new NotImplementedException() });
                await continuation;

                Assert.AreNotEqual(unitTestThreadId, continuationThreadId);
            });
        }
    }
}

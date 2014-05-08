using System;
using System.Linq.Expressions;
using NUnit.Framework;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Timers;
using System.ComponentModel;
using Timer = System.Timers.Timer;

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
    public class AsyncFactoryUnitTests
    {
        [Test]
        public void FromWaitHandle_SignaledHandle_SynchronouslyCompletes()
        {
            var mre = new ManualResetEvent(true);
            var task = AsyncFactory.FromWaitHandle(mre);
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void FromWaitHandle_SignaledHandleWithZeroTimeout_SynchronouslyCompletesWithTrueResult()
        {
            var mre = new ManualResetEvent(true);
            var task = AsyncFactory.FromWaitHandle(mre, TimeSpan.Zero);
            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(task.Result);
        }

        [Test]
        public void FromWaitHandle_UnsignaledHandleWithZeroTimeout_SynchronouslyCompletesWithFalseResult()
        {
            var mre = new ManualResetEvent(false);
            var task = AsyncFactory.FromWaitHandle(mre, TimeSpan.Zero);
            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(task.Result);
        }

        [Test]
        public void FromWaitHandle_SignaledHandleWithCanceledToken_SynchronouslyCompletes()
        {
            var mre = new ManualResetEvent(true);
            var task = AsyncFactory.FromWaitHandle(mre, CancellationTokenHelpers.Canceled);
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void FromWaitHandle_UnsignaledHandleWithCanceledToken_SynchronouslyCancels()
        {
            var mre = new ManualResetEvent(false);
            var task = AsyncFactory.FromWaitHandle(mre, CancellationTokenHelpers.Canceled);
            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(task.IsCanceled);
        }

        [Test]
        public void FromWaitHandle_SignaledHandleWithZeroTimeoutAndCanceledToken_SynchronouslyCompletesWithTrueResult()
        {
            var mre = new ManualResetEvent(true);
            var task = AsyncFactory.FromWaitHandle(mre, TimeSpan.Zero, CancellationTokenHelpers.Canceled);
            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(task.Result);
        }

        [Test]
        public void FromWaitHandle_UnsignaledHandleWithZeroTimeoutAndCanceledToken_SynchronouslyCompletesWithFalseResult()
        {
            var mre = new ManualResetEvent(false);
            var task = AsyncFactory.FromWaitHandle(mre, TimeSpan.Zero, CancellationTokenHelpers.Canceled);
            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(task.Result);
        }

        [Test]
        public void FromWaitHandle_HandleSignalled_Completes()
        {
            Test.Async(async () =>
            {
                var mre = new ManualResetEvent(false);
                var task = AsyncFactory.FromWaitHandle(mre);
                Assert.IsFalse(task.IsCompleted);
                mre.Set();
                await task;
            });
        }

        [Test]
        public void FromWaitHandle_HandleSignalledBeforeTimeout_CompletesWithTrueResult()
        {
            Test.Async(async () =>
            {
                var mre = new ManualResetEvent(false);
                var task = AsyncFactory.FromWaitHandle(mre, TaskShim.InfiniteTimeSpan);
                Assert.IsFalse(task.IsCompleted);
                mre.Set();
                var result = await task;
                Assert.IsTrue(result);
            });
        }

        [Test]
        public void FromWaitHandle_TimeoutBeforeHandleSignalled_CompletesWithFalseResult()
        {
            Test.Async(async () =>
            {
                var mre = new ManualResetEvent(false);
                var task = AsyncFactory.FromWaitHandle(mre, TimeSpan.FromMilliseconds(10));
                var result = await task;
                Assert.IsFalse(result);
            });
        }

        [Test]
        public void FromWaitHandle_HandleSignalledBeforeCanceled_CompletesSuccessfully()
        {
            Test.Async(async () =>
            {
                var mre = new ManualResetEvent(false);
                var cts = new CancellationTokenSource();
                var task = AsyncFactory.FromWaitHandle(mre, cts.Token);
                Assert.IsFalse(task.IsCompleted);
                mre.Set();
                await task;
            });
        }

        [Test]
        public void FromWaitHandle_CanceledBeforeHandleSignalled_CompletesCanceled()
        {
            Test.Async(async () =>
            {
                var mre = new ManualResetEvent(false);
                var cts = new CancellationTokenSource();
                var task = AsyncFactory.FromWaitHandle(mre, cts.Token);
                Assert.IsFalse(task.IsCompleted);
                cts.Cancel();
                await AssertEx.CompletesCanceledAsync(task);
            });
        }

        [Test]
        public void FromWaitHandle_HandleSignalledBeforeTimeoutOrCanceled_CompletesWithTrueResult()
        {
            Test.Async(async () =>
            {
                var mre = new ManualResetEvent(false);
                var cts = new CancellationTokenSource();
                var task = AsyncFactory.FromWaitHandle(mre, TaskShim.InfiniteTimeSpan, cts.Token);
                Assert.IsFalse(task.IsCompleted);
                mre.Set();
                var result = await task;
                Assert.IsTrue(result);
            });
        }

        [Test]
        public void FromWaitHandle_TimeoutBeforeHandleSignalledOrCanceled_CompletesWithFalseResult()
        {
            Test.Async(async () =>
            {
                var mre = new ManualResetEvent(false);
                var cts = new CancellationTokenSource();
                var task = AsyncFactory.FromWaitHandle(mre, TimeSpan.FromMilliseconds(10), cts.Token);
                var result = await task;
                Assert.IsFalse(result);
            });
        }

        [Test]
        public void FromWaitHandle_CanceledBeforeTimeoutOrHandleSignalled_CompletesCanceled()
        {
            Test.Async(async () =>
            {
                var mre = new ManualResetEvent(false);
                var cts = new CancellationTokenSource();
                var task = AsyncFactory.FromWaitHandle(mre, TaskShim.InfiniteTimeSpan, cts.Token);
                Assert.IsFalse(task.IsCompleted);
                cts.Cancel();
                await AssertEx.CompletesCanceledAsync(task);
            });
        }

        [Test]
        public void FromEvent0_EventNotFound_RaisesException()
        {
            var bgw = new BackgroundWorker();
            AssertEx.ThrowsException<InvalidOperationException>(() => AsyncFactory<ElapsedEventArgs>.FromEvent(bgw));
        }

        [Test]
        public void FromEvent0_EventNeverRaised_NeverCompletes()
        {
            Test.Async(async () =>
            {
                var timer = new Timer();
                var task = AsyncFactory<ElapsedEventArgs>.FromEvent(timer);
                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void FromEvent0_EventRaised_Completes()
        {
            Test.Async(async () =>
            {
                var timer = new Timer();
                var task = AsyncFactory<ElapsedEventArgs>.FromEvent(timer);

                var start = DateTime.Now;
                timer.Enabled = true;
            
                var result = await task;
                var end = DateTime.Now;

                Assert.IsTrue(result.SignalTime >= start && result.SignalTime <= end);
            });
        }

        [Test]
        public void FromEvent0_AsyncEventNeverRaised_NeverCompletes()
        {
            Test.Async(async () =>
            {
                var bgw = new BackgroundWorker();
                bgw.DoWork += (_, __) => { new Semaphore(0, 1).WaitOne(); };
                var task = AsyncFactory<RunWorkerCompletedEventArgs>.FromEvent(bgw);
                bgw.RunWorkerAsync();
                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void FromEvent0_AsyncEventRaised_Completes()
        {
            Test.Async(async () =>
            {
                var bgw = new BackgroundWorker();
                var o = new object();
                bgw.DoWork += (_, args) => { args.Result = o; };
                var task = AsyncFactory<RunWorkerCompletedEventArgs>.FromEvent(bgw);
                bgw.RunWorkerAsync();

                var result = await task;

                Assert.AreSame(o, result.Result);
            });
        }

        [Test]
        public void FromEvent0_AsyncEventCanceled_Cancels()
        {
            Test.Async(async () =>
            {
                var bgw = new BackgroundWorker();
                bgw.WorkerSupportsCancellation = true;
                bgw.DoWork += (_, args) => { while (!bgw.CancellationPending) { Thread.Sleep(100); } args.Cancel = true; };
                var task = AsyncFactory<RunWorkerCompletedEventArgs>.FromEvent(bgw);
                bgw.RunWorkerAsync();
                bgw.CancelAsync();

                await AssertEx.CompletesCanceledAsync(task);
            });
        }

        [Test]
        public void FromEvent0_AsyncEventFailed_Fails()
        {
            Test.Async(async () =>
            {
                var bgw = new BackgroundWorker();
                bgw.DoWork += (_, args) => { throw new NotImplementedException(); };
                var task = AsyncFactory<RunWorkerCompletedEventArgs>.FromEvent(bgw);
                bgw.RunWorkerAsync();

                await AssertEx.ThrowsExceptionAsync<NotImplementedException>(task);
            });
        }

        [Test]
        public void FromEvent1_EventNotFound_RaisesException()
        {
            var bgw = new BackgroundWorker();
            AssertEx.ThrowsException<InvalidOperationException>(() => AsyncFactory<RunWorkerCompletedEventArgs>.FromEvent(bgw, "Elapsed"));
        }

        [Test]
        public void FromEvent1_EventNeverRaised_NeverCompletes()
        {
            Test.Async(async () =>
            {
                var timer = new Timer();
                var task = AsyncFactory<ElapsedEventArgs>.FromEvent(timer, "Elapsed");
                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void FromEvent1_EventRaised_Completes()
        {
            Test.Async(async () =>
            {
                var timer = new Timer();
                var task = AsyncFactory<ElapsedEventArgs>.FromEvent(timer, "Elapsed");

                var start = DateTime.Now;
                timer.Enabled = true;

                var result = await task;
                var end = DateTime.Now;

                Assert.IsTrue(result.SignalTime >= start && result.SignalTime <= end);
            });
        }

        [Test]
        public void FromEvent1_AsyncEventNeverRaised_NeverCompletes()
        {
            Test.Async(async () =>
            {
                var bgw = new BackgroundWorker();
                bgw.DoWork += (_, __) => { new Semaphore(0, 1).WaitOne(); };
                var task = AsyncFactory<RunWorkerCompletedEventArgs>.FromEvent(bgw, "RunWorkerCompleted");
                bgw.RunWorkerAsync();
                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void FromEvent1_AsyncEventRaised_Completes()
        {
            Test.Async(async () =>
            {
                var bgw = new BackgroundWorker();
                var o = new object();
                bgw.DoWork += (_, args) => { args.Result = o; };
                var task = AsyncFactory<RunWorkerCompletedEventArgs>.FromEvent(bgw, "RunWorkerCompleted");
                bgw.RunWorkerAsync();

                var result = await task;

                Assert.AreSame(o, result.Result);
            });
        }

        [Test]
        public void FromEvent1_AsyncEventCanceled_Cancels()
        {
            Test.Async(async () =>
            {
                var bgw = new BackgroundWorker();
                bgw.WorkerSupportsCancellation = true;
                bgw.DoWork += (_, args) => { while (!bgw.CancellationPending) { Thread.Sleep(100); } args.Cancel = true; };
                var task = AsyncFactory<RunWorkerCompletedEventArgs>.FromEvent(bgw, "RunWorkerCompleted");
                bgw.RunWorkerAsync();
                bgw.CancelAsync();

                await AssertEx.CompletesCanceledAsync(task);
            });
        }

        [Test]
        public void FromEvent1_AsyncEventFailed_Fails()
        {
            Test.Async(async () =>
            {
                var bgw = new BackgroundWorker();
                bgw.DoWork += (_, args) => { throw new NotImplementedException(); };
                var task = AsyncFactory<RunWorkerCompletedEventArgs>.FromEvent(bgw, "RunWorkerCompleted");
                bgw.RunWorkerAsync();

                await AssertEx.ThrowsExceptionAsync<NotImplementedException>(task);
            });
        }

        [Test]
        public void Args0()
        {
            Test.Async(async () =>
            {
                await AsyncFactory.FromApm(BeginSuccess0, End);
            });
        }

        [Test]
        public void Args1()
        {
            Test.Async(async () =>
            {
                IntReference counter = new IntReference();
                await AsyncFactory.FromApm(BeginSuccess1, End, counter);
                Assert.AreEqual(1, counter.Value);
            });
        }

        [Test]
        public void Args2()
        {
            Test.Async(async () =>
            {
                IntReference counter = new IntReference();
                await AsyncFactory.FromApm(BeginSuccess2, End, counter, counter);
                Assert.AreEqual(1, counter.Value);
            });
        }

        [Test]
        public void Args3()
        {
            Test.Async(async () =>
            {
                IntReference counter = new IntReference();
                await AsyncFactory.FromApm(BeginSuccess3, End, counter, counter, counter);
                Assert.AreEqual(1, counter.Value);
            });
        }

        [Test]
        public void Args4()
        {
            Test.Async(async () =>
            {
                IntReference counter = new IntReference();
                await AsyncFactory.FromApm(BeginSuccess4, End, counter, counter, counter, counter);
                Assert.AreEqual(1, counter.Value);
            });
        }

        [Test]
        public void Args5()
        {
            Test.Async(async () =>
            {
                IntReference counter = new IntReference();
                await AsyncFactory.FromApm(BeginSuccess5, End, counter, counter, counter, counter, counter);
                Assert.AreEqual(1, counter.Value);
            });
        }

        [Test]
        public void Args6()
        {
            Test.Async(async () =>
            {
                IntReference counter = new IntReference();
                await AsyncFactory.FromApm(BeginSuccess6, End, counter, counter, counter, counter, counter, counter);
                Assert.AreEqual(1, counter.Value);
            });
        }

        [Test]
        public void Args7()
        {
            Test.Async(async () =>
            {
                IntReference counter = new IntReference();
                await AsyncFactory.FromApm(BeginSuccess7, End, counter, counter, counter, counter, counter, counter, counter);
                Assert.AreEqual(1, counter.Value);
            });
        }

        [Test]
        public void Args8()
        {
            Test.Async(async () =>
            {
                IntReference counter = new IntReference();
                await AsyncFactory.FromApm(BeginSuccess8, End, counter, counter, counter, counter, counter, counter, counter, counter);
                Assert.AreEqual(1, counter.Value);
            });
        }

        [Test]
        public void Args9()
        {
            Test.Async(async () =>
            {
                IntReference counter = new IntReference();
                await AsyncFactory.FromApm(BeginSuccess9, End, counter, counter, counter, counter, counter, counter, counter, counter, counter);
                Assert.AreEqual(1, counter.Value);
            });
        }

        [Test]
        public void Args10()
        {
            Test.Async(async () =>
            {
                IntReference counter = new IntReference();
                await AsyncFactory.FromApm(BeginSuccess10, End, counter, counter, counter, counter, counter, counter, counter, counter, counter, counter);
                Assert.AreEqual(1, counter.Value);
            });
        }

        [Test]
        public void Args11()
        {
            Test.Async(async () =>
            {
                IntReference counter = new IntReference();
                await AsyncFactory.FromApm(BeginSuccess11, End, counter, counter, counter, counter, counter, counter, counter, counter, counter, counter, counter);
                Assert.AreEqual(1, counter.Value);
            });
        }

        [Test]
        public void Args12()
        {
            Test.Async(async () =>
            {
                IntReference counter = new IntReference();
                await AsyncFactory.FromApm(BeginSuccess12, End, counter, counter, counter, counter, counter, counter, counter, counter, counter, counter, counter, counter);
                Assert.AreEqual(1, counter.Value);
            });
        }

        [Test]
        public void Args13()
        {
            Test.Async(async () =>
            {
                IntReference counter = new IntReference();
                await AsyncFactory.FromApm(BeginSuccess13, End, counter, counter, counter, counter, counter, counter, counter, counter, counter, counter, counter, counter, counter);
                Assert.AreEqual(1, counter.Value);
            });
        }

        [Test]
        public void Args14()
        {
            Test.Async(async () =>
            {
                IntReference counter = new IntReference();
                await AsyncFactory.FromApm(BeginSuccess14, End, counter, counter, counter, counter, counter, counter, counter, counter, counter, counter, counter, counter, counter, counter);
                Assert.AreEqual(1, counter.Value);
            });
        }

        [Test]
        public void Failed0()
        {
            Test.Async(async () =>
            {
                await AssertEx.ThrowsExceptionAsync<InvalidOperationException>(AsyncFactory.FromApm(BeginFail0, End));
            });
        }

        [Test]
        public void Cancelled0()
        {
            Test.Async(async () =>
            {
                await AssertEx.CompletesCanceledAsync(AsyncFactory.FromApm(BeginCancel0, End));
            });
        }

        private void End(IAsyncResult result)
        {
            AsyncFactory.ToEnd(result);
        }

        private IAsyncResult BeginSuccess0(AsyncCallback callback, object state)
        {
            return AsyncFactory.ToBegin(Success0(), callback, state);
        }

        private IAsyncResult BeginSuccess1(IntReference a, AsyncCallback callback, object state)
        {
            return AsyncFactory.ToBegin(Success1(a), callback, state);
        }

        private IAsyncResult BeginSuccess2(IntReference a, IntReference a1, AsyncCallback callback, object state)
        {
            return AsyncFactory.ToBegin(Success2(a, a1), callback, state);
        }

        private IAsyncResult BeginSuccess3(IntReference a, IntReference a1, IntReference a2, AsyncCallback callback, object state)
        {
            return AsyncFactory.ToBegin(Success3(a, a1, a2), callback, state);
        }

        private IAsyncResult BeginSuccess4(IntReference a, IntReference a1, IntReference a2, IntReference a3, AsyncCallback callback, object state)
        {
            return AsyncFactory.ToBegin(Success4(a, a1, a2, a3), callback, state);
        }

        private IAsyncResult BeginSuccess5(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4, AsyncCallback callback, object state)
        {
            return AsyncFactory.ToBegin(Success5(a, a1, a2, a3, a4), callback, state);
        }

        private IAsyncResult BeginSuccess6(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4, IntReference a5, AsyncCallback callback, object state)
        {
            return AsyncFactory.ToBegin(Success6(a, a1, a2, a3, a4, a5), callback, state);
        }

        private IAsyncResult BeginSuccess7(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4, IntReference a5, IntReference a6, AsyncCallback callback, object state)
        {
            return AsyncFactory.ToBegin(Success7(a, a1, a2, a3, a4, a5, a6), callback, state);
        }

        private IAsyncResult BeginSuccess8(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4, IntReference a5, IntReference a6, IntReference a7, AsyncCallback callback, object state)
        {
            return AsyncFactory.ToBegin(Success8(a, a1, a2, a3, a4, a5, a6, a7), callback, state);
        }

        private IAsyncResult BeginSuccess9(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4, IntReference a5, IntReference a6, IntReference a7, IntReference a8, AsyncCallback callback, object state)
        {
            return AsyncFactory.ToBegin(Success9(a, a1, a2, a3, a4, a5, a6, a7, a8), callback, state);
        }

        private IAsyncResult BeginSuccess10(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4, IntReference a5, IntReference a6, IntReference a7, IntReference a8, IntReference a9, AsyncCallback callback, object state)
        {
            return AsyncFactory.ToBegin(Success10(a, a1, a2, a3, a4, a5, a6, a7, a8, a9), callback, state);
        }

        private IAsyncResult BeginSuccess11(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4, IntReference a5, IntReference a6, IntReference a7, IntReference a8, IntReference a9, IntReference a10, AsyncCallback callback, object state)
        {
            return AsyncFactory.ToBegin(Success11(a, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10), callback, state);
        }

        private IAsyncResult BeginSuccess12(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4, IntReference a5, IntReference a6, IntReference a7, IntReference a8, IntReference a9, IntReference a10, IntReference a11, AsyncCallback callback, object state)
        {
            return AsyncFactory.ToBegin(Success12(a, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11), callback, state);
        }

        private IAsyncResult BeginSuccess13(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4, IntReference a5, IntReference a6, IntReference a7, IntReference a8, IntReference a9, IntReference a10, IntReference a11, IntReference a12, AsyncCallback callback, object state)
        {
            return AsyncFactory.ToBegin(Success13(a, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12), callback, state);
        }

        private IAsyncResult BeginSuccess14(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4, IntReference a5, IntReference a6, IntReference a7, IntReference a8, IntReference a9, IntReference a10, IntReference a11, IntReference a12, IntReference a13, AsyncCallback callback, object state)
        {
            return AsyncFactory.ToBegin(Success14(a, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13), callback, state);
        }

        private IAsyncResult BeginFail0(AsyncCallback callback, object state)
        {
            return AsyncFactory.ToBegin(Fail0(), callback, state);
        }

        private IAsyncResult BeginCancel0(AsyncCallback callback, object state)
        {
            return AsyncFactory.ToBegin(Cancel0(), callback, state);
        }

        private Task Success0()
        {
            return TaskShim.Run(() => { });
        }

        private Task Success1(IntReference a)
        {
            return TaskShim.Run(() => { a.Value = 1; });
        }

        private Task Success2(IntReference a, IntReference a1)
        {
            return TaskShim.Run(() => { a.Value = 1; });
        }

        private Task Success3(IntReference a, IntReference a1, IntReference a2)
        {
            return TaskShim.Run(() => { a.Value = 1; });
        }

        private Task Success4(IntReference a, IntReference a1, IntReference a2, IntReference a3)
        {
            return TaskShim.Run(() => { a.Value = 1; });
        }

        private Task Success5(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4)
        {
            return TaskShim.Run(() => { a.Value = 1; });
        }

        private Task Success6(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4, IntReference a5)
        {
            return TaskShim.Run(() => { a.Value = 1; });
        }

        private Task Success7(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4, IntReference a5, IntReference a6)
        {
            return TaskShim.Run(() => { a.Value = 1; });
        }

        private Task Success8(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4, IntReference a5, IntReference a6, IntReference a7)
        {
            return TaskShim.Run(() => { a.Value = 1; });
        }

        private Task Success9(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4, IntReference a5, IntReference a6, IntReference a7, IntReference a8)
        {
            return TaskShim.Run(() => { a.Value = 1; });
        }

        private Task Success10(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4, IntReference a5, IntReference a6, IntReference a7, IntReference a8, IntReference a9)
        {
            return TaskShim.Run(() => { a.Value = 1; });
        }

        private Task Success11(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4, IntReference a5, IntReference a6, IntReference a7, IntReference a8, IntReference a9, IntReference a10)
        {
            return TaskShim.Run(() => { a.Value = 1; });
        }

        private Task Success12(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4, IntReference a5, IntReference a6, IntReference a7, IntReference a8, IntReference a9, IntReference a10, IntReference a11)
        {
            return TaskShim.Run(() => { a.Value = 1; });
        }

        private Task Success13(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4, IntReference a5, IntReference a6, IntReference a7, IntReference a8, IntReference a9, IntReference a10, IntReference a11, IntReference a12)
        {
            return TaskShim.Run(() => { a.Value = 1; });
        }

        private Task Success14(IntReference a, IntReference a1, IntReference a2, IntReference a3, IntReference a4, IntReference a5, IntReference a6, IntReference a7, IntReference a8, IntReference a9, IntReference a10, IntReference a11, IntReference a12, IntReference a13)
        {
            return TaskShim.Run(() => { a.Value = 1; });
        }

        private Task Fail0()
        {
            return TaskShim.Run(() => { throw new InvalidOperationException(); });
        }

        private Task Cancel0()
        {
            return TaskShim.Run(() => { new CancellationToken(true).ThrowIfCancellationRequested(); });
        }

        private sealed class IntReference
        {
            private readonly object sync = new object();
            private int value;

            public int Value
            {
                get { lock (sync) { return value; } }
                set { lock (sync) { this.value = value; } }
            }
        }
    }
}

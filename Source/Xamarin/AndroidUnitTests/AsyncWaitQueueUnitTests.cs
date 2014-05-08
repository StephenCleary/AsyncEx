using System;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using Nito.AsyncEx;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public class AsyncWaitQueueUnitTests_NET40
    {
        [Test]
        public void IsEmpty_WhenEmpty_IsTrue()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            Assert.IsTrue(queue.IsEmpty);
        }

        [Test]
        public void IsEmpty_WithOneItem_IsFalse()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            queue.Enqueue();
            Assert.IsFalse(queue.IsEmpty);
        }

        [Test]
        public void IsEmpty_WithTwoItems_IsFalse()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            queue.Enqueue();
            queue.Enqueue();
            Assert.IsFalse(queue.IsEmpty);
        }

        [Test]
        public void Dequeue_Disposed_CompletesTask()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var task = queue.Enqueue();
            var finish = queue.Dequeue();
            Assert.IsFalse(task.IsCompleted);
            finish.Dispose();
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void Dequeue_WithTwoItems_OnlyCompletesFirstItem()
        {
            AsyncContext.Run(async () =>
            {
                var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
                var task1 = queue.Enqueue();
                var task2 = queue.Enqueue();
                queue.Dequeue().Dispose();
                Assert.IsTrue(task1.IsCompleted);
                await AssertEx.NeverCompletesAsync(task2);
            });
        }

        [Test]
        public void Dequeue_WithResult_CompletesWithResult()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var result = new object();
            var task = queue.Enqueue();
            queue.Dequeue(result).Dispose();
            Assert.AreSame(result, task.Result);
        }

        [Test]
        public void Dequeue_WithoutResult_CompletesWithDefaultResult()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var task = queue.Enqueue();
            queue.Dequeue().Dispose();
            Assert.AreEqual(default(object), task.Result);
        }

        [Test]
        public void DequeueAll_Disposed_CompletesAllTasks()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var task1 = queue.Enqueue();
            var task2 = queue.Enqueue();
            var finish = queue.DequeueAll();
            Assert.IsFalse(task1.IsCompleted);
            Assert.IsFalse(task2.IsCompleted);
            finish.Dispose();
            Assert.IsTrue(task1.IsCompleted);
            Assert.IsTrue(task2.IsCompleted);
        }

        [Test]
        public void DequeueAll_WithoutResult_CompletesAllTasksWithDefaultResult()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var task1 = queue.Enqueue();
            var task2 = queue.Enqueue();
            queue.DequeueAll().Dispose();
            Assert.AreEqual(default(object), task1.Result);
            Assert.AreEqual(default(object), task2.Result);
        }

        [Test]
        public void DequeueAll_WithResult_CompletesAllTasksWithResult()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var result = new object();
            var task1 = queue.Enqueue();
            var task2 = queue.Enqueue();
            queue.DequeueAll(result).Dispose();
            Assert.AreSame(result, task1.Result);
            Assert.AreSame(result, task2.Result);
        }

        [Test]
        public void TryCancel_EntryFound_CancelsTask()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var task = queue.Enqueue();
            queue.TryCancel(task).Dispose();
            Assert.IsTrue(task.IsCanceled);
        }

        [Test]
        public void TryCancel_EntryFound_RemovesTaskFromQueue()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var task = queue.Enqueue();
            queue.TryCancel(task).Dispose();
            Assert.IsTrue(queue.IsEmpty);
        }

        [Test]
        public void TryCancel_EntryNotFound_DoesNotRemoveTaskFromQueue()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var task = queue.Enqueue();
            queue.Enqueue();
            queue.Dequeue().Dispose();
            queue.TryCancel(task).Dispose();
            Assert.IsFalse(queue.IsEmpty);
        }

        [Test]
        public void Cancelled_WhenInQueue_CancelsTask()
        {
            AsyncContext.Run(async () =>
            {
                var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
                var cts = new CancellationTokenSource();
                var task = queue.Enqueue(cts.Token);
                cts.Cancel();
                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(task);
            });
        }

        [Test]
        public void Cancelled_WhenInQueue_RemovesTaskFromQueue()
        {
            AsyncContext.Run(async () =>
            {
                var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
                var cts = new CancellationTokenSource();
                var task = queue.Enqueue(cts.Token);
                cts.Cancel();
                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(task);
                Assert.IsTrue(queue.IsEmpty);
            });
        }

        [Test]
        public void Cancelled_WhenNotInQueue_DoesNotRemoveTaskFromQueue()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var cts = new CancellationTokenSource();
            var task = queue.Enqueue(cts.Token);
            var _ = queue.Enqueue();
            queue.Dequeue().Dispose();
            cts.Cancel();
            Assert.IsFalse(queue.IsEmpty);
        }

        [Test]
        public void Cancelled_BeforeEnqueue_SynchronouslyCancelsTask()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var task = queue.Enqueue(cts.Token);
            Assert.IsTrue(task.IsCanceled);
        }

        [Test]
        public void Cancelled_BeforeEnqueue_RemovesTaskFromQueue()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var task = queue.Enqueue(cts.Token);
            Assert.IsTrue(queue.IsEmpty);
        }
    }
}

using System;
using System.Diagnostics.CodeAnalysis;
using Nito.AsyncEx;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Nito.AsyncEx.Testing;

namespace UnitTests
{
    public class AsyncWaitQueueUnitTests
    {
        [Fact]
        public void IsEmpty_WhenEmpty_IsTrue()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            Assert.True(queue.IsEmpty);
        }

        [Fact]
        public void IsEmpty_WithOneItem_IsFalse()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            queue.Enqueue();
            Assert.False(queue.IsEmpty);
        }

        [Fact]
        public void IsEmpty_WithTwoItems_IsFalse()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            queue.Enqueue();
            queue.Enqueue();
            Assert.False(queue.IsEmpty);
        }

        [Fact]
        public void Dequeue_SynchronouslyCompletesTask()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var task = queue.Enqueue();
            queue.Dequeue();
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task Dequeue_WithTwoItems_OnlyCompletesFirstItem()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var task1 = queue.Enqueue();
            var task2 = queue.Enqueue();
            queue.Dequeue();
            Assert.True(task1.IsCompleted);
            await AsyncAssert.NeverCompletesAsync(task2);
        }

        [Fact]
        public void Dequeue_WithResult_SynchronouslyCompletesWithResult()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var result = new object();
            var task = queue.Enqueue();
            queue.Dequeue(result);
            Assert.Same(result, task.Result);
        }

        [Fact]
        public void Dequeue_WithoutResult_SynchronouslyCompletesWithDefaultResult()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var task = queue.Enqueue();
            queue.Dequeue();
            Assert.Equal(default(object), task.Result);
        }

        [Fact]
        public void DequeueAll_SynchronouslyCompletesAllTasks()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var task1 = queue.Enqueue();
            var task2 = queue.Enqueue();
            queue.DequeueAll();
            Assert.True(task1.IsCompleted);
            Assert.True(task2.IsCompleted);
        }

        [Fact]
        public void DequeueAll_WithoutResult_SynchronouslyCompletesAllTasksWithDefaultResult()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var task1 = queue.Enqueue();
            var task2 = queue.Enqueue();
            queue.DequeueAll();
            Assert.Equal(default(object), task1.Result);
            Assert.Equal(default(object), task2.Result);
        }

        [Fact]
        public void DequeueAll_WithResult_CompletesAllTasksWithResult()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var result = new object();
            var task1 = queue.Enqueue();
            var task2 = queue.Enqueue();
            queue.DequeueAll(result);
            Assert.Same(result, task1.Result);
            Assert.Same(result, task2.Result);
        }

        [Fact]
        public void TryCancel_EntryFound_SynchronouslyCancelsTask()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var task = queue.Enqueue();
            queue.TryCancel(task, new CancellationToken(true));
            Assert.True(task.IsCanceled);
        }

        [Fact]
        public void TryCancel_EntryFound_RemovesTaskFromQueue()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var task = queue.Enqueue();
            queue.TryCancel(task, new CancellationToken(true));
            Assert.True(queue.IsEmpty);
        }

        [Fact]
        public void TryCancel_EntryNotFound_DoesNotRemoveTaskFromQueue()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var task = queue.Enqueue();
            queue.Enqueue();
            queue.Dequeue();
            queue.TryCancel(task, new CancellationToken(true));
            Assert.False(queue.IsEmpty);
        }

        [Fact]
        public async Task Cancelled_WhenInQueue_CancelsTask()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var cts = new CancellationTokenSource();
            var task = queue.Enqueue(new object(), cts.Token);
            cts.Cancel();
            await AsyncAssert.ThrowsAsync<OperationCanceledException>(task);
        }

        [Fact]
        public async Task Cancelled_WhenInQueue_RemovesTaskFromQueue()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var cts = new CancellationTokenSource();
            var task = queue.Enqueue(new object(), cts.Token);
            cts.Cancel();
            await AsyncAssert.ThrowsAsync<OperationCanceledException>(task);
            Assert.True(queue.IsEmpty);
        }

        [Fact]
        public void Cancelled_WhenNotInQueue_DoesNotRemoveTaskFromQueue()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var cts = new CancellationTokenSource();
            var task = queue.Enqueue(new object(), cts.Token);
            var _ = queue.Enqueue();
            queue.Dequeue();
            cts.Cancel();
            Assert.False(queue.IsEmpty);
        }

        [Fact]
        public void Cancelled_BeforeEnqueue_SynchronouslyCancelsTask()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var task = queue.Enqueue(new object(), cts.Token);
            Assert.True(task.IsCanceled);
        }

        [Fact]
        public void Cancelled_BeforeEnqueue_RemovesTaskFromQueue()
        {
            var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var task = queue.Enqueue(new object(), cts.Token);
            Assert.True(queue.IsEmpty);
        }
    }
}

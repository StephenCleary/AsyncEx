using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Nito.AsyncEx.Testing;
using Nito.AsyncEx.Internals;

namespace UnitTests
{
    public class AsyncWaitQueueUnitTests
    {
        [Fact]
        public void IsEmpty_WhenEmpty_IsTrue()
        {
            var queue = DefaultAsyncWaitQueue<object>.Empty as IAsyncWaitQueue<object>;
            Assert.True(queue.IsEmpty);
        }

        [Fact]
        public void IsEmpty_WithOneItem_IsFalse()
        {
            var queue = DefaultAsyncWaitQueue<object>.Empty as IAsyncWaitQueue<object>;
            queue = queue.Enqueue(out _);
            Assert.False(queue.IsEmpty);
        }

        [Fact]
        public void IsEmpty_WithTwoItems_IsFalse()
        {
            var queue = DefaultAsyncWaitQueue<object>.Empty as IAsyncWaitQueue<object>;
            queue = queue.Enqueue(out _);
            queue = queue.Enqueue(out _);
            Assert.False(queue.IsEmpty);
        }

        [Fact]
        public void Dequeue_SynchronouslyCompletesTask()
        {
            var queue = DefaultAsyncWaitQueue<object>.Empty as IAsyncWaitQueue<object>;
            queue = queue.Enqueue(out var task);
            queue = queue.Dequeue(out var completion);
            completion?.Invoke();
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task Dequeue_WithTwoItems_OnlyCompletesFirstItem()
        {
            var queue = DefaultAsyncWaitQueue<object>.Empty as IAsyncWaitQueue<object>;
            queue = queue.Enqueue(out var task1);
            queue = queue.Enqueue(out var task2);
            queue = queue.Dequeue(out var completion);
            completion?.Invoke();
            Assert.True(task1.IsCompleted);
            await AsyncAssert.NeverCompletesAsync(task2);
        }

        [Fact]
        public void Dequeue_WithResult_SynchronouslyCompletesWithResult()
        {
            var queue = DefaultAsyncWaitQueue<object>.Empty as IAsyncWaitQueue<object>;
            var result = new object();
            queue = queue.Enqueue(out var task);
            queue = queue.Dequeue(out var completion, result);
            completion?.Invoke();
            Assert.Same(result, task.Result);
        }

        [Fact]
        public void Dequeue_WithoutResult_SynchronouslyCompletesWithDefaultResult()
        {
            var queue = DefaultAsyncWaitQueue<object>.Empty as IAsyncWaitQueue<object>;
            queue = queue.Enqueue(out var task);
            queue = queue.Dequeue(out var completion);
            completion?.Invoke();
            Assert.Equal(default(object), task.Result);
        }

        [Fact]
        public void DequeueAll_SynchronouslyCompletesAllTasks()
        {
            var queue = DefaultAsyncWaitQueue<object>.Empty as IAsyncWaitQueue<object>;
            queue = queue.Enqueue(out var task1);
            queue = queue.Enqueue(out var task2);
            queue = queue.DequeueAll(out var completion);
            completion?.Invoke();
            Assert.True(task1.IsCompleted);
            Assert.True(task2.IsCompleted);
        }

        [Fact]
        public void DequeueAll_WithoutResult_SynchronouslyCompletesAllTasksWithDefaultResult()
        {
            var queue = DefaultAsyncWaitQueue<object>.Empty as IAsyncWaitQueue<object>;
            queue = queue.Enqueue(out var task1);
            queue = queue.Enqueue(out var task2);
            queue = queue.DequeueAll(out var completion);
            completion?.Invoke();
            Assert.Equal(default(object), task1.Result);
            Assert.Equal(default(object), task2.Result);
        }

        [Fact]
        public void DequeueAll_WithResult_CompletesAllTasksWithResult()
        {
            var queue = DefaultAsyncWaitQueue<object>.Empty as IAsyncWaitQueue<object>;
            var result = new object();
            queue = queue.Enqueue(out var task1);
            queue = queue.Enqueue(out var task2);
            queue = queue.DequeueAll(out var completion, result);
            completion?.Invoke();
            Assert.Same(result, task1.Result);
            Assert.Same(result, task2.Result);
        }

        [Fact]
        public void TryCancel_EntryFound_SynchronouslyCancelsTask()
        {
            var queue = DefaultAsyncWaitQueue<object>.Empty as IAsyncWaitQueue<object>;
            queue = queue.Enqueue(out var task);
            var canceledQueue = queue.TryCancel(out var completion, task, new CancellationToken(true));
            completion?.Invoke();
            Assert.True(task.IsCanceled);
        }

        [Fact]
        public void TryCancel_EntryFound_RemovesTaskFromQueue()
        {
            var queue = DefaultAsyncWaitQueue<object>.Empty as IAsyncWaitQueue<object>;
            queue = queue.Enqueue(out var task);
            var canceledQueue = queue.TryCancel(out var completion, task, new CancellationToken(true));
            completion?.Invoke();
            Assert.True(canceledQueue!.IsEmpty);
        }

        [Fact]
        public void TryCancel_EntryNotFound_DoesNotRemoveTaskFromQueue()
        {
            var queue = DefaultAsyncWaitQueue<object>.Empty as IAsyncWaitQueue<object>;
            queue = queue.Enqueue(out var task);
            queue = queue.Enqueue(out _);
            queue = queue.Dequeue(out var continuation);
            continuation?.Invoke();
            var canceledQueue = queue.TryCancel(out continuation, task, new CancellationToken(true));
            continuation?.Invoke();
            Assert.Null(canceledQueue);
            Assert.False(queue.IsEmpty);
        }

        [Fact]
        public async Task Cancelled_WhenInQueue_CancelsTask()
        {
            var queue = DefaultAsyncWaitQueue<object>.Empty as IAsyncWaitQueue<object>;
            var cts = new CancellationTokenSource();
            queue = queue.Enqueue(cancel => queue = cancel(queue), cts.Token, out var task);
            cts.Cancel();
            await AsyncAssert.ThrowsAsync<OperationCanceledException>(task);
        }

        [Fact]
        public async Task Cancelled_WhenInQueue_RemovesTaskFromQueue()
        {
            var queue = DefaultAsyncWaitQueue<object>.Empty as IAsyncWaitQueue<object>;
            var cts = new CancellationTokenSource();
            queue = queue.Enqueue(cancel => queue = cancel(queue), cts.Token, out var task);
            cts.Cancel();
            await AsyncAssert.ThrowsAsync<OperationCanceledException>(task);
            Assert.True(queue.IsEmpty);
        }

        [Fact]
        public void Cancelled_WhenNotInQueue_DoesNotRemoveTaskFromQueue()
        {
            var queue = DefaultAsyncWaitQueue<object>.Empty as IAsyncWaitQueue<object>;
            var cts = new CancellationTokenSource();
            queue = queue.Enqueue(cancel => queue = cancel(queue), cts.Token, out var task);
            queue = queue.Enqueue(out _);
            queue = queue.Dequeue(out var continuation);
            continuation?.Invoke();
            cts.Cancel();
            Assert.False(queue.IsEmpty);
        }

        [Fact]
        public void Cancelled_BeforeEnqueue_SynchronouslyCancelsTask()
        {
            var queue = DefaultAsyncWaitQueue<object>.Empty as IAsyncWaitQueue<object>;
            var cts = new CancellationTokenSource();
            cts.Cancel();
            queue = queue.Enqueue(cancel => queue = cancel(queue), cts.Token, out var task);
            Assert.True(task.IsCanceled);
        }

        [Fact]
        public void Cancelled_BeforeEnqueue_RemovesTaskFromQueue()
        {
            var queue = DefaultAsyncWaitQueue<object>.Empty as IAsyncWaitQueue<object>;
            var cts = new CancellationTokenSource();
            cts.Cancel();
            queue = queue.Enqueue(cancel => queue = cancel(queue), cts.Token, out var task);
            Assert.True(queue.IsEmpty);
        }
    }
}

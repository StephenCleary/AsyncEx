using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Nito.AsyncEx.Testing;

namespace UnitTests
{
    public class AsyncProducerConsumerQueueUnitTests
    {
        [Fact]
        public void ConstructorWithZeroMaxCount_Throws()
        {
            AsyncAssert.Throws<ArgumentOutOfRangeException>(() => new AsyncProducerConsumerQueue<int>(0));
        }

        [Fact]
        public void ConstructorWithZeroMaxCountAndCollection_Throws()
        {
            AsyncAssert.Throws<ArgumentOutOfRangeException>(() => new AsyncProducerConsumerQueue<int>(new int[0], 0));
        }

        [Fact]
        public void ConstructorWithMaxCountSmallerThanCollectionCount_Throws()
        {
            AsyncAssert.Throws<ArgumentException>(() => new AsyncProducerConsumerQueue<int>(new[] { 3, 5 }, 1));
        }

        [Fact]
        public async Task ConstructorWithCollection_AddsItems()
        {
            var queue = new AsyncProducerConsumerQueue<int>(new[] { 3, 5, 7 });

            var result1 = await queue.DequeueAsync();
            var result2 = await queue.DequeueAsync();
            var result3 = await queue.DequeueAsync();

            Assert.Equal(3, result1);
            Assert.Equal(5, result2);
            Assert.Equal(7, result3);
        }

        [Fact]
        public async Task EnqueueAsync_SpaceAvailable_EnqueuesItem()
        {
            var queue = new AsyncProducerConsumerQueue<int>();

            await queue.EnqueueAsync(3);
            var result = await queue.DequeueAsync();

            Assert.Equal(3, result);
        }

        [Fact]
        public async Task EnqueueAsync_CompleteAdding_ThrowsException()
        {
            var queue = new AsyncProducerConsumerQueue<int>();
            queue.CompleteAdding();

            await AsyncAssert.ThrowsAsync<InvalidOperationException>(() => queue.EnqueueAsync(3));
        }

        [Fact]
        public async Task DequeueAsync_EmptyAndComplete_ThrowsException()
        {
            var queue = new AsyncProducerConsumerQueue<int>();
            queue.CompleteAdding();

            await AsyncAssert.ThrowsAsync<InvalidOperationException>(() => queue.DequeueAsync());
        }

        [Fact]
        public async Task DequeueAsync_Empty_DoesNotComplete()
        {
            var queue = new AsyncProducerConsumerQueue<int>();

            var task = queue.DequeueAsync();

            await AsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task DequeueAsync_Empty_ItemAdded_Completes()
        {
            var queue = new AsyncProducerConsumerQueue<int>();
            var task = queue.DequeueAsync();

            await queue.EnqueueAsync(13);
            var result = await task;

            Assert.Equal(13, result);
        }

        [Fact]
        public async Task DequeueAsync_Cancelled_Throws()
        {
            var queue = new AsyncProducerConsumerQueue<int>();
            var cts = new CancellationTokenSource();
            var task = queue.DequeueAsync(cts.Token);

            cts.Cancel();

            await AsyncAssert.ThrowsAsync<OperationCanceledException>(() => task);
        }

        [Fact]
        public async Task EnqueueAsync_Full_DoesNotComplete()
        {
            var queue = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);

            var task = queue.EnqueueAsync(7);

            await AsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task EnqueueAsync_SpaceAvailable_Completes()
        {
            var queue = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
            var task = queue.EnqueueAsync(7);

            await queue.DequeueAsync();

            await task;
        }

        [Fact]
        public async Task EnqueueAsync_Cancelled_Throws()
        {
            var queue = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
            var cts = new CancellationTokenSource();
            var task = queue.EnqueueAsync(7, cts.Token);

            cts.Cancel();

            await AsyncAssert.ThrowsAsync<OperationCanceledException>(() => task);
        }

        [Fact]
        public void CompleteAdding_MultipleTimes_DoesNotThrow()
        {
            var queue = new AsyncProducerConsumerQueue<int>();
            queue.CompleteAdding();

            queue.CompleteAdding();
        }

        [Fact]
        public async Task OutputAvailableAsync_NoItemsInQueue_IsNotCompleted()
        {
            var queue = new AsyncProducerConsumerQueue<int>();

            var task = queue.OutputAvailableAsync();

            await AsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task OutputAvailableAsync_ItemInQueue_ReturnsTrue()
        {
            var queue = new AsyncProducerConsumerQueue<int>();
            queue.Enqueue(13);

            var result = await queue.OutputAvailableAsync();
            Assert.True(result);
        }

        [Fact]
        public async Task OutputAvailableAsync_NoItemsAndCompleted_ReturnsFalse()
        {
            var queue = new AsyncProducerConsumerQueue<int>();
            queue.CompleteAdding();

            var result = await queue.OutputAvailableAsync();
            Assert.False(result);
        }

        [Fact]
        public async Task OutputAvailableAsync_ItemInQueueAndCompleted_ReturnsTrue()
        {
            var queue = new AsyncProducerConsumerQueue<int>();
            queue.Enqueue(13);
            queue.CompleteAdding();

            var result = await queue.OutputAvailableAsync();
            Assert.True(result);
        }

        [Fact]
        public async Task StandardAsyncSingleConsumerCode()
        {
            var queue = new AsyncProducerConsumerQueue<int>();
            var producer = Task.Run(() =>
            {
                queue.Enqueue(3);
                queue.Enqueue(13);
                queue.Enqueue(17);
                queue.CompleteAdding();
            });

            var results = new List<int>();
            while (await queue.OutputAvailableAsync())
            {
                results.Add(queue.Dequeue());
            }

            Assert.Equal(3, results.Count);
            Assert.Equal(3, results[0]);
            Assert.Equal(13, results[1]);
            Assert.Equal(17, results[2]);
        }
    }
}

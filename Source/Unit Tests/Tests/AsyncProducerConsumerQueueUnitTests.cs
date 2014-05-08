using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Threading.Tasks;
using Nito.AsyncEx;
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
    public class AsyncProducerConsumerQueueUnitTests
    {
        [Test]
        public void ConstructorWithZeroMaxCount_Throws()
        {
            AssertEx.ThrowsException<ArgumentOutOfRangeException>(() => new AsyncProducerConsumerQueue<int>(0));
        }

        [Test]
        public void ConstructorWithZeroMaxCountAndCollection_Throws()
        {
            AssertEx.ThrowsException<ArgumentOutOfRangeException>(() => new AsyncProducerConsumerQueue<int>(new int[0], 0));
        }

        [Test]
        public void ConstructorWithMaxCountSmallerThanCollectionCount_Throws()
        {
            AssertEx.ThrowsException<ArgumentException>(() => new AsyncProducerConsumerQueue<int>(new[] { 3, 5 }, 1));
        }

        [Test]
        public void ConstructorWithCollection_AddsItems()
        {
            Test.Async(async () =>
            {
                var queue = new AsyncProducerConsumerQueue<int>(new[] { 3, 5, 7 });

                var result1 = await queue.DequeueAsync();
                var result2 = await queue.DequeueAsync();
                var result3 = await queue.DequeueAsync();

                Assert.AreEqual(3, result1);
                Assert.AreEqual(5, result2);
                Assert.AreEqual(7, result3);
            });
        }

        [Test]
        public void EnqueueAsync_SpaceAvailable_EnqueuesItem()
        {
            Test.Async(async () =>
            {
                var queue = new AsyncProducerConsumerQueue<int>();

                await queue.EnqueueAsync(3);
                var result = await queue.DequeueAsync();

                Assert.AreEqual(3, result);
            });
        }

        [Test]
        public void TryEnqueueAsync_SpaceAvailable_EnqueuesItem()
        {
            Test.Async(async () =>
            {
                var queue = new AsyncProducerConsumerQueue<int>();

                var result = await queue.TryEnqueueAsync(3);
                var dequeueResult = await queue.DequeueAsync();

                Assert.IsTrue(result);
                Assert.AreEqual(3, dequeueResult);
            });
        }

        [Test]
        public void EnqueueToAnyAsync_SpaceAvailable_EnqueuesItem()
        {
            Test.Async(async () =>
            {
                var queue1 = new AsyncProducerConsumerQueue<int>(new[] { 3 }, 1);
                var queue2 = new AsyncProducerConsumerQueue<int>();
                var queues = new[] { queue1, queue2 };

                var result = await queues.EnqueueToAnyAsync(13);
                var dequeueResult = await queue2.DequeueAsync();

                Assert.AreSame(queue2, result);
                Assert.AreEqual(13, dequeueResult);
            });
        }

        [Test]
        public void TryEnqueueToAnyAsync_SpaceAvailable_EnqueuesItem()
        {
            Test.Async(async () =>
            {
                var queue1 = new AsyncProducerConsumerQueue<int>();
                var queue2 = new AsyncProducerConsumerQueue<int>(new[] { 3 }, 1);
                var queues = new[] { queue1, queue2 };

                var result = await queues.TryEnqueueToAnyAsync(13);
                var dequeueResult = await queue1.DequeueAsync();

                Assert.AreSame(queue1, result);
                Assert.AreEqual(13, dequeueResult);
            });
        }

        [Test]
        public void TryEnqueueAsync_CompleteAdding_ReturnsFailed()
        {
            Test.Async(async () =>
            {
                var queue = new AsyncProducerConsumerQueue<int>();
                queue.CompleteAdding();

                var result = await queue.TryEnqueueAsync(3);

                Assert.IsFalse(result);
            });
        }

        [Test]
        public void EnqueueAsync_CompleteAdding_ThrowsException()
        {
            Test.Async(async () =>
            {
                var queue = new AsyncProducerConsumerQueue<int>();
                queue.CompleteAdding();

                await AssertEx.ThrowsExceptionAsync<InvalidOperationException>(() => queue.EnqueueAsync(3));
            });
        }

        [Test]
        public void TryEnqueueToAnyAsync_CompleteAdding_ReturnsFailed()
        {
            Test.Async(async () =>
            {
                var queue1 = new AsyncProducerConsumerQueue<int>();
                queue1.CompleteAdding();
                var queue2 = new AsyncProducerConsumerQueue<int>();
                queue2.CompleteAdding();
                var queues = new[] { queue1, queue2 };

                var result = await queues.TryEnqueueToAnyAsync(3);

                Assert.IsNull(result);
            });
        }

        [Test]
        public void EnqueueToAnyAsync_CompleteAdding_ThrowsException()
        {
            Test.Async(async () =>
            {
                var queue1 = new AsyncProducerConsumerQueue<int>();
                queue1.CompleteAdding();
                var queue2 = new AsyncProducerConsumerQueue<int>();
                queue2.CompleteAdding();
                var queues = new[] { queue1, queue2 };

                await AssertEx.ThrowsExceptionAsync<InvalidOperationException>(() => queues.EnqueueToAnyAsync(3));
            });
        }

        [Test]
        public void DequeueAsync_EmptyAndComplete_ThrowsException()
        {
            Test.Async(async () =>
            {
                var queue = new AsyncProducerConsumerQueue<int>();
                queue.CompleteAdding();

                await AssertEx.ThrowsExceptionAsync<InvalidOperationException>(() => queue.DequeueAsync());
            });
        }

        [Test]
        public void TryDequeueAsync_EmptyAndComplete_ReturnsFailed()
        {
            Test.Async(async () =>
            {
                var queue = new AsyncProducerConsumerQueue<int>();
                queue.CompleteAdding();

                var result = await queue.TryDequeueAsync();

                Assert.IsFalse(result.Success);
            });
        }

        [Test]
        public void DequeueFromAnyAsync_EmptyAndComplete_ThrowsException()
        {
            Test.Async(async () =>
            {
                var queue1 = new AsyncProducerConsumerQueue<int>();
                queue1.CompleteAdding();
                var queue2 = new AsyncProducerConsumerQueue<int>();
                queue2.CompleteAdding();
                var queues = new[] { queue1, queue2 };

                await AssertEx.ThrowsExceptionAsync<InvalidOperationException>(() => queues.DequeueFromAnyAsync());
            });
        }

        [Test]
        public void TryDequeueFromAnyAsync_EmptyAndComplete_ReturnsFailed()
        {
            Test.Async(async () =>
            {
                var queue1 = new AsyncProducerConsumerQueue<int>();
                queue1.CompleteAdding();
                var queue2 = new AsyncProducerConsumerQueue<int>();
                queue2.CompleteAdding();
                var queues = new[] { queue1, queue2 };

                var result = await queues.TryDequeueFromAnyAsync();

                Assert.IsFalse(result.Success);
            });
        }

        [Test]
        public void DequeueAsync_Empty_DoesNotComplete()
        {
            Test.Async(async () =>
            {
                var queue = new AsyncProducerConsumerQueue<int>();

                var task = queue.DequeueAsync();

                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void TryDequeueAsync_Empty_DoesNotComplete()
        {
            Test.Async(async () =>
            {
                var queue = new AsyncProducerConsumerQueue<int>();

                var task = queue.TryDequeueAsync();

                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void DequeueFromAnyAsync_Empty_DoesNotComplete()
        {
            Test.Async(async () =>
            {
                var queue1 = new AsyncProducerConsumerQueue<int>();
                var queue2 = new AsyncProducerConsumerQueue<int>();
                var queues = new[] { queue1, queue2 };

                var task = queues.DequeueFromAnyAsync();

                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void TryDequeueFromAnyAsync_Empty_DoesNotComplete()
        {
            Test.Async(async () =>
            {
                var queue1 = new AsyncProducerConsumerQueue<int>();
                var queue2 = new AsyncProducerConsumerQueue<int>();
                var queues = new[] { queue1, queue2 };

                var task = queues.TryDequeueFromAnyAsync();

                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void DequeueAsync_Empty_ItemAdded_Completes()
        {
            Test.Async(async () =>
            {
                var queue = new AsyncProducerConsumerQueue<int>();
                var task = queue.DequeueAsync();

                await queue.EnqueueAsync(13);
                var result = await task;

                Assert.AreEqual(13, result);
            });
        }

        [Test]
        public void DequeueFromAnyAsync_Empty_ItemAdded_Completes()
        {
            Test.Async(async () =>
            {
                var queue1 = new AsyncProducerConsumerQueue<int>();
                var queue2 = new AsyncProducerConsumerQueue<int>();
                var queues = new[] { queue1, queue2 };
                var task = queues.DequeueFromAnyAsync();

                await queue2.EnqueueAsync(13);
                var result = await task;

                Assert.IsTrue(result.Success);
                Assert.AreSame(queue2, result.Queue);
                Assert.AreEqual(13, result.Item);
            });
        }

        [Test]
        public void TryDequeueAsync_Cancelled_Throws()
        {
            Test.Async(async () =>
            {
                var queue = new AsyncProducerConsumerQueue<int>();
                var cts = new CancellationTokenSource();
                var task = queue.TryDequeueAsync(cts.Token);

                cts.Cancel();

                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => task);
            });
        }

        [Test]
        public void DequeueAsync_Cancelled_Throws()
        {
            Test.Async(async () =>
            {
                var queue = new AsyncProducerConsumerQueue<int>();
                var cts = new CancellationTokenSource();
                var task = queue.DequeueAsync(cts.Token);

                cts.Cancel();

                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => task);
            });
        }

        [Test]
        public void TryDequeueFromAnyAsync_Cancelled_Throws()
        {
            Test.Async(async () =>
            {
                var queue1 = new AsyncProducerConsumerQueue<int>();
                var queue2 = new AsyncProducerConsumerQueue<int>();
                var queues = new[] { queue1, queue2 };
                var cts = new CancellationTokenSource();
                var task = queues.TryDequeueFromAnyAsync(cts.Token);

                cts.Cancel();

                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => task);
            });
        }

        [Test]
        public void DequeueFromAnyAsync_Cancelled_Throws()
        {
            Test.Async(async () =>
            {
                var queue1 = new AsyncProducerConsumerQueue<int>();
                var queue2 = new AsyncProducerConsumerQueue<int>();
                var queues = new[] { queue1, queue2 };
                var cts = new CancellationTokenSource();
                var task = queues.DequeueFromAnyAsync(cts.Token);

                cts.Cancel();

                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => task);
            });
        }

        [Test]
        public void EnqueueAsync_Full_DoesNotComplete()
        {
            Test.Async(async () =>
            {
                var queue = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);

                var task = queue.EnqueueAsync(7);

                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void TryEnqueueAsync_Full_DoesNotComplete()
        {
            Test.Async(async () =>
            {
                var queue = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);

                var task = queue.TryEnqueueAsync(7);

                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void EnqueueToAnyAsync_Full_DoesNotComplete()
        {
            Test.Async(async () =>
            {
                var queue1 = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
                var queue2 = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
                var queues = new[] { queue1, queue2 };

                var task = queues.EnqueueToAnyAsync(7);

                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void TryEnqueueToAnyAsync_Full_DoesNotComplete()
        {
            Test.Async(async () =>
            {
                var queue1 = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
                var queue2 = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
                var queues = new[] { queue1, queue2 };

                var task = queues.TryEnqueueToAnyAsync(7);

                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void EnqueueAsync_SpaceAvailable_Completes()
        {
            Test.Async(async () =>
            {
                var queue = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
                var task = queue.EnqueueAsync(7);

                await queue.DequeueAsync();

                await task;
            });
        }

        [Test]
        public void TryEnqueueAsync_SpaceAvailable_Completes()
        {
            Test.Async(async () =>
            {
                var queue = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
                var task = queue.TryEnqueueAsync(7);

                await queue.DequeueAsync();
                var result = await task;

                Assert.IsTrue(result);
            });
        }

        [Test]
        public void EnqueueToAnyAsync_SpaceAvailable_Completes()
        {
            Test.Async(async () =>
            {
                var queue1 = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
                var queue2 = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
                var queues = new[] { queue1, queue2 };
                var task = queues.EnqueueToAnyAsync(7);

                await queue1.DequeueAsync();
                var result = await task;

                Assert.AreSame(queue1, result);
            });
        }

        [Test]
        public void TryEnqueueToAnyAsync_SpaceAvailable_Completes()
        {
            Test.Async(async () =>
            {
                var queue1 = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
                var queue2 = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
                var queues = new[] { queue1, queue2 };
                var task = queues.TryEnqueueToAnyAsync(7);

                await queue2.DequeueAsync();
                var result = await task;

                Assert.AreSame(queue2, result);
            });
        }

        [Test]
        public void TryEnqueueAsync_Cancelled_Throws()
        {
            Test.Async(async () =>
            {
                var queue = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
                var cts = new CancellationTokenSource();
                var task = queue.TryEnqueueAsync(7, cts.Token);

                cts.Cancel();

                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => task);
            });
        }

        [Test]
        public void EnqueueAsync_Cancelled_Throws()
        {
            Test.Async(async () =>
            {
                var queue = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
                var cts = new CancellationTokenSource();
                var task = queue.EnqueueAsync(7, cts.Token);

                cts.Cancel();

                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => task);
            });
        }

        [Test]
        public void TryEnqueueToAnyAsync_Cancelled_Throws()
        {
            Test.Async(async () =>
            {
                var queue1 = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
                var queue2 = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
                var queues = new[] { queue1, queue2 };
                var cts = new CancellationTokenSource();
                var task = queues.TryEnqueueToAnyAsync(7, cts.Token);

                cts.Cancel();

                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => task);
            });
        }

        [Test]
        public void EnqueueToAnyAsync_Cancelled_Throws()
        {
            Test.Async(async () =>
            {
                var queue1 = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
                var queue2 = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
                var queues = new[] { queue1, queue2 };
                var cts = new CancellationTokenSource();
                var task = queues.EnqueueToAnyAsync(7, cts.Token);

                cts.Cancel();

                await AssertEx.ThrowsExceptionAsync<OperationCanceledException>(() => task);
            });
        }

        [Test]
        public void EnqueueToAnyAsync_MultipleHaveRoom_OnlyEnqueuesOne()
        {
            Test.Async(async () =>
            {
                var queue1 = new AsyncProducerConsumerQueue<int>();
                var queue2 = new AsyncProducerConsumerQueue<int>();
                var queues = new[] { queue1, queue2 };

                var result = await queues.EnqueueToAnyAsync(13);
                Task task = ((result == queue1) ? queue2 : queue1).DequeueAsync();
                await result.DequeueAsync();

                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void DequeueFromAnyAsync_MultipleHaveEntries_OnlyDequeuesOne()
        {
            Test.Async(async () =>
            {
                var queue1 = new AsyncProducerConsumerQueue<int>(new[] { 5 });
                var queue2 = new AsyncProducerConsumerQueue<int>(new[] { 7 });
                var queues = new[] { queue1, queue2 };

                var result = await queues.DequeueFromAnyAsync();
                await ((result.Queue == queue1) ? queue2 : queue1).DequeueAsync();
            });
        }

        [Test]
        public void CompleteAdding_MultipleTimes_DoesNotThrow()
        {
            var queue = new AsyncProducerConsumerQueue<int>();
            queue.CompleteAdding();

            queue.CompleteAdding();
        }

        [Test]
        public async Task OutputAvailableAsync_NoItemsInQueue_IsNotCompleted()
        {
            var queue = new AsyncProducerConsumerQueue<int>();

            var task = queue.OutputAvailableAsync();

            await AssertEx.NeverCompletesAsync(task);
        }

        [Test]
        public async Task OutputAvailableAsync_ItemInQueue_ReturnsTrue()
        {
            var queue = new AsyncProducerConsumerQueue<int>();
            queue.Enqueue(13);

            var result = await queue.OutputAvailableAsync();
            Assert.IsTrue(result);
        }

        [Test]
        public async Task OutputAvailableAsync_NoItemsAndCompleted_ReturnsFalse()
        {
            var queue = new AsyncProducerConsumerQueue<int>();
            queue.CompleteAdding();

            var result = await queue.OutputAvailableAsync();
            Assert.IsFalse(result);
        }

        [Test]
        public async Task OutputAvailableAsync_ItemInQueueAndCompleted_ReturnsTrue()
        {
            var queue = new AsyncProducerConsumerQueue<int>();
            queue.Enqueue(13);
            queue.CompleteAdding();

            var result = await queue.OutputAvailableAsync();
            Assert.IsTrue(result);
        }

        [Test]
        public async Task StandardAsyncSingleConsumerCode()
        {
            var queue = new AsyncProducerConsumerQueue<int>();
            var producer = TaskShim.Run(() =>
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

            CollectionAssert.AreEqual(new[] { 3, 13, 17 }, results);
        }
    }
}

using System;
using NUnit.Framework;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using Nito.AsyncEx.Internal;

namespace Tests
{
    [TestFixture]
    public class AsyncBarrierUnitTests_NET40
    {
        [Test]
        public void SignalAndWaitAsync_LastParticipant_CompletesPhase()
        {
            AsyncContext.Run(async () =>
            {
                var barrier = new AsyncBarrier(1);
                Assert.AreEqual(0, barrier.CurrentPhaseNumber);
                Assert.AreEqual(1, barrier.ParticipantCount);
                Assert.AreEqual(1, barrier.ParticipantsRemaining);
                await barrier.SignalAndWaitAsync();
                Assert.AreEqual(1, barrier.CurrentPhaseNumber);
                Assert.AreEqual(1, barrier.ParticipantCount);
                Assert.AreEqual(1, barrier.ParticipantsRemaining);
            });
        }

        [Test]
        public void SignalAndWaitAsync_NoParticipants_ThrowsException()
        {
            var barrier = new AsyncBarrier(0);
            Assert.AreEqual(0, barrier.CurrentPhaseNumber);
            Assert.AreEqual(0, barrier.ParticipantCount);
            Assert.AreEqual(0, barrier.ParticipantsRemaining);
            AssertEx.ThrowsException<InvalidOperationException>(() => barrier.SignalAndWaitAsync());
        }

        [Test]
        public void SignalAndWaitAsync_MultipleParticipants_CompleteTogether()
        {
            AsyncContext.Run(async () =>
            {
                var barrier = new AsyncBarrier(2);
                Assert.AreEqual(0, barrier.CurrentPhaseNumber);
                Assert.AreEqual(2, barrier.ParticipantCount);
                Assert.AreEqual(2, barrier.ParticipantsRemaining);
                var task1 = barrier.SignalAndWaitAsync();
                Assert.AreEqual(0, barrier.CurrentPhaseNumber);
                Assert.AreEqual(2, barrier.ParticipantCount);
                Assert.AreEqual(1, barrier.ParticipantsRemaining);
                var task2 = barrier.SignalAndWaitAsync();
                Assert.AreSame(task1, task2);
                await task1;
                Assert.AreEqual(1, barrier.CurrentPhaseNumber);
                Assert.AreEqual(2, barrier.ParticipantCount);
                Assert.AreEqual(2, barrier.ParticipantsRemaining);
            });
        }

        [Test]
        public void SignalAndWaitAsync_Underflow_ThrowsException()
        {
            var tcs = new TaskCompletionSource();
            var barrier = new AsyncBarrier(1, async _ => { await tcs.Task; });
            barrier.SignalAndWaitAsync();
            AssertEx.ThrowsException<InvalidOperationException>(() => barrier.SignalAndWaitAsync());
            tcs.SetResult();
        }

        [Test]
        public void PostPhaseAction_ExecutedAtEndOfEachPhase()
        {
            AsyncContext.Run(async () =>
            {
                int executed = 0;
                var barrier = new AsyncBarrier(1, _ => { Interlocked.Increment(ref executed); });
                await barrier.SignalAndWaitAsync();
                Assert.AreEqual(1, Interlocked.CompareExchange(ref executed, 0, 0));
                await barrier.SignalAndWaitAsync();
                Assert.AreEqual(2, Interlocked.CompareExchange(ref executed, 0, 0));
            });
        }

        [Test]
        public void PostPhaseAction_PropagatesExceptions()
        {
            AsyncContext.Run(async () =>
            {
                var barrier = new AsyncBarrier(1, _ => { throw new NotImplementedException(); });
                await AssertEx.ThrowsExceptionAsync<NotImplementedException>(barrier.SignalAndWaitAsync(), allowDerivedTypes: false);
            });
        }

        [Test]
        public void AsyncPostPhaseAction_PropagatesExceptions()
        {
            AsyncContext.Run(async () =>
            {
                var barrier = new AsyncBarrier(1, async _ =>
                {
                    await Task.Yield();
                    throw new NotImplementedException();
                });
                await AssertEx.ThrowsExceptionAsync<NotImplementedException>(barrier.SignalAndWaitAsync(), allowDerivedTypes: false);
            });
        }

        [Test]
        public void RemoveParticipantsAsync_Underflow_ThrowsException()
        {
            var barrier = new AsyncBarrier(2);
            barrier.SignalAndWaitAsync();
            AssertEx.ThrowsException<InvalidOperationException>(() => barrier.RemoveParticipantsAsync(2));
        }

        [Test]
        public void RemoveParticipantsAsync_LastParticipant_CompletesPhase()
        {
            AsyncContext.Run(async () =>
            {
                var barrier = new AsyncBarrier(1);
                Assert.AreEqual(0, barrier.CurrentPhaseNumber);
                Assert.AreEqual(1, barrier.ParticipantCount);
                Assert.AreEqual(1, barrier.ParticipantsRemaining);
                await barrier.RemoveParticipantsAsync(1);
                Assert.AreEqual(1, barrier.CurrentPhaseNumber);
                Assert.AreEqual(0, barrier.ParticipantCount);
                Assert.AreEqual(0, barrier.ParticipantsRemaining);
            });
        }

        [Test]
        public void AddParticipants_Overflow_ThrowsException()
        {
            var barrier = new AsyncBarrier(int.MaxValue);
            AssertEx.ThrowsException<InvalidOperationException>(() => barrier.AddParticipants());
        }

        [Test]
        public void AddParticipants_IncreasesParticipantsForCurrentPhase()
        {
            var barrier = new AsyncBarrier(1);
            Assert.AreEqual(0, barrier.CurrentPhaseNumber);
            Assert.AreEqual(1, barrier.ParticipantCount);
            Assert.AreEqual(1, barrier.ParticipantsRemaining);
            barrier.AddParticipants(2);
            Assert.AreEqual(0, barrier.CurrentPhaseNumber);
            Assert.AreEqual(3, barrier.ParticipantCount);
            Assert.AreEqual(3, barrier.ParticipantsRemaining);
        }

        [Test]
        public void AddParticipants_FromZero_IncreasesParticipantsForCurrentPhase()
        {
            AsyncContext.Run(async () =>
            {
                var barrier = new AsyncBarrier(1);
                Assert.AreEqual(0, barrier.CurrentPhaseNumber);
                Assert.AreEqual(1, barrier.ParticipantCount);
                Assert.AreEqual(1, barrier.ParticipantsRemaining);
                await barrier.RemoveParticipantsAsync();
                Assert.AreEqual(1, barrier.CurrentPhaseNumber);
                Assert.AreEqual(0, barrier.ParticipantCount);
                Assert.AreEqual(0, barrier.ParticipantsRemaining);
                barrier.AddParticipants();
                Assert.AreEqual(1, barrier.CurrentPhaseNumber);
                Assert.AreEqual(1, barrier.ParticipantCount);
                Assert.AreEqual(1, barrier.ParticipantsRemaining);
            });
        }

        [Test]
        public void AddZeroParticipants_DoesNothing()
        {
            var barrier = new AsyncBarrier(1);
            Assert.AreEqual(1, barrier.ParticipantCount);
            barrier.AddParticipants(0);
            Assert.AreEqual(1, barrier.ParticipantCount);
        }

        [Test]
        public void RemoveZeroParticipants_DoesNothing()
        {
            var barrier = new AsyncBarrier(1);
            Assert.AreEqual(1, barrier.ParticipantCount);
            barrier.RemoveParticipantsAsync(0);
            Assert.AreEqual(1, barrier.ParticipantCount);
        }

        [Test]
        public void SignalZero_DoesNothing()
        {
            AsyncContext.Run(async () =>
            {
                var barrier = new AsyncBarrier(1);
                Assert.AreEqual(1, barrier.ParticipantsRemaining);
                var task = barrier.SignalAndWaitAsync(0);
                Assert.AreEqual(1, barrier.ParticipantsRemaining);
                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void Id_IsNotZero()
        {
            var barrier = new AsyncBarrier(0);
            Assert.AreNotEqual(0, barrier.Id);
        }
    }
}

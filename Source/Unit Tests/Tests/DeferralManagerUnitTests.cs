using System;
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
    public class DeferralManagerUnitTests
    {
        [Test]
        public void NoDeferrals_IsCompleted()
        {
            var dm = new DeferralManager();
            var task = dm.SignalAndWaitAsync();
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void IncompleteDeferral_PreventsCompletion()
        {
            Test.Async(async () =>
            {
                var dm = new DeferralManager();
                var deferral = dm.GetDeferral();
                await AssertEx.NeverCompletesAsync(dm.SignalAndWaitAsync());
            });
        }

        [Test]
        public void DeferralCompleted_Completes()
        {
            Test.Async(async () =>
            {
                var dm = new DeferralManager();
                var deferral = dm.GetDeferral();
                var task = dm.SignalAndWaitAsync();
                Assert.IsFalse(task.IsCompleted);
                deferral.Dispose();
                await task;
            });
        }

        [Test]
        public void MultipleDeferralsWithOneIncomplete_PreventsCompletion()
        {
            Test.Async(async () =>
            {
                var dm = new DeferralManager();
                var deferral1 = dm.GetDeferral();
                var deferral2 = dm.GetDeferral();
                var task = dm.SignalAndWaitAsync();
                deferral1.Dispose();
                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void TwoDeferralsWithOneCompletedTwice_PreventsCompletion()
        {
            Test.Async(async () =>
            {
                var dm = new DeferralManager();
                var deferral1 = dm.GetDeferral();
                var deferral2 = dm.GetDeferral();
                var task = dm.SignalAndWaitAsync();
                deferral1.Dispose();
                deferral1.Dispose();
                await AssertEx.NeverCompletesAsync(task);
            });
        }

        [Test]
        public void MultipleDeferralsWithAllCompleted_Completes()
        {
            Test.Async(async () =>
            {
                var dm = new DeferralManager();
                var deferral1 = dm.GetDeferral();
                var deferral2 = dm.GetDeferral();
                var task = dm.SignalAndWaitAsync();
                deferral1.Dispose();
                deferral2.Dispose();
                await task;
            });
        }
    }
}

using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Xunit;
using Nito.AsyncEx.Testing;

namespace UnitTests
{
    public class DeferralManagerUnitTests
    {
        [Fact]
        public void NoDeferrals_IsCompleted()
        {
            var dm = new DeferralManager();
            var task = dm.WaitForDeferralsAsync();
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task IncompleteDeferral_PreventsCompletion()
        {
            var dm = new DeferralManager();
            var deferral = dm.DeferralSource.GetDeferral();
            await AsyncAssert.NeverCompletesAsync(dm.WaitForDeferralsAsync());
        }

        [Fact]
        public async Task DeferralCompleted_Completes()
        {
            var dm = new DeferralManager();
            var deferral = dm.DeferralSource.GetDeferral();
            var task = dm.WaitForDeferralsAsync();
            Assert.False(task.IsCompleted);
            deferral.Dispose();
            await task;
        }

        [Fact]
        public async Task MultipleDeferralsWithOneIncomplete_PreventsCompletion()
        {
            var dm = new DeferralManager();
            var deferral1 = dm.DeferralSource.GetDeferral();
            var deferral2 = dm.DeferralSource.GetDeferral();
            var task = dm.WaitForDeferralsAsync();
            deferral1.Dispose();
            await AsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task TwoDeferralsWithOneCompletedTwice_PreventsCompletion()
        {
            var dm = new DeferralManager();
            var deferral1 = dm.DeferralSource.GetDeferral();
            var deferral2 = dm.DeferralSource.GetDeferral();
            var task = dm.WaitForDeferralsAsync();
            deferral1.Dispose();
            deferral1.Dispose();
            await AsyncAssert.NeverCompletesAsync(task);
        }

        [Fact]
        public async Task MultipleDeferralsWithAllCompleted_Completes()
        {
            var dm = new DeferralManager();
            var deferral1 = dm.DeferralSource.GetDeferral();
            var deferral2 = dm.DeferralSource.GetDeferral();
            var task = dm.WaitForDeferralsAsync();
            deferral1.Dispose();
            deferral2.Dispose();
            await task;
        }

        [Fact]
        public async Task CompletedDeferralFollowedByIncompleteDeferral_PreventsCompletion()
        {
            var dm = new DeferralManager();
            dm.DeferralSource.GetDeferral().Dispose();
            var deferral = dm.DeferralSource.GetDeferral();
            var task = dm.WaitForDeferralsAsync();
            await AsyncAssert.NeverCompletesAsync(task);
        }
    }
}

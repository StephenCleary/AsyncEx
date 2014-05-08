using System;
using NUnit.Framework;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

namespace Tests
{
    [TestFixture]
    public class CancellationTokenHelpersUnitTests_NET40
    {
        [Test]
        public void Normalize_ManyUncancelableTokens_ReturnsTokenThatCannotCancel()
        {
            var result = CancellationTokenHelpers.Normalize(CancellationToken.None, CancellationToken.None, CancellationToken.None);

            Assert.IsFalse(result.Token.CanBeCanceled);
            Assert.AreEqual(CancellationToken.None, result.Token);
        }

        [Test]
        public void Normalize_OneCancelableTokenWithManyUncancelableTokens_ReturnsCancelableToken()
        {
            var cts = new CancellationTokenSource();

            var result = CancellationTokenHelpers.Normalize(CancellationToken.None, cts.Token, CancellationToken.None, CancellationToken.None);

            Assert.IsTrue(result.Token.CanBeCanceled);
            Assert.AreEqual(cts.Token, result.Token);
        }

        [Test]
        public void Normalize_ManyCancelableTokens_ReturnsNewCancelableToken()
        {
            var cts1 = new CancellationTokenSource();
            var cts2 = new CancellationTokenSource();

            var result = CancellationTokenHelpers.Normalize(cts1.Token, cts2.Token);

            Assert.IsTrue(result.Token.CanBeCanceled);
            Assert.AreNotEqual(cts1.Token, result);
            Assert.AreNotEqual(cts2.Token, result);
        }

        [Test]
        public void FromTask_ReturnsTokenWithoutCancellationRequested()
        {
            var tcs = new TaskCompletionSource();

            var result = CancellationTokenHelpers.FromTask(tcs.Task);

            Assert.IsTrue(result.Token.CanBeCanceled);
            Assert.IsFalse(result.Token.IsCancellationRequested);
        }

        [Test]
        public void FromTaskSynchronously_CompletedTask_ReturnsTokenWithCancellationRequested()
        {
            var tcs = new TaskCompletionSource();
            tcs.SetResult();

            var result = CancellationTokenHelpers.FromTask(tcs.Task, TaskContinuationOptions.ExecuteSynchronously);

            Assert.IsTrue(result.Token.CanBeCanceled);
            Assert.IsTrue(result.Token.IsCancellationRequested);
        }

        [Test]
        public void FromTaskSynchronously_TaskCompletes_TokenGetsCancellationRequested()
        {
            var tcs = new TaskCompletionSource();
            var result = CancellationTokenHelpers.FromTask(tcs.Task, TaskContinuationOptions.ExecuteSynchronously);

            tcs.SetResult();

            Assert.IsTrue(result.Token.CanBeCanceled);
            Assert.IsTrue(result.Token.IsCancellationRequested);
        }
    }
}

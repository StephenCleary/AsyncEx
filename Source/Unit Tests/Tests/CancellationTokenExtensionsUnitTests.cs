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
    public class CancellationTokenExtensionsUnitTests
    {
        [Test]
        public void AsTask_TokenThatCannotCancel_NeverCancels()
        {
            var task = CancellationToken.None.AsTask();

            // This is testing an implementation detail.
            Assert.AreSame(TaskConstants.Never, task);
        }

        [Test]
        public void AsTask_ConstructedCanceledToken_CancelsTask()
        {
            Test.Async(async () =>
            {
                var token = new CancellationToken(true);
                var task = token.AsTask();

                await AssertEx.ThrowsExceptionAsync<TaskCanceledException>(task);
            });
        }

        [Test]
        public void AsTask_CanceledToken_CancelsTask()
        {
            Test.Async(async () =>
            {
                var cts = new CancellationTokenSource();
                cts.Cancel();
                var task = cts.Token.AsTask();

                await AssertEx.ThrowsExceptionAsync<TaskCanceledException>(task);
            });
        }

        [Test]
        public void AsTask_TokenCanceled_CancelsTask()
        {
            Test.Async(async () =>
            {
                var cts = new CancellationTokenSource();
                var task = cts.Token.AsTask();
                Assert.IsFalse(task.IsCanceled);

                cts.Cancel();
                await AssertEx.ThrowsExceptionAsync<TaskCanceledException>(task);
            });
        }
    }
}

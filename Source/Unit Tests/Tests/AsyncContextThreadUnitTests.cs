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
    public class AsyncContextThreadUnitTests
    {
        [Test]
        public void AsyncContextThread_IsAnIndependentThread()
        {
            Test.Async(async () =>
            {
                var testThread = Thread.CurrentThread.ManagedThreadId;
                var thread = new AsyncContextThread();
                var contextThread = await thread.Factory.Run(() => Thread.CurrentThread.ManagedThreadId);
                Assert.AreNotEqual(testThread, contextThread);
                await thread.JoinAsync();
            });
        }

        [Test]
        public void AsyncDelegate_ResumesOnSameThread()
        {
            Test.Async(async () =>
            {
                var thread = new AsyncContextThread();
                int contextThread = -1, resumeThread = -1;
                await thread.Factory.Run(async () =>
                {
                    contextThread = Thread.CurrentThread.ManagedThreadId;
                    await TaskShim.Yield();
                    resumeThread = Thread.CurrentThread.ManagedThreadId;
                });
                Assert.AreEqual(contextThread, resumeThread);
                await thread.JoinAsync();
            });
        }

        [Test]
        public void Join_StopsTask()
        {
            Test.Async(async () =>
            {
                var context = new AsyncContextThread();
                var thread = await context.Factory.Run(() => Thread.CurrentThread);
                await context.JoinAsync();
            });
        }

        [Test]
        public void Context_IsCorrectAsyncContext()
        {
            Test.Async(async () =>
            {
                using (var thread = new AsyncContextThread())
                {
                    var observedContext = await thread.Factory.Run(() => AsyncContext.Current);
                    Assert.AreSame(observedContext, thread.Context);
                }
            });
        }
    }
}

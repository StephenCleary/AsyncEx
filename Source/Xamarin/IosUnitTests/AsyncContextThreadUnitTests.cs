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
    public class AsyncContextThreadUnitTests_NET40
    {
        [Test]
        public void AsyncContextThread_IsAnIndependentThread()
        {
            AsyncContext.Run(async () =>
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
            AsyncContext.Run(async () =>
            {
                var thread = new AsyncContextThread();
                int contextThread = -1, resumeThread = -1;
                await thread.Factory.Run(async () =>
                {
                    contextThread = Thread.CurrentThread.ManagedThreadId;
                    await Task.Yield();
                    resumeThread = Thread.CurrentThread.ManagedThreadId;
                });
                Assert.AreEqual(contextThread, resumeThread);
                await thread.JoinAsync();
            });
        }

        [Test]
        public void Join_StopsTask()
        {
            AsyncContext.Run(async () =>
            {
                var context = new AsyncContextThread();
                var thread = await context.Factory.Run(() => Thread.CurrentThread);
                await context.JoinAsync();
            });
        }

        [Test]
        public void Context_IsCorrectAsyncContext()
        {
            AsyncContext.Run(async () =>
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

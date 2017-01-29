using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace UnitTests
{
    public class AsyncContextThreadUnitTests
    {
        [Fact]
        public async Task AsyncContextThread_IsAnIndependentThread()
        {
            var testThread = Thread.CurrentThread.ManagedThreadId;
            var thread = new AsyncContextThread();
            var contextThread = await thread.Factory.Run(() => Thread.CurrentThread.ManagedThreadId);
            Assert.NotEqual(testThread, contextThread);
            await thread.JoinAsync();
        }

        [Fact]
        public async Task AsyncDelegate_ResumesOnSameThread()
        {
            var thread = new AsyncContextThread();
            int contextThread = -1, resumeThread = -1;
            await thread.Factory.Run(async () =>
            {
                contextThread = Thread.CurrentThread.ManagedThreadId;
                await Task.Yield();
                resumeThread = Thread.CurrentThread.ManagedThreadId;
            });
            Assert.Equal(contextThread, resumeThread);
            await thread.JoinAsync();
        }

        [Fact]
        public async Task Join_StopsTask()
        {
            var context = new AsyncContextThread();
            var thread = await context.Factory.Run(() => Thread.CurrentThread);
            await context.JoinAsync();
        }

        [Fact]
        public async Task Context_IsCorrectAsyncContext()
        {
            using (var thread = new AsyncContextThread())
            {
                var observedContext = await thread.Factory.Run(() => AsyncContext.Current);
                Assert.Same(observedContext, thread.Context);
            }
        }
    }
}

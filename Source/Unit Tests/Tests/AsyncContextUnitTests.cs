using System;
using NUnit.Framework;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Nito.AsyncEx.Internal;

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
    public class AsyncContextUnitTests
    {
        [Test]
        public void AsyncContext_StaysOnSameThread()
        {
            var testThread = Thread.CurrentThread.ManagedThreadId;
            var contextThread = AsyncContext.Run(() => Thread.CurrentThread.ManagedThreadId);
            Assert.AreEqual(testThread, contextThread);
        }

        [Test]
        public void Run_AsyncVoid_BlocksUntilCompletion()
        {
            bool resumed = false;
            AsyncContext.Run((Action)(async () =>
            {
                await TaskShim.Yield();
                resumed = true;
            }));
            Assert.IsTrue(resumed);
        }

        [Test]
        public void Run_FuncThatCallsAsyncVoid_BlocksUntilCompletion()
        {
            bool resumed = false;
            var result = AsyncContext.Run((Func<int>)(() =>
            {
                Action asyncVoid = async () =>
                {
                    await TaskShim.Yield();
                    resumed = true;
                };
                asyncVoid();
                return 13;
            }));
            Assert.IsTrue(resumed);
            Assert.AreEqual(13, result);
        }

        [Test]
        public void Run_AsyncTask_BlocksUntilCompletion()
        {
            bool resumed = false;
            AsyncContext.Run(async () =>
            {
                await TaskShim.Yield();
                resumed = true;
            });
            Assert.IsTrue(resumed);
        }

        [Test]
        public void Run_AsyncTaskWithResult_BlocksUntilCompletion()
        {
            bool resumed = false;
            var result = AsyncContext.Run(async () =>
            {
                await TaskShim.Yield();
                resumed = true;
                return 17;
            });
            Assert.IsTrue(resumed);
            Assert.AreEqual(17, result);
        }

        [Test]
        public void Current_WithoutAsyncContext_IsNull()
        {
            Assert.IsNull(AsyncContext.Current);
        }

        [Test]
        public void Current_FromAsyncContext_IsAsyncContext()
        {
            AsyncContext observedContext = null;
            var context = new AsyncContext();
            context.Factory.Run(() =>
            {
                observedContext = AsyncContext.Current;
            });

            context.Execute();

            Assert.AreSame(context, observedContext);
        }

        [Test]
        public void SynchronizationContextCurrent_FromAsyncContext_IsAsyncContextSynchronizationContext()
        {
            SynchronizationContext observedContext = null;
            var context = new AsyncContext();
            context.Factory.Run(() =>
            {
                observedContext = SynchronizationContext.Current;
            });

            context.Execute();

            Assert.AreSame(context.SynchronizationContext, observedContext);
        }

        [Test]
        public void TaskSchedulerCurrent_FromAsyncContext_IsAsyncContextTaskScheduler()
        {
            TaskScheduler observedScheduler = null;
            var context = new AsyncContext();
            context.Factory.Run(() =>
            {
                observedScheduler = TaskScheduler.Current;
            });

            context.Execute();

            Assert.AreSame(context.Scheduler, observedScheduler);
        }

        [Test]
        public void TaskScheduler_MaximumConcurrency_IsOne()
        {
            var scheduler = AsyncContext.Run(() => TaskScheduler.Current);
            Assert.AreEqual(1, scheduler.MaximumConcurrencyLevel);
        }

        [Test]
        public void Run_PropagatesException()
        {
            Action test = () => AsyncContext.Run(() => { throw new NotImplementedException(); });
            AssertEx.ThrowsException<NotImplementedException>(test, allowDerivedTypes: false);
        }

        [Test]
        public void Run_Async_PropagatesException()
        {
            Action test = () => AsyncContext.Run(async () => { await TaskShim.Yield(); throw new NotImplementedException(); });
            AssertEx.ThrowsException<NotImplementedException>(test, allowDerivedTypes: false);
        }

        [Test]
        public void SynchronizationContextPost_PropagatesException()
        {
            Action test = () => AsyncContext.Run(async () =>
            {
                SynchronizationContext.Current.Post(_ =>
                {
                    throw new NotImplementedException();
                }, null);
                await TaskShim.Yield();
            });
            AssertEx.ThrowsException<NotImplementedException>(test, allowDerivedTypes: false);
        }

        [Test]
        public void SynchronizationContext_Send_ExecutesSynchronously()
        {
            Test.Async(async () =>
            {
                using (var thread = new AsyncContextThread())
                {
                    var synchronizationContext = await thread.Factory.Run(() => SynchronizationContext.Current);
                    int value = 0;
                    synchronizationContext.Send(_ => { value = 13; }, null);
                    Assert.AreEqual(13, value);
                }
            });
        }

        [Test]
        public void SynchronizationContext_Send_ExecutesInlineIfNecessary()
        {
            Test.Async(async () =>
            {
                using (var thread = new AsyncContextThread())
                {
                    int value = 0;
                    await thread.Factory.Run(() =>
                    {
                        SynchronizationContext.Current.Send(_ => { value = 13; }, null);
                        Assert.AreEqual(13, value);
                    });
                    Assert.AreEqual(13, value);
                }
            });
        }

        [Test]
        public void Task_AfterExecute_NeverRuns()
        {
            int value = 0;
            var context = new AsyncContext();
            context.Factory.Run(() => { value = 1; });
            context.Execute();

            var task = context.Factory.Run(() => { value = 2; });

            task.ContinueWith(_ => { Assert.Fail(); });
            Assert.AreEqual(1, value);
        }

        [Test]
        public void SynchronizationContext_IsEqualToCopyOfItself()
        {
            var synchronizationContext1 = AsyncContext.Run(() => SynchronizationContext.Current);
            var synchronizationContext2 = synchronizationContext1.CreateCopy();
            Assert.AreEqual(synchronizationContext1.GetHashCode(), synchronizationContext2.GetHashCode());
            Assert.IsTrue(synchronizationContext1.Equals(synchronizationContext2));
            Assert.IsFalse(synchronizationContext1.Equals(new SynchronizationContext()));
        }

        [Test]
        public void Id_IsEqualToTaskSchedulerId()
        {
            var context = new AsyncContext();
            Assert.AreEqual(context.Scheduler.Id, context.Id);
        }
    }
}

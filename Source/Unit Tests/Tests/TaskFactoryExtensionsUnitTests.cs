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
    public class TaskFactoryExtensionsUnitTests
    {
        [Test]
        public void Run_SchedulesAction()
        {
            Test.Async(async () =>
            {
                var factory = Task.Factory;
                TaskScheduler result = null;
                var task = factory.Run(() => { result = TaskScheduler.Current; });
                await task;
                Assert.AreSame(TaskScheduler.Default, result);
                TaskShim.AssertDenyChildAttach(task);
            });
        }

        [Test]
        public void Run_SchedulesFunc()
        {
            Test.Async(async () =>
            {
                var factory = Task.Factory;
                TaskScheduler result = null;
                var task = factory.Run(() => { result = TaskScheduler.Current; return 13; });
                await task;
                Assert.AreSame(TaskScheduler.Default, result);
                TaskShim.AssertDenyChildAttach(task);
            });
        }

        [Test]
        public void Run_SchedulesAsyncAction()
        {
            Test.Async(async () =>
            {
                var factory = Task.Factory;
                TaskScheduler result = null;
                var task = factory.Run(async () => { await TaskShim.Delay(100); result = TaskScheduler.Current; });
                await task;
                Assert.AreSame(TaskScheduler.Default, result);
            });
        }

        [Test]
        public void Run_SchedulesAsyncFunc()
        {
            Test.Async(async () =>
            {
                var factory = Task.Factory;
                TaskScheduler result = null;
                var task = factory.Run(async () => { await TaskShim.Delay(100); result = TaskScheduler.Current; return 13; });
                await task;
                Assert.AreSame(TaskScheduler.Default, result);
            });
        }
    }
}

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
    public class TaskFactoryExtensionsUnitTests_NET40
    {
        [Test]
        public void Run_SchedulesAction()
        {
            AsyncContext.Run(async () =>
            {
                var factory = Task.Factory;
                TaskScheduler result = null;
                var task = factory.Run(() => { result = TaskScheduler.Current; });
                await task;
                Assert.AreSame(TaskScheduler.Default, result);
            });
        }

        [Test]
        public void Run_SchedulesFunc()
        {
            AsyncContext.Run(async () =>
            {
                var factory = Task.Factory;
                TaskScheduler result = null;
                var task = factory.Run(() =>
                {
                    result = TaskScheduler.Current;
                    return 13;
                });
                await task;
                Assert.AreSame(TaskScheduler.Default, result);
            });
        }

        [Test]
        public void Run_SchedulesAsyncAction()
        {
            AsyncContext.Run(async () =>
            {
                var factory = Task.Factory;
                TaskScheduler result = null;
                var task = factory.Run(async () =>
                {
                    await AndroidWorkarounds.Delay(100);
                    result = TaskScheduler.Current;
                });
                await task;
                Assert.AreSame(TaskScheduler.Default, result);
            });
        }

        [Test]
        public void Run_SchedulesAsyncFunc()
        {
            AsyncContext.Run(async () =>
            {
                var factory = Task.Factory;
                TaskScheduler result = null;
                var task = factory.Run(async () =>
                {
                    await AndroidWorkarounds.Delay(100);
                    result = TaskScheduler.Current;
                    return 13;
                });
                await task;
                Assert.AreSame(TaskScheduler.Default, result);
            });
        }
    }
}

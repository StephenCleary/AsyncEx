using System;
using NUnit.Framework;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
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
    public class PropertyProgressUnitTests
    {
        [Test]
        public void ProgressReport_UpdatesPropertyValue()
        {
            AsyncContext.Run(async () =>
            {
                var propertyProgress = new PropertyProgress<int>();
                IProgress<int> progress = propertyProgress;
                await TaskShim.Run(() => progress.Report(13));
                await TaskShim.Yield();
                Assert.AreEqual(13, propertyProgress.Progress);

                await TaskShim.Run(() => progress.Report(17));
                await TaskShim.Yield();
                Assert.AreEqual(17, propertyProgress.Progress);
            });
        }

        [Test]
        public void ProgressReport_NotifiesChangeWithCorrectName()
        {
            string propertyName = null;

            AsyncContext.Run(async () =>
            {
                var propertyProgress = new PropertyProgress<int>();
                propertyProgress.PropertyChanged += (_, e) =>
                {
                    propertyName = e.PropertyName;
                };
                IProgress<int> progress = propertyProgress;
                await TaskShim.Run(() => progress.Report(13));
            });

            Assert.AreEqual("Progress", propertyName);
        }

        [Test]
        public void ProgressReport_NotifiesChangeOnCapturedSynchronizationContext()
        {
            Test.Async(async () =>
            {
                SynchronizationContext updateContext = null;
                SynchronizationContext threadContext = null;

                var tcs = new TaskCompletionSource();
                using (var thread = new AsyncContextThread())
                {
                    threadContext = await thread.Factory.Run(() => SynchronizationContext.Current);
                    PropertyProgress<int> propertyProgress = await thread.Factory.Run(() => new PropertyProgress<int>());
                    propertyProgress.PropertyChanged += (_, e) =>
                    {
                        updateContext = SynchronizationContext.Current;
                        tcs.SetResult();
                    };
                    IProgress<int> progress = propertyProgress;
                    progress.Report(13);
                    await tcs.Task;
                }

                Assert.IsNotNull(updateContext);
                Assert.AreEqual(threadContext, updateContext);
            });
        }
    }
}

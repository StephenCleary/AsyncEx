using System;
using NUnit.Framework;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Nito.AsyncEx.Internal;

namespace Tests
{
    [TestFixture]
    public class PropertyProgressUnitTests_NET40
    {
        [Test]
        public void ProgressReport_UpdatesPropertyValue()
        {
            AsyncContext.Run(async () =>
            {
                var propertyProgress = new PropertyProgress<int>();
                IProgress<int> progress = propertyProgress;
                await Task.Run(() => progress.Report(13));
                await Task.Yield();
                Assert.AreEqual(13, propertyProgress.Progress);

                await Task.Run(() => progress.Report(17));
                await Task.Yield();
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
                await Task.Run(() => progress.Report(13));
            });

            Assert.AreEqual("Progress", propertyName);
        }

        [Test]
        public void ProgressReport_NotifiesChangeOnCapturedSynchronizationContext()
        {
            AsyncContext.Run(async () =>
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

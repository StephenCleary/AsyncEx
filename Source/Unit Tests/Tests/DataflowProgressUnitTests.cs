using System;
using NUnit.Framework;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
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
    public class DataflowProgressUnitTests
    {
        [Test]
        public void ProgressReports_AreSentToBlock()
        {
            Test.Async(async () =>
            {
                var block = new BufferBlock<int>();
                IProgress<int> progress = new DataflowProgress<int>(block);
                await TaskShim.Run(() =>
                {
                    progress.Report(13);
                    progress.Report(17);
                });
                var report1 = block.Receive();
                var report2 = block.Receive();
                Assert.AreEqual(13, report1);
                Assert.AreEqual(17, report2);
            });
        }

        [Test]
        public void ObserveTask_Completed_CompletesBlock()
        {
            Test.Async(async () =>
            {
                var block = new BufferBlock<int>();
                var dataflowProgress = new DataflowProgress<int>(block);
                IProgress<int> progress = dataflowProgress;
                var task = TaskShim.Run(() =>
                {
                    progress.Report(13);
                });
                dataflowProgress.ObserveTaskForCompletion(task);
                var report1 = block.Receive();
                Assert.AreEqual(13, report1);
                await block.Completion;
            });
        }

        [Test]
        public void ObserveTask_Faulted_FaultsBlock()
        {
            Test.Async(async () =>
            {
                var block = new BufferBlock<int>();
                var dataflowProgress = new DataflowProgress<int>(block);
                IProgress<int> progress = dataflowProgress;
                var task = TaskShim.Run(() =>
                {
                    throw new NotImplementedException();
                });
                dataflowProgress.ObserveTaskForCompletion(task);
                await AssertEx.ThrowsExceptionAsync<NotImplementedException>(block.Completion, allowDerivedTypes: false);
            });
        }
    }
}

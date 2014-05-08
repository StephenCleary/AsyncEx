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
    public class ProducerProgressUnitTests
    {
        [Test]
        public void ProgressReports_AreSentToCollection()
        {
            Test.Async(async () =>
            {
                var queue = new ConcurrentQueue<int>();
                IProgress<int> progress = new ProducerProgress<int>(queue);
                await TaskShim.Run(() =>
                {
                    progress.Report(13);
                    progress.Report(17);
                });
                Assert.IsTrue(queue.SequenceEqual(new[] { 13, 17 }));
            });
        }
    }
}

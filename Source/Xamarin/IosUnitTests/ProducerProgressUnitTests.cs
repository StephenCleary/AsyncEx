using System;
using NUnit.Framework;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Tests
{
    [TestFixture]
    public class ProducerProgressUnitTests_NET40
    {
        [Test]
        public void ProgressReports_AreSentToCollection()
        {
            AsyncContext.Run(async () =>
            {
                var queue = new ConcurrentQueue<int>();
                IProgress<int> progress = new ProducerProgress<int>(queue);
                await Task.Run(() =>
                {
                    progress.Report(13);
                    progress.Report(17);
                });
                Assert.IsTrue(queue.SequenceEqual(new[] { 13, 17 }));
            });
        }
    }
}

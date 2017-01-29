using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using Nito.AsyncEx.Testing;
using Xunit;

namespace UnitTests
{
    public class CancellationTokenTaskSourceUnitTests
    {
        [Fact]
        public void Constructor_AlreadyCanceledToken_TaskReturnsSynchronouslyCanceledTask()
        {
            var token = new CancellationToken(true);
            using (var source = new CancellationTokenTaskSource<object>(token))
                Assert.True(source.Task.IsCanceled);
        }
    }
}

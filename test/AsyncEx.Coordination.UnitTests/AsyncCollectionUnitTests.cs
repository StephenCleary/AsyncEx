using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AsyncEx.Coordination.UnitTests
{
    public class AsyncCollectionUnitTests
    {
        [Fact]
        public async Task CompleteInOrder()
        {
            var collection = new AsyncCollection<string>();
            var queue = new AsyncInvokeQueue<string>(collection.TakeAsync);

            collection.Add("a");
            var a = queue.InvokeAsync();
            var b = queue.InvokeAsync();
            var c = queue.InvokeAsync();
            collection.Add("b");
            var d = queue.InvokeAsync();
            collection.Add("c");
            collection.Add("d");
            collection.Add("e");
            var e = queue.InvokeAsync();
            Assert.Equal("a", await a);
            Assert.Equal("b", await b);
            Assert.Equal("c", await c);
            Assert.Equal("d", await d);
            Assert.Equal("e", await e);
        }
    }
}
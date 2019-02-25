using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
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

            collection.Add("a");
            var a = collection.TakeAsync();
            var b = collection.TakeAsync();
            var c = collection.TakeAsync();
            collection.Add("b");
            var d = collection.TakeAsync();
            collection.Add("c");
            collection.Add("d");
            collection.Add("e");
            var e = collection.TakeAsync();
            Assert.Equal("a", await a);
            Assert.Equal("b", await b);
            Assert.Equal("c", await c);
            Assert.Equal("d", await d);
            Assert.Equal("e", await e);
        }

        [Fact]
        public async Task BlockingCollectionCompletesInOrder()
        {
            var collection = new BlockingCollection<string>();

            collection.Add("a");
            var a = collection.Take();
            var b = Task.Run(() => collection.Take());
            while (b.Status != TaskStatus.Running) ;
            var c = Task.Run(() => collection.Take());
            while (c.Status != TaskStatus.Running) ;
            collection.Add("b");
            var d = Task.Run(() => collection.Take());
            while (d.Status != TaskStatus.Running) ;
            collection.Add("c");
            collection.Add("d");
            collection.Add("e");
            var e = Task.Run(() => collection.Take());
            while (e.Status != TaskStatus.Running) ;
            Assert.Equal("a", a);
            Assert.Equal("b", await b);
            Assert.Equal("c", await c);
            Assert.Equal("d", await d);
            Assert.Equal("e", await e);
        }
    }
}

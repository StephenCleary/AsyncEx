using System;
using System.Collections.Generic;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    public sealed class BlockingQueue<T> : IDisposable
    {
        public BlockingQueue()
        {
            throw Enlightenment.Exception();
        }

        public bool TryAdd(T item)
        {
            throw Enlightenment.Exception();
        }

        public IEnumerable<T> GetConsumingEnumerable()
        {
            throw Enlightenment.Exception();
        }

        [System.Diagnostics.DebuggerNonUserCode]
        public IEnumerable<T> EnumerateForDebugger()
        {
            throw Enlightenment.Exception();
        }

        public void CompleteAdding()
        {
            throw Enlightenment.Exception();
        }

        public void Dispose()
        {
            throw Enlightenment.Exception();
        }
    }
}

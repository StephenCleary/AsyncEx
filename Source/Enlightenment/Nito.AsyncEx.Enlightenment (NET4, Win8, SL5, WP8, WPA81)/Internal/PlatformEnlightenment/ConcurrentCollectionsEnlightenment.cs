using System;
using System.Collections.Generic;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    public sealed class BlockingQueue<T>
    {
        public BlockingQueue()
        {
            throw Enlightenment.Exception();
        }

        public bool TryAdd(T item)
        {
            throw Enlightenment.Exception();
        }
    }
}

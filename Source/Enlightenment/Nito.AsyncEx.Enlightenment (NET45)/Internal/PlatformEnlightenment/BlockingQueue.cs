using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    public sealed class BlockingQueue<T> : IDisposable
    {
        private readonly BlockingCollection<T> _queue;

        public BlockingQueue()
        {
            _queue = new BlockingCollection<T>();
        }

        public bool TryAdd(T item)
        {
            try
            {
                return _queue.TryAdd(item);
            }
            catch (InvalidOperationException)
            {
                // vexing exception
                return false;
            }
        }

        public IEnumerable<T> GetConsumingEnumerable()
        {
            return _queue.GetConsumingEnumerable();
        }

        [System.Diagnostics.DebuggerNonUserCode]
        public IEnumerable<T> EnumerateForDebugger()
        {
            return _queue;
        }

        public void CompleteAdding()
        {
            _queue.CompleteAdding();
        }

        public void Dispose()
        {
            _queue.Dispose();
        }
    }
}

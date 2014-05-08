using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    partial class EnlightenmentProvider
    {
        private sealed class ConcurrentCollectionsEnlightenment : IConcurrentCollectionsEnlightenment
        {
            IBlockingQueue<T> IConcurrentCollectionsEnlightenment.CreateBlockingQueue<T>()
            {
                return new BlockingQueue<T>();
            }

            private sealed class BlockingQueue<T> : BlockingCollection<T>, IBlockingQueue<T>
            {
                [System.Diagnostics.DebuggerNonUserCode]
                IEnumerable<T> IBlockingQueue<T>.EnumerateForDebugger()
                {
                    return this;
                }

                bool IBlockingQueue<T>.TryAdd(T item)
                {
                    try
                    {
                        return TryAdd(item);
                    }
                    catch (InvalidOperationException)
                    {
                        // vexing exception
                        return false;
                    }
                }
            }
        }
    }
}

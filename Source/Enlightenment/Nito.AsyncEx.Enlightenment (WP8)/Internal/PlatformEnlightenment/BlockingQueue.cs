using System;
using System.Collections.Generic;
using System.Threading;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    public sealed class BlockingQueue<T> : IDisposable
    {
        /// <summary>
        /// The underlying queue.
        /// </summary>
        private readonly Queue<T> _queue;

        /// <summary>
        /// An event that is set whenever the queue is non-empty or if the queue is empty and done.
        /// </summary>
        private readonly ManualResetEvent _nonEmpty;

        /// <summary>
        /// Whether we are done adding tasks to the queue.
        /// </summary>
        private bool _done;

        public BlockingQueue()
        {
            _queue = new Queue<T>();
            _nonEmpty = new ManualResetEvent(false);
        }

        public bool TryAdd(T item)
        {
            lock (_queue)
            {
                if (_done)
                    return false;
                _queue.Enqueue(item);
                _nonEmpty.Set();
                return true;
            }
        }

        public IEnumerable<T> GetConsumingEnumerable()
        {
            while (true)
            {
                _nonEmpty.WaitOne();

                T item;
                lock (_queue)
                {
                    if (_queue.Count == 0)
                        break;
                    item = _queue.Dequeue();
                    if (_queue.Count == 0 && !_done)
                    {
                        _nonEmpty.Reset();
                    }
                }

                yield return item;
            }
        }

        [System.Diagnostics.DebuggerNonUserCode]
        public IEnumerable<T> EnumerateForDebugger()
        {
            bool taken = false;
            try
            {
                Monitor.TryEnter(_queue, ref taken);
                if (taken)
                    return _queue.ToArray();
                else
                    throw new InvalidOperationException("Cannot access BlockingQueue lock.");
            }
            finally
            {
                if (taken)
                    Monitor.Exit(_queue);
            }
        }

        public void CompleteAdding()
        {
            lock (_queue)
            {
                _done = true;
                _nonEmpty.Set();
            }
        }

        public void Dispose()
        {
            _nonEmpty.Dispose();
        }
    }
}

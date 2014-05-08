using System;
using System.Collections.Concurrent;

namespace Nito.AsyncEx
{
    /// <summary>
    /// A progress implementation that sends progress reports to a producer/consumer collection.
    /// </summary>
    /// <typeparam name="T">The type of progress value.</typeparam>
    public sealed class ProducerProgress<T> : IProgress<T>
    {
        /// <summary>
        /// The producer/consumer collection that receives progress reports.
        /// </summary>
        private readonly IProducerConsumerCollection<T> _collection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerProgress&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="collection">The producer/consumer collection that receives progress reports.</param>
        public ProducerProgress(IProducerConsumerCollection<T> collection)
        {
            _collection = collection;
        }

        void IProgress<T>.Report(T value)
        {
            _collection.TryAdd(value);
        }
    }
}

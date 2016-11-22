using Nito.AsyncEx.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// Async compatible Dynamic Dictionary of semaphores
    /// </summary>
    public sealed class AsyncMultiSemaphore<T>
    {
        /// <summary>
        /// Dictionary containing the semaphores
        /// </summary>
        private readonly Dictionary<T, AsyncSemaphore> _dictionary = new Dictionary<T, AsyncSemaphore>();

        /// <summary>
        /// Default count for new Semaphore
        /// </summary>
        private readonly Int32 _count = 0;

        /// <summary>
        /// The semi-unique identifier for this instance. This is 0 if the id has not yet been created.
        /// </summary>
        private int _id;


        /// <summary>
        /// Creates a new async-compatible multisemaphore with the specified initial count.
        /// </summary>
        /// <param name="initialCount"></param>
        public AsyncMultiSemaphore(Int32 initialCount = 1)
        {
            this._count = initialCount;
        }

        /// <summary>
        /// Asynchronously waits for a slot in the semaphore specified by the key to be available.
        /// </summary>
        /// <param name="key">Semaphore Key</param>
        /// <param name="initialCount">The initial count for this semaphore. This must be greater than or equal to zero.</param>
        /// <param name="queue">The wait queue used to manage waiters.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this is already set, then this method will attempt to take the slot immediately (succeeding if a slot is currently available).</param>
        /// <returns></returns>
        public Task WaitAsync(T key, Int32 initialCount, IAsyncWaitQueue<object> queue, CancellationToken cancellationToken)
        {
            lock (this._dictionary)
            {
                if (!_dictionary.ContainsKey(key))
                    this._dictionary.Add(key, new AsyncSemaphore(initialCount, queue));
            }
            return this._dictionary[key].WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously waits for a slot in the semaphore specified by the key to be available.
        /// </summary>
        /// <param name="key">Semaphore Key</param>
        /// <param name="initialCount">The initial count for this semaphore. This must be greater than or equal to zero.</param>
        /// <param name="queue">The wait queue used to manage waiters.</param>
        /// <returns></returns>
        public Task WaitAsync(T key, Int32 initialCount, IAsyncWaitQueue<object> queue)
        {
            return WaitAsync(key, initialCount, queue, CancellationToken.None);

        }

        /// <summary>
        /// Asynchronously waits for a slot in the semaphore specified by the key to be available.
        /// </summary>
        /// <param name="key">Semaphore Key</param>
        /// <param name="initialCount">The initial count for this semaphore. This must be greater than or equal to zero.</param>
        /// <returns></returns>
        public Task WaitAsync(T key, Int32 initialCount)
        {
            return WaitAsync(key, initialCount, new DefaultAsyncWaitQueue<object>());
        }

        /// <summary>
        /// Asynchronously waits for a slot in the semaphore specified by the key to be available.
        /// </summary>
        /// <param name="key">Semaphore Key</param>
        /// <returns></returns>
        public Task WaitAsync(T key)
        {
            return WaitAsync(key, this._count);
        }

        /// <summary>
        /// Releases the semaphore specified by the key.
        /// </summary>
        /// <param name="key">Semaphore Key</param>
        public void Release(T key)
        {
            Release(key, 1);
        }


        /// <summary>
        /// Releases the semaphore specified by the key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="releaseCount"></param>
        public void Release(T key, Int32 releaseCount)
        {
            lock (this._dictionary)
            {
                AsyncSemaphore s = this._dictionary[key];
                if (this._dictionary[key].WaitersCount == 0)
                    this._dictionary.Remove(key);
                s.Release(releaseCount);
            }
        }

        /// <summary>
        /// Gets a semi-unique identifier for this asynchronous semaphore.
        /// </summary>
        public int Id
        {
            get { return IdManager<AsyncMultiSemaphore<T>>.GetId(ref _id); }
        }

    }

}

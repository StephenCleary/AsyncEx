using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// A pair of tasks representing the same operation. One supports synchronous continuations; the other forces asynchronous continuations.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    public interface ITaskSyncAsyncPair<T>
    {
        /// <summary>
        /// The synchronously-completed task. This is appropriate for waiting on in internal code.
        /// </summary>
        Task<T> SynchronousTask { get; }

        /// <summary>
        /// The asynchronously-completed task. This is appropriate for returning to end-user APIs.
        /// </summary>
        Task<T> AsynchronousTask { get; }
    }
}
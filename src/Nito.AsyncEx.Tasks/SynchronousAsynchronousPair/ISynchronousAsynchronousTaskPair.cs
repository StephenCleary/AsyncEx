using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Nito.AsyncEx.SynchronousAsynchronousPair
{
    /// <summary>
    /// A pair of tasks representing the same operation. One supports synchronous continuations; the other forces asynchronous continuations.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    [AsyncMethodBuilder(typeof(CompilerSupport.SyncAsyncTaskPairAsyncMethodBuilder<>))]
    public interface ISynchronousAsynchronousTaskPair<T>
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

    /// <summary>
    /// Provides extensions for task pairs.
    /// </summary>
    public static class TaskSyncAsyncPairExtensions
    {
        /// <summary>
        /// Allows task pairs to be awaited. When a task pair is awaited, it uses the synchronous task for continuations.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="this">The task pair.</param>
        /// <returns>The task awaiter for the synchronous task.</returns>
        public static TaskAwaiter<T> GetAwaiter<T>(this ISynchronousAsynchronousTaskPair<T> @this) => @this.SynchronousTask.GetAwaiter();

        /// <summary>
        /// Allows task pairs to be awaited without context. When a task pair is awaited, it uses the synchronous task for continuations.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="this">The task pair.</param>
        /// <param name="continueOnCapturedContext">Whether the continuation should run on a captured context.</param>
        /// <returns>The configured task awaiter for the synchronous task.</returns>
        public static ConfiguredTaskAwaitable<T> ConfigureAwait<T>(this ISynchronousAsynchronousTaskPair<T> @this, bool continueOnCapturedContext) =>
            @this.SynchronousTask.ConfigureAwait(continueOnCapturedContext);
    }
}
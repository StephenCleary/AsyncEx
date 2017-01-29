using System;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// Provides extension methods for task factories.
    /// </summary>
    public static class TaskFactoryExtensions
    {
        /// <summary>
        /// Queues work to the task factory and returns a <see cref="Task"/> representing that work. If the task factory does not specify a task scheduler, the thread pool task scheduler is used.
        /// </summary>
        /// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
        /// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
        /// <returns>The started task.</returns>
        public static Task Run(this TaskFactory @this, Action action)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return @this.StartNew(action, @this.CancellationToken, @this.CreationOptions | TaskCreationOptions.DenyChildAttach, @this.Scheduler ?? TaskScheduler.Default);
        }

        /// <summary>
        /// Queues work to the task factory and returns a <see cref="Task{TResult}"/> representing that work. If the task factory does not specify a task scheduler, the thread pool task scheduler is used.
        /// </summary>
        /// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
        /// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
        /// <returns>The started task.</returns>
        public static Task<TResult> Run<TResult>(this TaskFactory @this, Func<TResult> action)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return @this.StartNew(action, @this.CancellationToken, @this.CreationOptions | TaskCreationOptions.DenyChildAttach, @this.Scheduler ?? TaskScheduler.Default);
        }

        /// <summary>
        /// Queues work to the task factory and returns a proxy <see cref="Task"/> representing that work. If the task factory does not specify a task scheduler, the thread pool task scheduler is used.
        /// </summary>
        /// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
        /// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
        /// <returns>The started task.</returns>
        public static Task Run(this TaskFactory @this, Func<Task> action)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return @this.StartNew(action, @this.CancellationToken, @this.CreationOptions | TaskCreationOptions.DenyChildAttach, @this.Scheduler ?? TaskScheduler.Default).Unwrap();
        }

        /// <summary>
        /// Queues work to the task factory and returns a proxy <see cref="Task{TResult}"/> representing that work. If the task factory does not specify a task scheduler, the thread pool task scheduler is used.
        /// </summary>
        /// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
        /// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
        /// <returns>The started task.</returns>
        public static Task<TResult> Run<TResult>(this TaskFactory @this, Func<Task<TResult>> action)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return @this.StartNew(action, @this.CancellationToken, @this.CreationOptions | TaskCreationOptions.DenyChildAttach, @this.Scheduler ?? TaskScheduler.Default).Unwrap();
        }
    }
}

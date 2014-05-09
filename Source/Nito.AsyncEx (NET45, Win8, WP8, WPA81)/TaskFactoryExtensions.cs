using Nito.AsyncEx.Internal.PlatformEnlightenment;
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
        /// Queues work to the task factory and returns a <see cref="Task"/> representing that work.
        /// </summary>
        /// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
        /// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
        /// <returns>The started task.</returns>
        public static Task Run(this TaskFactory @this, Action action)
        {
            return @this.StartNew(action, @this.CancellationToken, AsyncEnlightenment.AddDenyChildAttach(@this.CreationOptions), @this.Scheduler ?? TaskScheduler.Default);
        }

        /// <summary>
        /// Queues work to the task factory and returns a <see cref="Task{TResult}"/> representing that work.
        /// </summary>
        /// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
        /// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
        /// <returns>The started task.</returns>
        public static Task<TResult> Run<TResult>(this TaskFactory @this, Func<TResult> action)
        {
            return @this.StartNew(action, @this.CancellationToken, AsyncEnlightenment.AddDenyChildAttach(@this.CreationOptions), @this.Scheduler ?? TaskScheduler.Default);
        }

        /// <summary>
        /// Queues work to the task factory and returns a proxy <see cref="Task"/> representing that work.
        /// </summary>
        /// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
        /// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
        /// <returns>The started task.</returns>
        public static Task Run(this TaskFactory @this, Func<Task> action)
        {
            return @this.StartNew(action, @this.CancellationToken, AsyncEnlightenment.AddDenyChildAttach(@this.CreationOptions), @this.Scheduler ?? TaskScheduler.Default).Unwrap();
        }

        /// <summary>
        /// Queues work to the task factory and returns a proxy <see cref="Task{TResult}"/> representing that work.
        /// </summary>
        /// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
        /// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
        /// <returns>The started task.</returns>
        public static Task<TResult> Run<TResult>(this TaskFactory @this, Func<Task<TResult>> action)
        {
            return @this.StartNew(action, @this.CancellationToken, AsyncEnlightenment.AddDenyChildAttach(@this.CreationOptions), @this.Scheduler ?? TaskScheduler.Default).Unwrap();
        }
    }
}

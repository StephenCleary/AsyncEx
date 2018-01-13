using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx.SynchronousAsynchronousPair
{
    /// <summary>
    /// An asynchronous <see cref="TaskCompletionSource{TResult}"/> that is linked to a synchronous task.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    public sealed class LinkedSynchronousAsynchronousTaskPair<T>: ISynchronousAsynchronousTaskPair<T>
    {
        private readonly TaskCompletionSource<T> _asynchronous;
        private readonly Task<bool> _linkingContinuation;

        /// <summary>
        /// Creates a new TCS linked to the specified task.
        /// </summary>
        /// <param name="synchronousTask">The synchronous task to link to.</param>
        public LinkedSynchronousAsynchronousTaskPair(Task<T> synchronousTask)
        {
            SynchronousTask = synchronousTask;
            _asynchronous = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            _linkingContinuation = _asynchronous.LinkCompletionFromTask(synchronousTask);
        }

        /// <summary>
        /// The synchronously-completed task. This is appropriate for waiting on in internal code.
        /// </summary>
        public Task<T> SynchronousTask { get; }

        /// <summary>
        /// The asynchronously-completed task. This is appropriate for returning to end-user APIs.
        /// </summary>
        public Task<T> AsynchronousTask => _asynchronous.Task;
    }
}

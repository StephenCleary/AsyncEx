using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx.Internal
{
    /// <summary>
    /// A tasklike whose continuations always run on the dedicated thread.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    internal sealed class DedicatedTask<T>
    {
        private readonly Task<T> _task;

        /// <summary>
        /// Creates a new dedicated task.
        /// </summary>
        public DedicatedTask(Task<T> task)
        {
            _task = task;
        }

        /// <summary>
        /// Gets an asynchronously-completed task. This is appropriate for returning to end-user APIs.
        /// </summary>
        public Task<T> GetAsynchronousTask()
        {
            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            tcs.LinkCompletionFromTask(_task, DedicatedThread.SynchronizationContext);
            return tcs.Task;
        }

        /// <summary>
        /// Creates and completes a TCS pair from a result value.
        /// </summary>
        /// <param name="result">The result value.</param>
        /// <returns>The completed TCS pair.</returns>
        public static DedicatedTask<T> FromResult(T result) => new DedicatedTask<T>(Task.FromResult(result));

        /// <summary>
        /// Creates and completes a TCS pair from an exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>The completed TCS pair.</returns>
        public static DedicatedTask<T> FromException(Exception exception) => new DedicatedTask<T>(Task.FromException<T>(exception));

        /// <summary>
        /// Creates and completes a TCS pair from a cancellation token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The completed TCS pair.</returns>
        public static DedicatedTask<T> FromCanceled(CancellationToken cancellationToken) => new DedicatedTask<T>(Task.FromCanceled<T>(cancellationToken));

        public Awaiter GetAwaiter() => new Awaiter(_task);

        public sealed class Awaiter : INotifyCompletion
        {
            private readonly TaskAwaiter<T> _awaiter;

            public Awaiter(Task<T> task)
            {
                _awaiter = task.GetAwaiter();
            }

            public bool IsCompleted => _awaiter.IsCompleted;

            public void OnCompleted(Action continuation) => DedicatedThread.ApplyContext(() => _awaiter.OnCompleted(continuation));

            public T GetResult() => _awaiter.GetResult();
        }
    }
}

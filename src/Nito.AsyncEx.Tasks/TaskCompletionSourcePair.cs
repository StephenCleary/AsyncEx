using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// A synchronous <see cref="TaskCompletionSource{TResult}"/> paired with an asynchronous <see cref="TaskCompletionSource{TResult}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    public sealed class TaskCompletionSourcePair<T>
    {
        private readonly TaskCompletionSource<T> _synchronous;
        private readonly TaskCompletionSource<T> _asynchronous;
        private readonly Task<bool> _linkingContinuation;

        public TaskCompletionSourcePair()
        {
            _synchronous = new TaskCompletionSource<T>();
            _asynchronous = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            _linkingContinuation = _asynchronous.LinkCompletionFromTask(_synchronous.Task);
        }

        public Task<T> SynchronousTask => _synchronous.Task;
        public Task<T> AsynchronousTask => _asynchronous.Task;
        public bool TrySetResult(T result) => _synchronous.TrySetResult(result);
        public bool TrySetException(Exception exception) => _synchronous.TrySetException(exception);
        public bool TrySetCancel(CancellationToken cancellationToken) => _synchronous.TrySetCanceled(cancellationToken);
    }
}

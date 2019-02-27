using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// An invocation queue for an asynchronous operation. The operation is only invoked once at a time, with completion results applied strictly in order of request.
    /// </summary>
    /// <typeparam name="T">The type returned by the operation.</typeparam>
    public sealed class AsyncInvokeQueue<T>
    {
        private readonly Func<Task<T>> _func;
        private readonly IAsyncWaitQueue<T> _queue = new DefaultAsyncWaitQueue<T>();
        private readonly object _mutex = new object();
        private bool _executing;

        /// <summary>
        /// Creates a new invocation queue for the specified asynchronous operation.
        /// </summary>
        /// <param name="func">The asynchronous operation.</param>
        public AsyncInvokeQueue(Func<Task<T>> func) => _func = func;

        /// <summary>
        /// Invokes the asynchronous operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to cancel the invocation. This is <i>not</i> passed to the asynchronous operation!</param>
        public Task<T> InvokeAsync(CancellationToken cancellationToken)
        {
            lock (_mutex)
            {
                var result = _queue.Enqueue(_mutex, cancellationToken);
                if (!_executing)
                    Go();
                return result;
            }
        }

        private static Task<T> InvokeFunc(Func<Task<T>> func)
        {
            try
            {
                return func();
            }
            catch (OperationCanceledException ex)
            {
                var token = ex.CancellationToken;
                return Task.FromCanceled<T>(token.IsCancellationRequested ? token : new CancellationToken(canceled: true));
            }
            catch (Exception ex)
            {
                return Task.FromException<T>(ex);
            }
        }

        private async void Go()
        {
            _executing = true;
            var task = InvokeFunc(_func);
            T result = default(T);
            CancellationToken? cancellationToken = null;
            Exception exception = null;
            try
            {
                result = await _func();
            }
            catch (OperationCanceledException ex)
            {
                cancellationToken = ex.CancellationToken.IsCancellationRequested ? ex.CancellationToken : new CancellationToken(canceled: true);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                lock (_mutex)
                {
                    if (!_queue.IsEmpty)
                    {
                        if (exception != null)
                            _queue.DequeueException(exception);
                        else if (cancellationToken != null)
                            _queue.DequeueCancel(cancellationToken.Value);
                        else
                            _queue.Dequeue(result);

                        Go();
                    }
                    else
                    {
                        _executing = false;
                    }
                }
            }
        }
    }
}

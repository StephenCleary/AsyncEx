using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// An async-compatible monitor.
    /// </summary>
    [DebuggerDisplay("Id = {Id}, ConditionVariableId = {_conditionVariable.Id}")]
    public sealed class AsyncMonitor
    {
        /// <summary>
        /// The lock.
        /// </summary>
        private readonly AsyncLock _asyncLock;

        /// <summary>
        /// The condition variable.
        /// </summary>
        private readonly AsyncConditionVariable _conditionVariable;

        /// <summary>
        /// Constructs a new monitor.
        /// </summary>
        /// <param name="lockQueue">The wait queue used to manage waiters for the lock. This may be <c>null</c> to use a default (FIFO) queue.</param>
        /// <param name="conditionVariableQueue">The wait queue used to manage waiters for the signal. This may be <c>null</c> to use a default (FIFO) queue.</param>
        internal AsyncMonitor(IAsyncWaitQueue<IDisposable>? lockQueue, IAsyncWaitQueue<object>? conditionVariableQueue)
        {
            _asyncLock = new AsyncLock(lockQueue);
            _conditionVariable = new AsyncConditionVariable(_asyncLock, conditionVariableQueue);
        }

        /// <summary>
        /// Constructs a new monitor.
        /// </summary>
        public AsyncMonitor()
            : this(null, null)
        {
        }

        /// <summary>
        /// Gets a semi-unique identifier for this monitor.
        /// </summary>
        public int Id
        {
            get { return _asyncLock.Id; }
        }

        /// <summary>
        /// Asynchronously enters the monitor. Returns a disposable that leaves the monitor when disposed.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the enter. If this is already set, then this method will attempt to enter the monitor immediately (succeeding if the monitor is currently available).</param>
        /// <returns>A disposable that leaves the monitor when disposed.</returns>
        public AwaitableDisposable<IDisposable> EnterAsync(CancellationToken cancellationToken)
        {
            return _asyncLock.LockAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously enters the monitor. Returns a disposable that leaves the monitor when disposed.
        /// </summary>
        /// <returns>A disposable that leaves the monitor when disposed.</returns>
        public AwaitableDisposable<IDisposable> EnterAsync()
        {
            return EnterAsync(CancellationToken.None);
        }

        /// <summary>
        /// Synchronously enters the monitor. Returns a disposable that leaves the monitor when disposed. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the enter. If this is already set, then this method will attempt to enter the monitor immediately (succeeding if the monitor is currently available).</param>
        public IDisposable Enter(CancellationToken cancellationToken)
        {
            return _asyncLock.Lock(cancellationToken);
        }

        /// <summary>
        /// Asynchronously enters the monitor. Returns a disposable that leaves the monitor when disposed. This method may block the calling thread.
        /// </summary>
        public IDisposable Enter()
        {
            return Enter(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously waits for a pulse signal on this monitor. The monitor MUST already be entered when calling this method, and it will still be entered when this method returns, even if the method is cancelled. This method internally will leave the monitor while waiting for a notification.
        /// </summary>
        /// <param name="cancellationToken">The cancellation signal used to cancel this wait.</param>
        public Task WaitAsync(CancellationToken cancellationToken)
        {
            return _conditionVariable.WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously waits for a pulse signal on this monitor. The monitor MUST already be entered when calling this method, and it will still be entered when this method returns. This method internally will leave the monitor while waiting for a notification.
        /// </summary>
        public Task WaitAsync()
        {
            return WaitAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously waits for a pulse signal on this monitor. This method may block the calling thread. The monitor MUST already be entered when calling this method, and it will still be entered when this method returns, even if the method is cancelled. This method internally will leave the monitor while waiting for a notification.
        /// </summary>
        /// <param name="cancellationToken">The cancellation signal used to cancel this wait.</param>
        public void Wait(CancellationToken cancellationToken)
        {
            _conditionVariable.Wait(cancellationToken);
        }

        /// <summary>
        /// Asynchronously waits for a pulse signal on this monitor. This method may block the calling thread. The monitor MUST already be entered when calling this method, and it will still be entered when this method returns. This method internally will leave the monitor while waiting for a notification.
        /// </summary>
        public void Wait()
        {
            Wait(CancellationToken.None);
        }

        /// <summary>
        /// Sends a signal to a single task waiting on this monitor. The monitor MUST already be entered when calling this method, and it will still be entered when this method returns.
        /// </summary>
        public void Pulse()
        {
            _conditionVariable.Notify();
        }

        /// <summary>
        /// Sends a signal to all tasks waiting on this monitor. The monitor MUST already be entered when calling this method, and it will still be entered when this method returns.
        /// </summary>
        public void PulseAll()
        {
            _conditionVariable.NotifyAll();
        }
    }
}

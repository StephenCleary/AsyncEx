using System.Threading.Tasks;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    /// <summary>
    /// The type of reader/writer lock being acquired or released.
    /// </summary>
    public enum AsyncReaderWriterLockLockType
    {
        /// <summary>
        /// A reader lock.
        /// </summary>
        Reader,

        /// <summary>
        /// An upgradeable reader lock.
        /// </summary>
        UpgradeableReader,

        /// <summary>
        /// A writer lock.
        /// </summary>
        Writer,

        /// <summary>
        /// An upgradeable reader lock upgrading to a writer lock.
        /// </summary>
        UpgradingToWriter,
    }

    /// <summary>
    /// Provides detailed trace information.
    /// </summary>
    public interface ITraceEnlightenment
    {
        /// <summary>
        /// Fatal error: a task could not be queued to a context.
        /// </summary>
        void AsyncContext_TaskQueueFailed(AsyncContext context, Task task);

        /// <summary>
        /// Detailed log: a task has been scheduled to a context.
        /// </summary>
        void AsyncContext_TaskScheduled(AsyncContext context, Task task);

        /// <summary>
        /// Detailed log: the operation count of a context has been explicitly incremented.
        /// </summary>
        void AsyncContext_ExplicitOperationIncrement(AsyncContext context);

        /// <summary>
        /// Detailed log: the operation count of a context has been explicitly decremented.
        /// </summary>
        void AsyncContext_ExplicitOperationDecrement(AsyncContext context);

        /// <summary>
        /// Detailed log: the operation count of a context has been incremented.
        /// </summary>
        void AsyncContext_OperationIncrement(AsyncContext context, int newOperationCount);

        /// <summary>
        /// Detailed log: the operation count of a context has been decremented.
        /// </summary>
        void AsyncContext_OperationDecrement(AsyncContext context, int newOperationCount);

        /// <summary>
        /// Detailed log: a task has started waiting on a lock.
        /// </summary>
        void AsyncLock_TrackLock(AsyncLock asyncLock, Task lockTask);

        /// <summary>
        /// Detailed log: a task has unlocked a lock.
        /// </summary>
        void AsyncLock_Unlocked(AsyncLock asyncLock);

        /// <summary>
        /// Detailed log: a manual-reset event has been set.
        /// </summary>
        void AsyncManualResetEvent_Set(AsyncManualResetEvent manualResetEvent, Task waitTask);

        /// <summary>
        /// Detailed log: a manual-reset event has been reset.
        /// </summary>
        void AsyncManualResetEvent_Reset(AsyncManualResetEvent manualResetEvent, Task waitTask);

        /// <summary>
        /// Detailed log: a task is waiting on a manual-reset event.
        /// </summary>
        void AsyncManualResetEvent_Wait(AsyncManualResetEvent manualResetEvent, Task waitTask);

        /// <summary>
        /// Detailed log: asynchronous lazy initialization has started.
        /// </summary>
        void AsyncLazy_Started<T>(AsyncLazy<T> lazy, Task initializationTask);

        /// <summary>
        /// Detailed log: a task has notified another single task on a condition variable.
        /// </summary>
        void AsyncConditionVariable_NotifyOne(AsyncConditionVariable conditionVariable, AsyncLock asyncLock);

        /// <summary>
        /// Detailed log: a task has notified all other tasks on a condition variable.
        /// </summary>
        void AsyncConditionVariable_NotifyAll(AsyncConditionVariable conditionVariable, AsyncLock asyncLock);

        /// <summary>
        /// Detailed log: a task has started waiting on a condition variable.
        /// </summary>
        void AsyncConditionVariable_TrackWait(AsyncConditionVariable conditionVariable, AsyncLock asyncLock, Task notifyTask, Task waitTask);

        /// <summary>
        /// Detailed log: a monitor has been created.
        /// </summary>
        void AsyncMonitor_Created(AsyncLock asyncLock, AsyncConditionVariable conditionVariable);

        /// <summary>
        /// Detailed log: a task has started waiting on an auto-reset event.
        /// </summary>
        void AsyncAutoResetEvent_TrackWait(AsyncAutoResetEvent autoResetEvent, Task waitTask);

        /// <summary>
        /// Detailed log: an auto-reset event has been set.
        /// </summary>
        void AsyncAutoResetEvent_Set(AsyncAutoResetEvent autoResetEvent);

        /// <summary>
        /// Detailed log: a countdown event has changed.
        /// </summary>
        void AsyncCountdownEvent_CountChanged(AsyncCountdownEvent countdownEvent, int oldCount, int newCount);

        /// <summary>
        /// Detailed log: a barrier has finished its phase.
        /// </summary>
        void AsyncBarrier_PhaseChanged(AsyncBarrier barrier, long phase, int participants, Task waitTask);

        /// <summary>
        /// Detailed log: the count of a barrier has changed.
        /// </summary>
        void AsyncBarrier_CountChanged(AsyncBarrier barrier, long phase, int count);

        /// <summary>
        /// Detailed log: the number of participants in a barrier has changed.
        /// </summary>
        void AsyncBarrier_ParticipantsChanged(AsyncBarrier barrier, long phase, int participants);

        /// <summary>
        /// Detailed log: the count of a semaphore has changed.
        /// </summary>
        void AsyncSemaphore_CountChanged(AsyncSemaphore semaphore, int count);

        /// <summary>
        /// Detailed log: a task has started waiting on a semaphore.
        /// </summary>
        void AsyncSemaphore_TrackWait(AsyncSemaphore semaphore, Task waitTask);

        /// <summary>
        /// Detailed log: a task is waiting on a reader-writer lock.
        /// </summary>
        void AsyncReaderWriterLock_TrackLock(AsyncReaderWriterLock rwLock, AsyncReaderWriterLockLockType lockType, Task waitTask);

        /// <summary>
        /// Detailed log: a reader-writer lock has been released.
        /// </summary>
        void AsyncReaderWriterLock_LockReleased(AsyncReaderWriterLock rwLock, AsyncReaderWriterLockLockType lockType);
    }
}

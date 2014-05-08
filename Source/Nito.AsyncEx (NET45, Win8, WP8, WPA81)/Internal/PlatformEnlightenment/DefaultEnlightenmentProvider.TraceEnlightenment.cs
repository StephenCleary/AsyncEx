using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    partial class DefaultEnlightenmentProvider
    {
        /// <summary>
        /// The default trace enlightenment.
        /// </summary>
        public sealed class TraceEnlightenment : ITraceEnlightenment
        {
            void ITraceEnlightenment.AsyncContext_TaskQueueFailed(AsyncContext context, Task task) { }
            void ITraceEnlightenment.AsyncContext_TaskScheduled(AsyncContext context, Task task) { }
            void ITraceEnlightenment.AsyncContext_ExplicitOperationIncrement(AsyncContext context) { }
            void ITraceEnlightenment.AsyncContext_ExplicitOperationDecrement(AsyncContext context) { }
            void ITraceEnlightenment.AsyncContext_OperationIncrement(AsyncContext context, int newOperationCount) { }
            void ITraceEnlightenment.AsyncContext_OperationDecrement(AsyncContext context, int newOperationCount) { }
            void ITraceEnlightenment.AsyncLock_TrackLock(AsyncLock asyncLock, Task lockTask) { }
            void ITraceEnlightenment.AsyncLock_Unlocked(AsyncLock asyncLock) { }
            void ITraceEnlightenment.AsyncManualResetEvent_Set(AsyncManualResetEvent manualResetEvent, Task waitTask) { }
            void ITraceEnlightenment.AsyncManualResetEvent_Reset(AsyncManualResetEvent manualResetEvent, Task waitTask) { }
            void ITraceEnlightenment.AsyncManualResetEvent_Wait(AsyncManualResetEvent manualResetEvent, Task waitTask) { }
            void ITraceEnlightenment.AsyncLazy_Started<T>(AsyncLazy<T> lazy, Task initializationTask) { }
            void ITraceEnlightenment.AsyncConditionVariable_NotifyOne(AsyncConditionVariable conditionVariable, AsyncLock asyncLock) { }
            void ITraceEnlightenment.AsyncConditionVariable_NotifyAll(AsyncConditionVariable conditionVariable, AsyncLock asyncLock) { }
            void ITraceEnlightenment.AsyncConditionVariable_TrackWait(AsyncConditionVariable conditionVariable, AsyncLock asyncLock, Task notifyTask, Task waitTask) { }
            void ITraceEnlightenment.AsyncMonitor_Created(AsyncLock asyncLock, AsyncConditionVariable conditionVariable) { }
            void ITraceEnlightenment.AsyncAutoResetEvent_TrackWait(AsyncAutoResetEvent autoResetEvent, Task waitTask) { }
            void ITraceEnlightenment.AsyncAutoResetEvent_Set(AsyncAutoResetEvent autoResetEvent) { }
            void ITraceEnlightenment.AsyncCountdownEvent_CountChanged(AsyncCountdownEvent countdownEvent, int oldCount, int newCount) { }
            void ITraceEnlightenment.AsyncBarrier_PhaseChanged(AsyncBarrier barrier, long phase, int participants, Task waitTask) { }
            void ITraceEnlightenment.AsyncBarrier_CountChanged(AsyncBarrier barrier, long phase, int count) { }
            void ITraceEnlightenment.AsyncBarrier_ParticipantsChanged(AsyncBarrier barrier, long phase, int participants) { }
            void ITraceEnlightenment.AsyncSemaphore_CountChanged(AsyncSemaphore semaphore, int count) { }
            void ITraceEnlightenment.AsyncSemaphore_TrackWait(AsyncSemaphore semaphore, Task waitTask) { }
            void ITraceEnlightenment.AsyncReaderWriterLock_TrackLock(AsyncReaderWriterLock rwLock, AsyncReaderWriterLockLockType lockType, Task waitTask) { }
            void ITraceEnlightenment.AsyncReaderWriterLock_LockReleased(AsyncReaderWriterLock rwLock, AsyncReaderWriterLockLockType lockType) { }
        }
    }
}

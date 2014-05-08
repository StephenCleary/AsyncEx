using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    partial class EnlightenmentProvider
    {
        [EventSource(Name = "Nito.AsyncEx", Guid = "{2CC34E38-26FB-4A31-8D9C-AC9C7994A728}")]
        private sealed class TraceEnlightenment : EventSource, ITraceEnlightenment
        {
            public class Keywords
            {
                public const EventKeywords AsyncContext = (EventKeywords)0x1;
                public const EventKeywords AsyncSynchronization = (EventKeywords)0x2;
                public const EventKeywords AsyncUtilities = (EventKeywords)0x4;
                public const EventKeywords Dataflow = (EventKeywords)0x8;
            }

            [Event(1, Level = EventLevel.Critical, Keywords = Keywords.AsyncContext, Message = "Could not queue task {2} to AsyncContext {0} because the context has already completed.")]
            public void TaskQueueFailed(int AsyncContextTaskSchedulerID, int OriginatingTaskID, int TaskID, int TaskCreationOptions)
            {
                WriteEvent(1, AsyncContextTaskSchedulerID, OriginatingTaskID, TaskID, TaskCreationOptions);
                if (Debugger.IsAttached)
                    Debugger.Break();
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncContext_TaskQueueFailed(AsyncContext context, Task task)
            {
                if (IsEnabled(EventLevel.Critical, Keywords.AsyncContext))
                    TaskQueueFailed(context.Id, Task.CurrentId ?? 0, task.Id, (int)task.CreationOptions);
            }

            [Event(2, Level = EventLevel.Verbose, Keywords = Keywords.AsyncContext, Message = "Task {2} scheduled to AsyncContext {0}.")]
            public void TaskScheduled(int AsyncContextTaskSchedulerID, int OriginatingTaskID, int TaskID, int TaskCreationOptions)
            {
                WriteEvent(2, AsyncContextTaskSchedulerID, OriginatingTaskID, TaskID, TaskCreationOptions);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncContext_TaskScheduled(AsyncContext context, Task task)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncContext))
                    TaskScheduled(context.Id, Task.CurrentId ?? 0, task.Id, (int)task.CreationOptions);
            }

            [Event(3, Level = EventLevel.Verbose, Keywords = Keywords.AsyncContext, Message = "Asynchronous operation started on AsyncContext {0}.")]
            public void ExplicitOperationIncrement(int AsyncContextTaskSchedulerID, int OriginatingTaskID)
            {
                WriteEvent(3, AsyncContextTaskSchedulerID, OriginatingTaskID);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncContext_ExplicitOperationIncrement(AsyncContext context)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncContext))
                    ExplicitOperationIncrement(context.Id, Task.CurrentId ?? 0);
            }

            [Event(4, Level = EventLevel.Verbose, Keywords = Keywords.AsyncContext, Message = "Asynchronous operation completed on AsyncContext {0}.")]
            public void ExplicitOperationDecrement(int AsyncContextTaskSchedulerID, int OriginatingTaskID)
            {
                WriteEvent(4, AsyncContextTaskSchedulerID, OriginatingTaskID);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncContext_ExplicitOperationDecrement(AsyncContext context)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncContext))
                    ExplicitOperationDecrement(context.Id, Task.CurrentId ?? 0);
            }

            [Event(5, Level = EventLevel.Verbose, Keywords = Keywords.AsyncContext, Message = "AsyncContext {0} count incremented to {2}.")]
            public void OperationIncrement(int AsyncContextTaskSchedulerID, int OriginatingTaskID, int NewOperationCount)
            {
                WriteEvent(5, AsyncContextTaskSchedulerID, OriginatingTaskID, NewOperationCount);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncContext_OperationIncrement(AsyncContext context, int newOperationCount)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncContext))
                    OperationIncrement(context.Id, Task.CurrentId ?? 0, newOperationCount);
            }

            [Event(6, Level = EventLevel.Verbose, Keywords = Keywords.AsyncContext, Message = "AsyncContext {0} count decremented to {2}.")]
            public void OperationDecrement(int AsyncContextTaskSchedulerID, int OriginatingTaskID, int NewOperationCount)
            {
                WriteEvent(6, AsyncContextTaskSchedulerID, OriginatingTaskID);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncContext_OperationDecrement(AsyncContext context, int newOperationCount)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncContext))
                    OperationDecrement(context.Id, Task.CurrentId ?? 0, newOperationCount);
            }

            [Event(7, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Opcode = EventOpcode.Start, Message = "AsyncLock {0} starting Lock task {2}, synchronous = {3}.")]
            public void AsyncLockBegin(int AsyncLockID, int OriginatingTaskID, int LockTaskID, bool IsSynchronous)
            {
                WriteEvent(7, AsyncLockID, OriginatingTaskID, LockTaskID, IsSynchronous);
            }

            [Event(8, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Opcode = EventOpcode.Stop, Message = "AsyncLock {0} completing Lock task {2}, canceled = {3}.")]
            public void AsyncLockEnd(int AsyncLockID, int OriginatingTaskID, int LockTaskID, bool IsCanceled)
            {
                WriteEvent(8, AsyncLockID, OriginatingTaskID, LockTaskID, IsCanceled);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncLock_TrackLock(AsyncLock asyncLock, Task lockTask)
            {
                if (!IsEnabled(EventLevel.Verbose, Keywords.AsyncSynchronization))
                    return;
                var asyncLockId = asyncLock.Id;
                var lockTaskId = lockTask.Id;
                AsyncLockBegin(asyncLockId, Task.CurrentId ?? 0, lockTaskId, lockTask.IsCompleted);
                lockTask.ContinueWith(t => AsyncLockEnd(asyncLockId, Task.CurrentId ?? 0, lockTaskId, t.IsCanceled), TaskContinuationOptions.ExecuteSynchronously);
            }

            [Event(9, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Message = "AsyncLock {0} unlocked.")]
            public void AsyncLockUnlocked(int AsyncLockID, int OriginatingTaskID)
            {
                WriteEvent(9, AsyncLockID, OriginatingTaskID);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncLock_Unlocked(AsyncLock asyncLock)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncSynchronization))
                    AsyncLockUnlocked(asyncLock.Id, Task.CurrentId ?? 0);
            }

            [Event(10, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Message = "AsyncManualResetEvent {0} is set; completing wait task {2}.")]
            public void AsyncManualResetEventSet(int AsyncManualResetEventID, int OriginatingTaskID, int WaitTaskID)
            {
                WriteEvent(10, AsyncManualResetEventID, OriginatingTaskID, WaitTaskID);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncManualResetEvent_Set(AsyncManualResetEvent manualResetEvent, Task waitTask)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncSynchronization))
                    AsyncManualResetEventSet(manualResetEvent.Id, Task.CurrentId ?? 0, waitTask.Id);
            }

            [Event(11, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Message = "AsyncManualResetEvent {0} is reset; new wait task is {2}.")]
            public void AsyncManualResetEventReset(int AsyncManualResetEventID, int OriginatingTaskID, int WaitTaskID)
            {
                WriteEvent(11, AsyncManualResetEventID, OriginatingTaskID, WaitTaskID);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncManualResetEvent_Reset(AsyncManualResetEvent manualResetEvent, Task waitTask)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncSynchronization))
                    AsyncManualResetEventReset(manualResetEvent.Id, Task.CurrentId ?? 0, waitTask.Id);
            }

            [Event(12, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Message = "AsyncManualResetEvent {0} returning wait task {2}.")]
            public void AsyncManualResetEventWait(int AsyncManualResetEventID, int OriginatingTaskID, int WaitTaskID)
            {
                WriteEvent(12, AsyncManualResetEventID, OriginatingTaskID, WaitTaskID);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncManualResetEvent_Wait(AsyncManualResetEvent manualResetEvent, Task waitTask)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncSynchronization))
                    AsyncManualResetEventWait(manualResetEvent.Id, Task.CurrentId ?? 0, waitTask.Id);
            }

            [Event(13, Level = EventLevel.Verbose, Keywords = Keywords.AsyncUtilities, Message = "AsyncLazy {0} starting initialization task {2}.")]
            public void AsyncLazyInitializationStarted(int AsyncLazyID, int OriginatingTaskID, int InitializationTaskID)
            {
                WriteEvent(13, AsyncLazyID, OriginatingTaskID, InitializationTaskID);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncLazy_Started<T>(AsyncLazy<T> lazy, Task initializationTask)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncUtilities))
                    AsyncLazyInitializationStarted(lazy.Id, Task.CurrentId ?? 0, initializationTask.Id);
            }

            [Event(14, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Message = "AsyncConditionVariable {0} notifying one task.")]
            public void AsyncConditionVariableNotifyOne(int AsyncConditionVariableID, int OriginatingTaskID, int AsyncLockID)
            {
                WriteEvent(14, AsyncConditionVariableID, OriginatingTaskID, AsyncLockID);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncConditionVariable_NotifyOne(AsyncConditionVariable conditionVariable, AsyncLock asyncLock)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncSynchronization))
                    AsyncConditionVariableNotifyOne(conditionVariable.Id, Task.CurrentId ?? 0, asyncLock.Id);
            }

            [Event(15, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Message = "AsyncConditionVariable {0} notifying all tasks.")]
            public void AsyncConditionVariableNotifyAll(int AsyncConditionVariableID, int OriginatingTaskID, int AsyncLockID)
            {
                WriteEvent(15, AsyncConditionVariableID, OriginatingTaskID, AsyncLockID);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncConditionVariable_NotifyAll(AsyncConditionVariable conditionVariable, AsyncLock asyncLock)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncSynchronization))
                    AsyncConditionVariableNotifyAll(conditionVariable.Id, Task.CurrentId ?? 0, asyncLock.Id);
            }

            [Event(16, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Opcode = EventOpcode.Start, Message = "AsyncConditionVariable {0} starting wait {4}.")]
            public void AsyncConditionVariableWaitBegin(int AsyncConditionVariableID, int OriginatingTaskID, int AsyncLockID, int NotifyTaskID, int WaitTaskID)
            {
                WriteEvent(16, AsyncConditionVariableID, OriginatingTaskID, AsyncLockID, NotifyTaskID, WaitTaskID);
            }

            [Event(17, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Opcode = EventOpcode.Stop, Message = "AsyncConditionVariable {0} completing wait {4}, canceled = {5}.")]
            public void AsyncConditionVariableWaitEnd(int AsyncConditionVariableID, int OriginatingTaskID, int AsyncLockID, int NotifyTaskID, int WaitTaskID, bool IsCanceled)
            {
                WriteEvent(17, AsyncConditionVariableID, OriginatingTaskID, AsyncLockID, NotifyTaskID, WaitTaskID, IsCanceled);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncConditionVariable_TrackWait(AsyncConditionVariable conditionVariable, AsyncLock asyncLock, Task notifyTask, Task waitTask)
            {
                if (!IsEnabled(EventLevel.Verbose, Keywords.AsyncSynchronization))
                    return;
                var conditionVariableId = conditionVariable.Id;
                var asyncLockId = asyncLock.Id;
                var notifyTaskId = notifyTask.Id;
                var waitTaskId = waitTask.Id;
                AsyncConditionVariableWaitBegin(conditionVariableId, Task.CurrentId ?? 0, asyncLockId, notifyTaskId, waitTaskId);
                waitTask.ContinueWith(t => AsyncConditionVariableWaitEnd(conditionVariableId, Task.CurrentId ?? 0, asyncLockId, notifyTaskId, waitTaskId, t.IsCanceled), TaskContinuationOptions.ExecuteSynchronously);
            }

            [Event(18, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Message = "AsyncMonitor created with AsyncLock {0} and AsyncConditionVariable {1}.")]
            public void AsyncMonitorCreated(int AsyncLockID, int AsyncConditionVariableID, int OriginatingTaskID)
            {
                WriteEvent(18, AsyncLockID, AsyncConditionVariableID, OriginatingTaskID);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncMonitor_Created(AsyncLock asyncLock, AsyncConditionVariable conditionVariable)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncSynchronization))
                    AsyncMonitorCreated(asyncLock.Id, conditionVariable.Id, Task.CurrentId ?? 0);
            }

            [Event(19, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Message = "AsyncAutoResetEvent {0} starting wait {2}, synchronous = {3}.")]
            public void AsyncAutoResetEventWaitBegin(int AsyncAutoResetEventID, int OriginatingTaskID, int WaitTaskID, bool IsSynchronous)
            {
                WriteEvent(19, AsyncAutoResetEventID, OriginatingTaskID, WaitTaskID, IsSynchronous);
            }

            [Event(20, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Message = "AsyncAutoResetEvent {0} completing wait {2}, canceled = {3}.")]
            public void AsyncAutoResetEventWaitEnd(int AsyncAutoResetEventID, int OriginatingTaskID, int WaitTaskID, bool IsCanceled)
            {
                WriteEvent(20, AsyncAutoResetEventID, OriginatingTaskID, WaitTaskID, IsCanceled);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncAutoResetEvent_TrackWait(AsyncAutoResetEvent autoResetEvent, Task waitTask)
            {
                if (!IsEnabled(EventLevel.Verbose, Keywords.AsyncSynchronization))
                    return;
                var autoResetEventId = autoResetEvent.Id;
                var waitTaskId = waitTask.Id;
                AsyncAutoResetEventWaitBegin(autoResetEventId, Task.CurrentId ?? 0, waitTaskId, waitTask.IsCompleted);
                waitTask.ContinueWith(t => AsyncAutoResetEventWaitEnd(autoResetEventId, Task.CurrentId ?? 0, waitTaskId, t.IsCanceled), TaskContinuationOptions.ExecuteSynchronously);
            }

            [Event(21, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Message = "AsyncAutoResetEvent {0} set.")]
            public void AsyncAutoResetEventSet(int AsyncAutoResetEventID, int OriginatingTaskID)
            {
                WriteEvent(21, AsyncAutoResetEventID, OriginatingTaskID);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncAutoResetEvent_Set(AsyncAutoResetEvent autoResetEvent)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncSynchronization))
                    AsyncAutoResetEventSet(autoResetEvent.Id, Task.CurrentId ?? 0);
            }

            [Event(22, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Message = "AsyncCountdownEvent {0} count changed from {2} to {3}.")]
            public void AsyncCountdownEventCountChanged(int AsyncCountdownEventTaskID, int OriginatingTaskID, int OldCount, int NewCount)
            {
                WriteEvent(22, AsyncCountdownEventTaskID, OriginatingTaskID, OldCount, NewCount);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncCountdownEvent_CountChanged(AsyncCountdownEvent countdownEvent, int oldCount, int newCount)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncSynchronization))
                    AsyncCountdownEventCountChanged(countdownEvent.Id, Task.CurrentId ?? 0, oldCount, newCount);
            }

            [Event(23, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Message = "AsyncBarrier {0} changed to phase {2} with {3} participants and wait task {4}.")]
            public void AsyncBarrierPhaseChanged(int AsyncBarrierID, int OriginatingTaskID, long Phase, int Participants, int WaitTaskID)
            {
                WriteEvent(23, AsyncBarrierID, OriginatingTaskID, Phase, Participants, WaitTaskID);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncBarrier_PhaseChanged(AsyncBarrier barrier, long phase, int participants, Task waitTask)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncSynchronization))
                    AsyncBarrierPhaseChanged(barrier.Id, Task.CurrentId ?? 0, phase, participants, waitTask.Id);
            }

            [Event(24, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Message = "AsyncBarrier {0} phase {2} count changed to {3}.")]
            public void AsyncBarrierCountChanged(int AsyncBarrierID, int OriginatingTaskID, long Phase, int Count)
            {
                WriteEvent(24, AsyncBarrierID, OriginatingTaskID, Phase, Count);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncBarrier_CountChanged(AsyncBarrier barrier, long phase, int count)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncSynchronization))
                    AsyncBarrierCountChanged(barrier.Id, Task.CurrentId ?? 0, phase, count);
            }

            [Event(25, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Message = "AsyncBarrier {0} phase {2} participants changed to {3}.")]
            public void AsyncBarrierParticipantsChanged(int AsyncBarrierID, int OriginatingTaskID, long Phase, int Participants)
            {
                WriteEvent(25, AsyncBarrierID, OriginatingTaskID, Phase, Participants);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncBarrier_ParticipantsChanged(AsyncBarrier barrier, long phase, int participants)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncSynchronization))
                    AsyncBarrierParticipantsChanged(barrier.Id, Task.CurrentId ?? 0, phase, participants);
            }

            [Event(26, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Message = "AsyncSemaphore {0} count changed to {2}.")]
            public void AsyncSemaphoreCountChanged(int AsyncSemaphoreID, int OriginatingTaskID, int Count)
            {
                WriteEvent(26, AsyncSemaphoreID, OriginatingTaskID, Count);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncSemaphore_CountChanged(AsyncSemaphore semaphore, int count)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncSynchronization))
                    AsyncSemaphoreCountChanged(semaphore.Id, Task.CurrentId ?? 0, count);
            }

            [Event(27, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Opcode = EventOpcode.Start, Message = "AsyncSemaphore {0} starting wait {2}.")]
            public void AsyncSemaphoreWaitBegin(int AsyncSemaphoreID, int OriginatingTaskID, int WaitTaskID)
            {
                WriteEvent(27, AsyncSemaphoreID, OriginatingTaskID, WaitTaskID);
            }

            [Event(28, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Opcode = EventOpcode.Stop, Message = "AsyncSemaphore {0} completing wait {2}, canceled = {3}.")]
            public void AsyncSemaphoreWaitEnd(int AsyncSemaphoreID, int OriginatingTaskID, int WaitTaskID, bool IsCanceled)
            {
                WriteEvent(28, AsyncSemaphoreID, OriginatingTaskID, WaitTaskID, IsCanceled);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncSemaphore_TrackWait(AsyncSemaphore semaphore, Task waitTask)
            {
                if (!IsEnabled(EventLevel.Verbose, Keywords.AsyncSynchronization))
                    return;
                var semaphoreId = semaphore.Id;
                var waitTaskId = waitTask.Id;
                AsyncSemaphoreWaitBegin(semaphoreId, Task.CurrentId ?? 0, waitTaskId);
                waitTask.ContinueWith(t => AsyncSemaphoreWaitEnd(semaphoreId, Task.CurrentId ?? 0, waitTaskId, t.IsCanceled), TaskContinuationOptions.ExecuteSynchronously);
            }

            [Event(29, Level = EventLevel.Informational, Keywords = Keywords.Dataflow, Message = "FuncBlock '{0}' ({1}) created with function task {2}.")]
            public void FuncBlockCreated(string BlockName, int BlockID, int FunctionTaskID)
            {
                WriteEvent(29, BlockName, BlockID, FunctionTaskID);
            }

            [NonEvent]
            void ITraceEnlightenment.FuncBlock_Created(ITraceableDataflowBlock block, Task functionTask)
            {
                if (IsEnabled(EventLevel.Informational, Keywords.Dataflow))
                    FuncBlockCreated(block.Name, block.Completion.Id, functionTask.Id);
            }

            [Event(30, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Opcode = EventOpcode.Start, Message = "AsyncReaderWriterLock {0} starting {2} wait {3}.")]
            public void AsyncReaderWriterLockLockBegin(int AsyncReaderWriterLockID, int OriginatingTaskID, AsyncReaderWriterLockLockType LockType, int WaitTaskID)
            {
                WriteEvent(30, AsyncReaderWriterLockID, OriginatingTaskID, LockType, WaitTaskID);
            }

            [Event(31, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Opcode = EventOpcode.Stop, Message = "AsyncReaderWriterLock {0} completing {2} wait {3}, canceled = {4}.")]
            public void AsyncReaderWriterLockLockEnd(int AsyncReaderWriterLockID, int OriginatingTaskID, int WaitTaskID, bool IsCanceled)
            {
                WriteEvent(31, AsyncReaderWriterLockID, OriginatingTaskID, WaitTaskID, IsCanceled);
            }

            [NonEvent]
            void ITraceEnlightenment.AsyncReaderWriterLock_TrackLock(AsyncReaderWriterLock rwLock, AsyncReaderWriterLockLockType lockType, Task waitTask)
            {
                if (!IsEnabled(EventLevel.Verbose, Keywords.AsyncSynchronization))
                    return;
                var rwLockId = rwLock.Id;
                var waitTaskId = waitTask.Id;
                AsyncReaderWriterLockLockBegin(rwLockId, Task.CurrentId ?? 0, lockType, waitTaskId);
                waitTask.ContinueWith(t => AsyncReaderWriterLockLockEnd(rwLockId, Task.CurrentId ?? 0, waitTaskId, t.IsCanceled), TaskContinuationOptions.ExecuteSynchronously);
            }

            [Event(32, Level = EventLevel.Verbose, Keywords = Keywords.AsyncSynchronization, Message = "AsyncReaderWriterLock {0} released {2}.")]
            public void AsyncReaderWriterLockLockReleased(int AsyncReaderWriterLockID, int OriginatingTaskID, AsyncReaderWriterLockLockType LockType)
            {
                WriteEvent(32, AsyncReaderWriterLockID, OriginatingTaskID, LockType);
            }

            void ITraceEnlightenment.AsyncReaderWriterLock_LockReleased(AsyncReaderWriterLock rwLock, AsyncReaderWriterLockLockType lockType)
            {
                if (IsEnabled(EventLevel.Verbose, Keywords.AsyncSynchronization))
                    AsyncReaderWriterLockLockReleased(rwLock.Id, Task.CurrentId ?? 0, lockType);
            }
        }
    }
}

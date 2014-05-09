using System.Threading;
using Nito.AsyncEx.Internal;
using Nito.AsyncEx.Internal.PlatformEnlightenment;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;

// Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266932.aspx

namespace Nito.AsyncEx
{
    /// <summary>
    /// An async-compatible barrier.
    /// </summary>
    [DebuggerDisplay("Id = {Id}, CurrentPhaseNumber = {_phase}, ParticipantsRemaining = {_count}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed class AsyncBarrier
    {
        /// <summary>
        /// Mutex used to control access to other fields.
        /// </summary>
        private readonly object _sync;

        /// <summary>
        /// The TCS used to signal the current phase.
        /// </summary>
        private TaskCompletionSource _tcs;

        /// <summary>
        /// The current phase.
        /// </summary>
        private long _phase;

        /// <summary>
        /// The remaining count on this event.
        /// </summary>
        private int _count;

        /// <summary>
        /// The total number of participants.
        /// </summary>
        private int _participants;

        /// <summary>
        /// The semi-unique identifier for this instance. This is 0 if the id has not yet been created.
        /// </summary>
        private int _id;

        /// <summary>
        /// The asynchronous post-phase action, if any. Either this member or <see cref="_syncPostPhaseAction"/> may be non-<c>null</c>, but not both.
        /// </summary>
        private readonly Func<AsyncBarrier, Task> _asyncPostPhaseAction;

        /// <summary>
        /// The synchonous post-phase action, if any. Either this member or <see cref="_asyncPostPhaseAction"/> may be non-<c>null</c>, but not both.
        /// </summary>
        private readonly Action<AsyncBarrier> _syncPostPhaseAction;

        /// <summary>
        /// Creates an async-compatible barrier.
        /// </summary>
        /// <param name="participants">The number of participants.</param>
        public AsyncBarrier(int participants)
        {
            _sync = new object();
            _tcs = new TaskCompletionSource();
            _participants = _count = participants;
            //Enlightenment.Trace.AsyncBarrier_PhaseChanged(this, 0, participants, _tcs.Task);
        }

        /// <summary>
        /// Creates an async-compatible barrier.
        /// </summary>
        /// <param name="participants">The number of participants.</param>
        /// <param name="postPhaseAction">The post-phase action to execute at the end of every phase.</param>
        public AsyncBarrier(int participants, Action<AsyncBarrier> postPhaseAction)
            : this(participants)
        {
            _syncPostPhaseAction = postPhaseAction;
        }

        /// <summary>
        /// Creates an async-compatible barrier.
        /// </summary>
        /// <param name="participants">The number of participants.</param>
        /// <param name="postPhaseAction">The post-phase action to execute at the end of every phase.</param>
        public AsyncBarrier(int participants, Func<AsyncBarrier, Task> postPhaseAction)
            : this(participants)
        {
            _asyncPostPhaseAction = postPhaseAction;
        }

        /// <summary>
        /// Gets a semi-unique identifier for this asynchronous barrier.
        /// </summary>
        public int Id
        {
            get { return IdManager<AsyncBarrier>.GetId(ref _id); }
        }

        /// <summary>
        /// Gets the current phase of the barrier.
        /// </summary>
        public long CurrentPhaseNumber
        {
            get { lock (_sync) { return _phase; } }
        }

        /// <summary>
        /// Gets the number of participants in this barrier.
        /// </summary>
        public int ParticipantCount
        {
            get { lock (_sync) { return _participants; } }
        }

        /// <summary>
        /// Gets the number of participants for this phase that have not yet signalled.
        /// </summary>
        public int ParticipantsRemaining
        {
            get { lock (_sync) { return _count; } }
        }

        /// <summary>
        /// Starts executing the post-phase action and returns a <see cref="Task"/> representing the action.
        /// </summary>
        private Task RunPostPhaseActionAsync()
        {
            if (_syncPostPhaseAction != null)
                return TaskShim.Run(() => _syncPostPhaseAction(this));
            if (_asyncPostPhaseAction != null)
                return TaskShim.Run(() => _asyncPostPhaseAction(this));
            return TaskConstants.Completed;
        }

        /// <summary>
        /// Signals completions to this barrier. Returns the task for the current phase, which may already be completed. Returns <c>null</c> if the signal count is greater than the remaining participant count.
        /// </summary>
        /// <param name="signalCount">The number of completions to signal.</param>
        /// <param name="removeParticipants">Whether the participants should be removed permanently.</param>
        private Task SignalAsync(int signalCount, bool removeParticipants)
        {
            TaskCompletionSource oldTcs = _tcs;
            lock (_sync)
            {
                if (signalCount == 0)
                    return _tcs.Task;
                if (signalCount > _count)
                    return null;
                _count -= signalCount;
                //Enlightenment.Trace.AsyncBarrier_CountChanged(this, _phase, _count);
                if (removeParticipants)
                {
                    _participants -= signalCount;
                    //Enlightenment.Trace.AsyncBarrier_ParticipantsChanged(this, _phase, _participants);
                }

                if (_count == 0)
                {
                    // Start post-phase action; when it completes, move to the next phase.
                    RunPostPhaseActionAsync().ContinueWith(t =>
                    {
                        lock (_sync)
                        {
                            _tcs = new TaskCompletionSource();
                            _count = _participants;
                            ++_phase;
                            //Enlightenment.Trace.AsyncBarrier_PhaseChanged(this, _phase, _participants, _tcs.Task);
                        }

                        // When the post-phase action completes, complete all waiting tasks for that phase, propagating the post-phase action result.
                        oldTcs.TryCompleteFromCompletedTask(t);
                    }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                }
            }
            
            return oldTcs.Task;
        }

        /// <summary>
        /// Signals the specified number of completions to this barrier and asynchronously waits for the phase to complete. This method may not be called during the post-phase action.
        /// </summary>
        /// <param name="count">The number of completion signals to send to this barrier.</param>
        public Task SignalAndWaitAsync(int count)
        {
            var ret = SignalAsync(count, removeParticipants: false);
            if (ret == null)
                throw new InvalidOperationException("Cannot signal barrier.");
            return ret;
        }

        /// <summary>
        /// Signals the specified number of completions to this barrier and synchronously waits for the phase to complete. This method may not be called during the post-phase action. This method may block the calling thread.
        /// </summary>
        /// <param name="count">The number of completion signals to send to this barrier.</param>
        public void SignalAndWait(int count)
        {
            var ret = SignalAsync(count, removeParticipants: false);
            if (ret == null)
                throw new InvalidOperationException("Cannot signal barrier.");
            ret.WaitAndUnwrapException();
        }

        /// <summary>
        /// Signals the specified number of completions to this barrier and synchronously waits for the phase to complete. This method may not be called during the post-phase action. This method may block the calling thread.
        /// </summary>
        /// <param name="count">The number of completion signals to send to this barrier.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this signal completes the phase and there is no post-phase action, then this token is ignored.</param>
        public void SignalAndWait(int count, CancellationToken cancellationToken)
        {
            var ret = SignalAsync(count, removeParticipants: false);
            if (ret == null)
                throw new InvalidOperationException("Cannot signal barrier.");
            if (ret.IsCompleted)
                ret.WaitAndUnwrapException();
            else
                ret.WaitAndUnwrapException(cancellationToken);
        }

        /// <summary>
        /// Signals a completion to this barrier and asynchronously waits for the phase to complete. This method may not be called during the post-phase action.
        /// </summary>
        public Task SignalAndWaitAsync()
        {
            return SignalAndWaitAsync(1);
        }

        /// <summary>
        /// Signals a completion to this barrier and asynchronously waits for the phase to complete. This method may not be called during the post-phase action. This method may block the calling thread.
        /// </summary>
        public void SignalAndWait()
        {
            SignalAndWait(1);
        }

        /// <summary>
        /// Signals a completion to this barrier and asynchronously waits for the phase to complete. This method may not be called during the post-phase action. This method may block the calling thread.
        /// </summary>
        public void SignalAndWait(CancellationToken cancellationToken)
        {
            SignalAndWait(1, cancellationToken);
        }

        /// <summary>
        /// Adds the specified number of participants to the barrier. Returns the current phase. This method may not be called during the post-phase action.
        /// </summary>
        /// <param name="count">The number of participants to add.</param>
        public long AddParticipants(int count)
        {
            if (count == 0)
                return CurrentPhaseNumber;
            lock (_sync)
            {
                if ((count > int.MaxValue - _participants) || (_count == 0 && _participants != 0))
                    throw new InvalidOperationException("Cannot add participants to barrier.");
                _count += count;
                //Enlightenment.Trace.AsyncBarrier_CountChanged(this, _phase, count);
                _participants += count;
                //Enlightenment.Trace.AsyncBarrier_ParticipantsChanged(this, _phase, _participants);
                return _phase;
            }
        }

        /// <summary>
        /// Adds a participant to the barrier. Returns the current phase. This method may not be called during the post-phase action.
        /// </summary>
        public long AddParticipants()
        {
            return AddParticipants(1);
        }

        /// <summary>
        /// Removes the specified number of participants from the barrier. These participants must not have signalled the barrier for this phase yet. This method may not be called during the post-phase action.
        /// </summary>
        /// <param name="count">The number of participants to remove.</param>
        [Obsolete("Use RemoveParticipants(int) or RemoveParticipantsAndWaitAsync(int)")]
        public Task RemoveParticipantsAsync(int count)
        {
            return RemoveParticipantsAndWaitAsync(count);
        }

        /// <summary>
        /// Removes the specified number of participants from the barrier and asynchronously waits for the phase to complete. These participants must not have signalled the barrier for this phase yet. This method may not be called during the post-phase action.
        /// </summary>
        /// <param name="count">The number of participants to remove.</param>
        public Task RemoveParticipantsAndWaitAsync(int count)
        {
            var ret = SignalAsync(count, removeParticipants: true);
            if (ret == null)
                throw new InvalidOperationException("Cannot remove participants from barrier.");
            return ret;
        }

        /// <summary>
        /// Removes the specified number of participants from the barrier and synchronously waits for the phase to complete. These participants must not have signalled the barrier for this phase yet. This method may not be called during the post-phase action. This method may block the calling thread.
        /// </summary>
        /// <param name="count">The number of participants to remove.</param>
        public void RemoveParticipantsAndWait(int count)
        {
            var ret = SignalAsync(count, removeParticipants: true);
            if (ret == null)
                throw new InvalidOperationException("Cannot remove participants from barrier.");
            ret.WaitAndUnwrapException();
        }

        /// <summary>
        /// Removes the specified number of participants from the barrier and synchronously waits for the phase to complete. These participants must not have signalled the barrier for this phase yet. This method may not be called during the post-phase action. This method may block the calling thread.
        /// </summary>
        /// <param name="count">The number of participants to remove.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this removal completes the phase and there is no post-phase action, then this token is ignored.</param>
        public void RemoveParticipantsAndWait(int count, CancellationToken cancellationToken)
        {
            var ret = SignalAsync(count, removeParticipants: true);
            if (ret == null)
                throw new InvalidOperationException("Cannot remove participants from barrier.");
            if (ret.IsCompleted)
                ret.WaitAndUnwrapException();
            else
                ret.WaitAndUnwrapException(cancellationToken);
        }

        /// <summary>
        /// Removes the specified number of participants from the barrier. These participants must not have signalled the barrier for this phase yet. This method may not be called during the post-phase action.
        /// </summary>
        /// <param name="count">The number of participants to remove.</param>
        public void RemoveParticipants(int count)
        {
            var ret = SignalAsync(count, removeParticipants: true);
            if (ret == null)
                throw new InvalidOperationException("Cannot remove participants from barrier.");
        }

        /// <summary>
        /// Removes one participant from the barrier. This participant must not have signalled the barrier for this phase yet. This method may not be called during the post-phase action.
        /// </summary>
        [Obsolete("Use RemoveParticipantsAndWaitAsync() or RemoveParticipants()")]
        public Task RemoveParticipantsAsync()
        {
            return RemoveParticipantsAndWaitAsync();
        }

        /// <summary>
        /// Removes one participant from the barrier and asynchronously waits for the phase to complete. This participant must not have signalled the barrier for this phase yet. This method may not be called during the post-phase action.
        /// </summary>
        public Task RemoveParticipantsAndWaitAsync()
        {
            return RemoveParticipantsAndWaitAsync(1);
        }

        /// <summary>
        /// Removes one participant from the barrier and synchronously waits for the phase to complete. This participant must not have signalled the barrier for this phase yet. This method may not be called during the post-phase action. This method may block the calling thread.
        /// </summary>
        public void RemoveParticipantsAndWait()
        {
            RemoveParticipantsAndWait(1);
        }

        /// <summary>
        /// Removes one participant from the barrier and synchronously waits for the phase to complete. This participant must not have signalled the barrier for this phase yet. This method may not be called during the post-phase action. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this removal completes the phase and there is no post-phase action, then this token is ignored.</param>
        public void RemoveParticipantsAndWait(CancellationToken cancellationToken)
        {
            RemoveParticipantsAndWait(1, cancellationToken);
        }

        /// <summary>
        /// Removes one participant from the barrier. This participant must not have signalled the barrier for this phase yet. This method may not be called during the post-phase action.
        /// </summary>
        public void RemoveParticipants()
        {
            RemoveParticipants(1);
        }

        // ReSharper disable UnusedMember.Local
        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly AsyncBarrier _barrier;

            public DebugView(AsyncBarrier barrier)
            {
                _barrier = barrier;
            }

            public int Id { get { return _barrier.Id; } }

            public long CurrentPhaseNumber { get { return _barrier._phase; } }

            public int RemainingParticipants { get { return _barrier._count; } }

            public int ParticipantCount { get { return _barrier._participants; } }

            public object PostPhaseAction { get { return (_barrier._asyncPostPhaseAction == null) ? _barrier._syncPostPhaseAction : (object)_barrier._asyncPostPhaseAction; } }

            public Task CurrentPhaseTask { get { return _barrier._tcs.Task; } }
        }
        // ReSharper restore UnusedMember.Local
    }
}

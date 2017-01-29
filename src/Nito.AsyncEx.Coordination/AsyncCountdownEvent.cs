using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

// Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx

namespace Nito.AsyncEx
{
    /// <summary>
    /// An async-compatible countdown event.
    /// </summary>
    [DebuggerDisplay("Id = {Id}, CurrentCount = {_count}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed class AsyncCountdownEvent
    {
        /// <summary>
        /// The underlying manual-reset event.
        /// </summary>
        private readonly AsyncManualResetEvent _mre;

        /// <summary>
        /// The remaining count on this event.
        /// </summary>
        private long _count;

        /// <summary>
        /// Creates an async-compatible countdown event.
        /// </summary>
        /// <param name="count">The number of signals this event will need before it becomes set.</param>
        public AsyncCountdownEvent(long count)
        {
            _mre = new AsyncManualResetEvent(count == 0);
            _count = count;
        }

        /// <summary>
        /// Gets a semi-unique identifier for this asynchronous countdown event.
        /// </summary>
        public int Id
        {
            get { return _mre.Id; }
        }

        /// <summary>
        /// Gets the current number of remaining signals before this event becomes set. This member is seldom used; code using this member has a high possibility of race conditions.
        /// </summary>
        public long CurrentCount
        {
            get
            {
                lock (_mre)
                    return _count;
            }
        }

        /// <summary>
        /// Asynchronously waits for the count to reach zero.
        /// </summary>
        public Task WaitAsync()
        {
            return _mre.WaitAsync();
        }

        /// <summary>
        /// Synchronously waits for the count to reach zero. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this token is already canceled, this method will first check whether the event is set.</param>
        public Task WaitAsync(CancellationToken cancellationToken)
        {
            return _mre.WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Synchronously waits for the count to reach zero. This method may block the calling thread.
        /// </summary>
        public void Wait()
        {
            _mre.Wait();
        }

        /// <summary>
        /// Synchronously waits for the count to reach zero. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this token is already canceled, this method will first check whether the event is set.</param>
        public void Wait(CancellationToken cancellationToken)
        {
            _mre.Wait(cancellationToken);
        }

        /// <summary>
        /// Attempts to modify the current count by the specified amount.
        /// </summary>
        /// <param name="difference">The amount to change the current count.</param>
        /// <param name="add"><c>true</c> to add to the current count; <c>false</c> to subtract.</param>
        private void ModifyCount(long difference, bool add)
        {
            if (difference == 0)
                return;
            lock (_mre)
            {
                var oldCount = _count;
                checked
                {
                    if (add)
                        _count += difference;
                    else
                        _count -= difference;
                }
                if (oldCount == 0)
                {
                    _mre.Reset();
                }
                else if (_count == 0)
                {
                    _mre.Set();
                }
                else if ((oldCount < 0 && _count > 0) || (oldCount > 0 && _count < 0))
                {
                    _mre.Set();
                    _mre.Reset();
                }
            }
        }

        /// <summary>
        /// Adds the specified value to the current count.
        /// </summary>
        /// <param name="addCount">The amount to change the current count.</param>
        public void AddCount(long addCount)
        {
            ModifyCount(addCount, true);
        }

        /// <summary>
        /// Adds one to the current count.
        /// </summary>
        public void AddCount()
        {
            AddCount(1);
        }

        /// <summary>
        /// Subtracts the specified value from the current count.
        /// </summary>
        /// <param name="signalCount">The amount to change the current count.</param>
        public void Signal(long signalCount)
        {
            ModifyCount(signalCount, false);
        }

        /// <summary>
        /// Subtracts one from the current count.
        /// </summary>
        public void Signal()
        {
            Signal(1);
        }

        // ReSharper disable UnusedMember.Local
        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly AsyncCountdownEvent _ce;

            public DebugView(AsyncCountdownEvent ce)
            {
                _ce = ce;
            }

            public int Id { get { return _ce.Id; } }

            public long CurrentCount { get { return _ce.CurrentCount; } }

            public AsyncManualResetEvent AsyncManualResetEvent { get { return _ce._mre; } }
        }
        // ReSharper restore UnusedMember.Local
    }
}

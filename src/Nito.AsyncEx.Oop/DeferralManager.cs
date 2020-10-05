using System;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// A source for deferrals. Event argument types may implement this interface to indicate they understand async event handlers.
    /// </summary>
    public interface IDeferralSource
    {
        /// <summary>
        /// Requests a deferral. When the deferral is disposed, it is considered complete.
        /// </summary>
        IDisposable GetDeferral();
    }

    /// <summary>
    /// Manages the deferrals for an event that may have asynchonous handlers and needs to know when they complete. Instances of this type may not be reused.
    /// </summary>
    public sealed class DeferralManager
    {
        /// <summary>
        /// The deferral source for deferrals managed by this manager.
        /// </summary>
        private readonly IDeferralSource _source;

        /// <summary>
        /// The lock protecting <see cref="_ce"/>.
        /// </summary>
        private readonly object _mutex;

        /// <summary>
        /// The underlying countdown event. May be <c>null</c> if no deferrals were ever requested.
        /// </summary>
        private AsyncCountdownEvent? _ce;

        /// <summary>
        /// Creates a new deferral manager.
        /// </summary>
        public DeferralManager()
        {
            _source = new ManagedDeferralSource(this);
            _mutex = new object();
        }

        /// <summary>
        /// Increments the count of active deferrals for this manager.
        /// </summary>
        internal void IncrementCount()
        {
            lock (_mutex)
            {
                if (_ce == null)
                    _ce = new AsyncCountdownEvent(1);
                else
                    _ce.AddCount();
            }
        }

        /// <summary>
        /// Decrements the count of active deferrals for this manager. If the count reaches <c>0</c>, then the manager notifies the code raising the event.
        /// </summary>
        internal void DecrementCount()
        {
            if (_ce == null)
                throw new InvalidOperationException("You must call IncrementCount before calling DecrementCount.");
            _ce.Signal();
        }

        /// <summary>
        /// Gets a source for deferrals managed by this deferral manager. This is generally used to implement <see cref="IDeferralSource"/> for event argument types.
        /// </summary>
        public IDeferralSource DeferralSource { get { return _source; } }

        /// <summary>
        /// Notifies the manager that all deferral requests have been made, and returns a task that is completed when all deferrals have completed.
        /// </summary>
        public Task WaitForDeferralsAsync()
        {
            lock (_mutex)
            {
                if (_ce == null)
                    return TaskConstants.Completed;
                return _ce.WaitAsync();
            }
        }

        /// <summary>
        /// A source for deferrals.
        /// </summary>
        private sealed class ManagedDeferralSource : IDeferralSource
        {
            /// <summary>
            /// The deferral manager in charge of this deferral source.
            /// </summary>
            private readonly DeferralManager _manager;

            public ManagedDeferralSource(DeferralManager manager)
            {
                _manager = manager;
            }

            IDisposable IDeferralSource.GetDeferral()
            {
                _manager.IncrementCount();
                return new Deferral(_manager);
            }

            /// <summary>
            /// A deferral.
            /// </summary>
            private sealed class Deferral : Disposables.SingleDisposable<DeferralManager>
            {
                public Deferral(DeferralManager manager)
                    : base(manager)
                {
                }

                protected override void Dispose(DeferralManager context)
                {
                    context.DecrementCount();
                }
            }
        }
    }
}
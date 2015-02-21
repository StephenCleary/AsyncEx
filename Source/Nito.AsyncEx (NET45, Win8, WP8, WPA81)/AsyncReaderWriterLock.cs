using Nito.AsyncEx.Internal;
using Nito.AsyncEx.Internal.PlatformEnlightenment;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;

// Original idea from Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/building-async-coordination-primitives-part-7-asyncreaderwriterlock.aspx

namespace Nito.AsyncEx
{
    /// <summary>
    /// A reader/writer lock that is compatible with async. Note that this lock is <b>not</b> recursive!
    /// </summary>
    [DebuggerDisplay("Id = {Id}, State = {GetStateForDebugger}, ReaderCount = {GetReaderCountForDebugger}, UpgradeInProgress = {GetUpgradeInProgressForDebugger}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed class AsyncReaderWriterLock
    {
        /// <summary>
        /// The queue of TCSs that other tasks are awaiting to acquire the lock as writers.
        /// </summary>
        private readonly IAsyncWaitQueue<IDisposable> _writerQueue;

        /// <summary>
        /// The queue of TCSs that other tasks are awaiting to acquire the lock as readers.
        /// </summary>
        private readonly IAsyncWaitQueue<IDisposable> _readerQueue;

        /// <summary>
        /// The queue of TCSs that other tasks are awaiting to acquire the lock as upgradeable readers.
        /// </summary>
        private readonly IAsyncWaitQueue<UpgradeableReaderKey> _upgradeableReaderQueue;

        /// <summary>
        /// The queue of TCSs that other tasks are awaiting to upgrade a reader lock to a writer lock.
        /// </summary>
        private readonly IAsyncWaitQueue<IDisposable> _upgradeReaderQueue;

        /// <summary>
        /// The semi-unique identifier for this instance. This is 0 if the id has not yet been created.
        /// </summary>
        private int _id;

        /// <summary>
        /// The current upgradeable reader lock key, if any. If this is not <c>null</c>, then there is an upgradeable reader lock held.
        /// </summary>
        private UpgradeableReaderKey _upgradeableReaderKey;

        /// <summary>
        /// Number of reader locks held (including an upgradeable reader lock, if applicable); -1 if a writer lock is held; 0 if no locks are held.
        /// </summary>
        private int _locksHeld;

        /// <summary>
        /// The object used for mutual exclusion.
        /// </summary>
        private readonly object _mutex;

        /// <summary>
        /// A task that is completed with the reader key object for this lock.
        /// </summary>
        private readonly Task<IDisposable> _cachedReaderKeyTask;

        /// <summary>
        /// A task that is completed with the writer key object for this lock.
        /// </summary>
        private readonly Task<IDisposable> _cachedWriterKeyTask;

        [DebuggerNonUserCode]
        internal State GetStateForDebugger
        {
            get
            {
                if (_locksHeld == 0)
                    return State.Unlocked;
                if (_locksHeld == -1)
                    if (_upgradeableReaderKey != null)
                        return State.WriteLockedWithUpgradeableReader;
                    else
                        return State.WriteLocked;
                if (_upgradeableReaderKey != null)
                    return State.ReadLockedWithUpgradeableReader;
                return State.ReadLocked;
            }
        }

        internal enum State
        {
            Unlocked,
            ReadLocked,
            ReadLockedWithUpgradeableReader,
            WriteLocked,
            WriteLockedWithUpgradeableReader,
        }

        [DebuggerNonUserCode]
        internal int GetReaderCountForDebugger { get { return (_locksHeld > 0 ? _locksHeld : 0); } }
        [DebuggerNonUserCode]
        internal bool GetUpgradeInProgressForDebugger { get { return !_upgradeReaderQueue.IsEmpty; } }

        /// <summary>
        /// Creates a new async-compatible reader/writer lock.
        /// </summary>
        public AsyncReaderWriterLock(IAsyncWaitQueue<IDisposable> writerQueue, IAsyncWaitQueue<IDisposable> readerQueue,
            IAsyncWaitQueue<UpgradeableReaderKey> upgradeableReaderQueue, IAsyncWaitQueue<IDisposable> upgradeReaderQueue)
        {
            _writerQueue = writerQueue;
            _readerQueue = readerQueue;
            _upgradeableReaderQueue = upgradeableReaderQueue;
            _upgradeReaderQueue = upgradeReaderQueue;
            _mutex = new object();
            _cachedReaderKeyTask = TaskShim.FromResult<IDisposable>(new ReaderKey(this));
            _cachedWriterKeyTask = TaskShim.FromResult<IDisposable>(new WriterKey(this));
        }

        /// <summary>
        /// Creates a new async-compatible reader/writer lock.
        /// </summary>
        public AsyncReaderWriterLock()
            : this(new DefaultAsyncWaitQueue<IDisposable>(), new DefaultAsyncWaitQueue<IDisposable>(),
            new DefaultAsyncWaitQueue<UpgradeableReaderKey>(), new DefaultAsyncWaitQueue<IDisposable>())
        {
        }

        /// <summary>
        /// Gets a semi-unique identifier for this asynchronous lock.
        /// </summary>
        public int Id
        {
            get { return IdManager<AsyncReaderWriterLock>.GetId(ref _id); }
        }

        internal object SyncObject
        {
            get { return _mutex; }
        }

        /// <summary>
        /// Applies a continuation to the task that will call <see cref="ReleaseWaiters"/> if the task is canceled. This method may not be called while holding the sync lock.
        /// </summary>
        /// <param name="task">The task to observe for cancellation.</param>
        private void ReleaseWaitersWhenCanceled(Task task)
        {
            task.ContinueWith(t =>
            {
                List<IDisposable> finishes;
                lock (SyncObject) { finishes = ReleaseWaiters(); }
                foreach (var finish in finishes)
                    finish.Dispose();
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        /// <summary>
        /// Asynchronously acquires the lock as a reader. Returns a disposable that releases the lock when disposed.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public AwaitableDisposable<IDisposable> ReaderLockAsync(CancellationToken cancellationToken)
        {
            Task<IDisposable> ret;
            lock (SyncObject)
            {
                // If the lock is available or in read mode and there are no waiting writers, upgradeable readers, or upgrading readers, take it immediately.
                if (_locksHeld >= 0 && _writerQueue.IsEmpty && _upgradeableReaderQueue.IsEmpty && _upgradeReaderQueue.IsEmpty)
                {
                    ++_locksHeld;
                    ret = _cachedReaderKeyTask;
                }
                else
                {
                    // Wait for the lock to become available or cancellation.
                    ret = _readerQueue.Enqueue(SyncObject, cancellationToken);
                }
                //Enlightenment.Trace.AsyncReaderWriterLock_TrackLock(this, AsyncReaderWriterLockLockType.Reader, ret);
            }

            return new AwaitableDisposable<IDisposable>(ret);
        }

        /// <summary>
        /// Synchronously acquires the lock as a reader. Returns a disposable that releases the lock when disposed. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public IDisposable ReaderLock(CancellationToken cancellationToken)
        {
            Task<IDisposable> ret;
            lock (SyncObject)
            {
                // If the lock is available or in read mode and there are no waiting writers, upgradeable readers, or upgrading readers, take it immediately.
                if (_locksHeld >= 0 && _writerQueue.IsEmpty && _upgradeableReaderQueue.IsEmpty && _upgradeReaderQueue.IsEmpty)
                {
                    ++_locksHeld;
                    return _cachedReaderKeyTask.Result;
                }

                // Wait for the lock to become available or cancellation.
                ret = _readerQueue.Enqueue(SyncObject, cancellationToken);
            }

            return ret.WaitAndUnwrapException();
        }

        /// <summary>
        /// Asynchronously acquires the lock as a reader. Returns a disposable that releases the lock when disposed.
        /// </summary>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public AwaitableDisposable<IDisposable> ReaderLockAsync()
        {
            return ReaderLockAsync(CancellationToken.None);
        }

        /// <summary>
        /// Synchronously acquires the lock as a reader. Returns a disposable that releases the lock when disposed. This method may block the calling thread.
        /// </summary>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public IDisposable ReaderLock()
        {
            return ReaderLock(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously acquires the lock as a writer. Returns a disposable that releases the lock when disposed.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public AwaitableDisposable<IDisposable> WriterLockAsync(CancellationToken cancellationToken)
        {
            Task<IDisposable> ret;
            lock (SyncObject)
            {
                // If the lock is available, take it immediately.
                if (_locksHeld == 0)
                {
                    _locksHeld = -1;
                    ret = _cachedWriterKeyTask;
                }
                else
                {
                    // Wait for the lock to become available or cancellation.
                    ret = _writerQueue.Enqueue(SyncObject, cancellationToken);
                }
                //Enlightenment.Trace.AsyncReaderWriterLock_TrackLock(this, AsyncReaderWriterLockLockType.Writer, ret);
            }

            ReleaseWaitersWhenCanceled(ret);
            return new AwaitableDisposable<IDisposable>(ret);
        }

        /// <summary>
        /// Synchronously acquires the lock as a writer. Returns a disposable that releases the lock when disposed. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public IDisposable WriterLock(CancellationToken cancellationToken)
        {
            Task<IDisposable> ret;
            lock (SyncObject)
            {
                // If the lock is available, take it immediately.
                if (_locksHeld == 0)
                {
                    _locksHeld = -1;
                    return _cachedWriterKeyTask.Result;
                }
             
                // Wait for the lock to become available or cancellation.
                ret = _writerQueue.Enqueue(SyncObject, cancellationToken);
            }

            ReleaseWaitersWhenCanceled(ret);
            return ret.WaitAndUnwrapException();
        }

        /// <summary>
        /// Asynchronously acquires the lock as a writer. Returns a disposable that releases the lock when disposed.
        /// </summary>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public AwaitableDisposable<IDisposable> WriterLockAsync()
        {
            return WriterLockAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously acquires the lock as a writer. Returns a disposable that releases the lock when disposed. This method may block the calling thread.
        /// </summary>
        /// <returns>A disposable that releases the lock when disposed.</returns>
        public IDisposable WriterLock()
        {
            return WriterLock(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously acquires the lock as a reader with the option to upgrade. Returns a key that can be used to upgrade and downgrade the lock, and releases the lock when disposed.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
        /// <returns>A key that can be used to upgrade and downgrade this lock, and releases the lock when disposed.</returns>
        public AwaitableDisposable<UpgradeableReaderKey> UpgradeableReaderLockAsync(CancellationToken cancellationToken)
        {
            Task<UpgradeableReaderKey> ret;
            lock (SyncObject)
            {
                // If the lock is available, take it immediately.
                if (_locksHeld == 0 || (_locksHeld > 0 && _upgradeableReaderKey == null))
                {
                    ++_locksHeld;
                    _upgradeableReaderKey = new UpgradeableReaderKey(this);
                    ret = TaskShim.FromResult(_upgradeableReaderKey);
                }
                else
                {
                    // Wait for the lock to become available or cancellation.
                    ret = _upgradeableReaderQueue.Enqueue(SyncObject, cancellationToken);
                }
                //Enlightenment.Trace.AsyncReaderWriterLock_TrackLock(this, AsyncReaderWriterLockLockType.UpgradeableReader, ret);
            }

            ReleaseWaitersWhenCanceled(ret);
            return new AwaitableDisposable<UpgradeableReaderKey>(ret);
        }

        /// <summary>
        /// Synchronously acquires the lock as a reader with the option to upgrade. Returns a key that can be used to upgrade and downgrade the lock, and releases the lock when disposed. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
        /// <returns>A key that can be used to upgrade and downgrade this lock, and releases the lock when disposed.</returns>
        public UpgradeableReaderKey UpgradeableReaderLock(CancellationToken cancellationToken)
        {
            Task<UpgradeableReaderKey> ret;
            lock (SyncObject)
            {
                // If the lock is available, take it immediately.
                if (_locksHeld == 0 || (_locksHeld > 0 && _upgradeableReaderKey == null))
                {
                    ++_locksHeld;
                    _upgradeableReaderKey = new UpgradeableReaderKey(this);
                    return _upgradeableReaderKey;
                }

                // Wait for the lock to become available or cancellation.
                ret = _upgradeableReaderQueue.Enqueue(SyncObject, cancellationToken);
            }

            ReleaseWaitersWhenCanceled(ret);
            return ret.WaitAndUnwrapException();
        }

        /// <summary>
        /// Asynchronously acquires the lock as a reader with the option to upgrade. Returns a key that can be used to upgrade and downgrade the lock, and releases the lock when disposed.
        /// </summary>
        /// <returns>A key that can be used to upgrade and downgrade this lock, and releases the lock when disposed.</returns>
        public AwaitableDisposable<UpgradeableReaderKey> UpgradeableReaderLockAsync()
        {
            return UpgradeableReaderLockAsync(CancellationToken.None);
        }

        /// <summary>
        /// Synchronously acquires the lock as a reader with the option to upgrade. Returns a key that can be used to upgrade and downgrade the lock, and releases the lock when disposed. This method may block the calling thread.
        /// </summary>
        /// <returns>A key that can be used to upgrade and downgrade this lock, and releases the lock when disposed.</returns>
        public UpgradeableReaderKey UpgradeableReaderLock()
        {
            return UpgradeableReaderLock(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously upgrades a reader lock to a writer lock. This method assumes the sync lock is already held.
        /// </summary>
        internal Task<IDisposable> UpgradeAsync(CancellationToken cancellationToken)
        {
            Task<IDisposable> ret;

            // If the lock is available, take it immediately.
            if (_locksHeld == 1)
            {
                _locksHeld = -1;
                ret = _upgradeableReaderKey._cachedUpgradeKeyTask;
            }
            else
            {
                // Wait for the lock to become available or cancellation.
                ret = _upgradeReaderQueue.Enqueue(SyncObject, cancellationToken);
            }

            //Enlightenment.Trace.AsyncReaderWriterLock_TrackLock(this, AsyncReaderWriterLockLockType.UpgradingToWriter, ret);
            return ret;
        }

        /// <summary>
        /// Downgrades a writer lock to a reader lock. This method assumes the sync lock is already held.
        /// </summary>
        internal List<IDisposable> Downgrade()
        {
            _locksHeld = 1;
            //Enlightenment.Trace.AsyncReaderWriterLock_LockReleased(this, AsyncReaderWriterLockLockType.UpgradingToWriter);
            return ReleaseWaiters();
        }

        /// <summary>
        /// Grants lock(s) to waiting tasks. This method assumes the sync lock is already held.
        /// </summary>
        private List<IDisposable> ReleaseWaiters()
        {
            var ret = new List<IDisposable>();

            if (_locksHeld == 0)
            {
                // Give priority to writers.
                if (!_writerQueue.IsEmpty)
                {
                    ret.Add(_writerQueue.Dequeue(_cachedWriterKeyTask.Result));
                    _locksHeld = -1;
                    return ret;
                }

                // Then to upgradeable readers.
                if (!_upgradeableReaderQueue.IsEmpty)
                {
                    _upgradeableReaderKey = new UpgradeableReaderKey(this);
                    ret.Add(_upgradeableReaderQueue.Dequeue(_upgradeableReaderKey));
                    ++_locksHeld;
                }

                // Finally to readers.
                while (!_readerQueue.IsEmpty)
                {
                    ret.Add(_readerQueue.Dequeue(_cachedReaderKeyTask.Result));
                    ++_locksHeld;
                }

                return ret;
            }

            // Give priority to upgrading readers.
            if (_locksHeld == 1)
            {
                if (!_upgradeReaderQueue.IsEmpty)
                {
                    ret.Add(_upgradeReaderQueue.Dequeue(_upgradeableReaderKey._cachedUpgradeKeyTask.Result));
                    _locksHeld = -1;
                }
            }
            
            if (_locksHeld > 0)
            {
                // If there are current reader locks and waiting writers, then do nothing.
                if (!_writerQueue.IsEmpty || !_upgradeableReaderQueue.IsEmpty || !_upgradeReaderQueue.IsEmpty)
                    return ret;

                // If there are current reader locks but no upgradeable reader lock, try to release an upgradeable reader.
                if (_upgradeableReaderKey == null && !_upgradeableReaderQueue.IsEmpty)
                {
                    _upgradeableReaderKey = new UpgradeableReaderKey(this);
                    ret.Add(_upgradeableReaderQueue.Dequeue(_upgradeableReaderKey));
                }
            }

            return ret;
        }

        /// <summary>
        /// Releases the lock as a reader.
        /// </summary>
        internal void ReleaseReaderLock()
        {
            List<IDisposable> finishes;
            lock (SyncObject)
            {
                --_locksHeld;
                //Enlightenment.Trace.AsyncReaderWriterLock_LockReleased(this, AsyncReaderWriterLockLockType.Reader);
                finishes = ReleaseWaiters();
            }
            foreach (var finish in finishes)
                finish.Dispose();
        }

        /// <summary>
        /// Releases the lock as a writer.
        /// </summary>
        internal void ReleaseWriterLock()
        {
            List<IDisposable> finishes;
            lock (SyncObject)
            {
                _locksHeld = 0;
                //Enlightenment.Trace.AsyncReaderWriterLock_LockReleased(this, AsyncReaderWriterLockLockType.Writer);
                finishes = ReleaseWaiters();
            }
            foreach (var finish in finishes)
                finish.Dispose();
        }

        /// <summary>
        /// Releases the lock as an upgradeable reader.
        /// </summary>
        internal void ReleaseUpgradeableReaderLock(Task upgrade)
        {
            IDisposable cancelFinish = null;
            List<IDisposable> finishes;
            lock (SyncObject)
            {
                if (upgrade != null)
                    cancelFinish = _upgradeReaderQueue.TryCancel(upgrade);
                _upgradeableReaderKey = null;
                //Enlightenment.Trace.AsyncReaderWriterLock_LockReleased(this, AsyncReaderWriterLockLockType.UpgradeableReader);
                --_locksHeld;
                finishes = ReleaseWaiters();
            }
            if (cancelFinish != null)
                cancelFinish.Dispose();
            foreach (var finish in finishes)
                finish.Dispose();
        }

        /// <summary>
        /// The disposable which releases the reader lock.
        /// </summary>
        private sealed class ReaderKey : IDisposable
        {
            /// <summary>
            /// The lock to release.
            /// </summary>
            private readonly AsyncReaderWriterLock _asyncReaderWriterLock;

            /// <summary>
            /// Creates the key for a lock.
            /// </summary>
            /// <param name="asyncReaderWriterLock">The lock to release. May not be <c>null</c>.</param>
            public ReaderKey(AsyncReaderWriterLock asyncReaderWriterLock)
            {
                _asyncReaderWriterLock = asyncReaderWriterLock;
            }

            /// <summary>
            /// Release the lock.
            /// </summary>
            public void Dispose()
            {
                _asyncReaderWriterLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// The disposable which releases the writer lock.
        /// </summary>
        private sealed class WriterKey : IDisposable
        {
            /// <summary>
            /// The lock to release.
            /// </summary>
            private readonly AsyncReaderWriterLock _asyncReaderWriterLock;

            /// <summary>
            /// Creates the key for a lock.
            /// </summary>
            /// <param name="asyncReaderWriterLock">The lock to release. May not be <c>null</c>.</param>
            public WriterKey(AsyncReaderWriterLock asyncReaderWriterLock)
            {
                _asyncReaderWriterLock = asyncReaderWriterLock;
            }

            /// <summary>
            /// Release the lock.
            /// </summary>
            public void Dispose()
            {
                _asyncReaderWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// The disposable which manages the upgradeable reader lock.
        /// </summary>
        [DebuggerDisplay("State = {GetStateForDebugger}, ReaderWriterLockId = {_asyncReaderWriterLock.Id}")]
        public sealed class UpgradeableReaderKey : IDisposable
        {
            /// <summary>
            /// The lock to release.
            /// </summary>
            private readonly AsyncReaderWriterLock _asyncReaderWriterLock;

            /// <summary>
            /// The task doing the upgrade.
            /// </summary>
            private Task<IDisposable> _upgrade;

            /// <summary>
            /// A task that is completed with the upgrade key object for this key.
            /// </summary>
            internal readonly Task<IDisposable> _cachedUpgradeKeyTask;

            [DebuggerNonUserCode]
            internal State GetStateForDebugger
            {
                get
                {
                    if (_upgrade == null)
                        return State.Reader;
                    if (_upgrade.Status == TaskStatus.RanToCompletion)
                        return State.Writer;
                    return State.UpgradingToWriter;
                }
            }

            internal enum State
            {
                Reader,
                UpgradingToWriter,
                Writer,
            }

            /// <summary>
            /// Creates the key for a lock.
            /// </summary>
            /// <param name="asyncReaderWriterLock">The lock to release. May not be <c>null</c>.</param>
            internal UpgradeableReaderKey(AsyncReaderWriterLock asyncReaderWriterLock)
            {
                _asyncReaderWriterLock = asyncReaderWriterLock;
                _cachedUpgradeKeyTask = TaskShim.FromResult<IDisposable>(new UpgradeKey(this));
            }

            /// <summary>
            /// Gets a value indicating whether this lock has been upgraded to a write lock.
            /// </summary>
            public bool Upgraded
            {
                get
                {
                    Task task;
                    lock (_asyncReaderWriterLock.SyncObject) { task = _upgrade; }
                    return (task != null && task.Status == TaskStatus.RanToCompletion);
                }
            }

            /// <summary>
            /// Upgrades the reader lock to a writer lock. Returns a disposable that downgrades the writer lock to a reader lock when disposed.
            /// </summary>
            /// <param name="cancellationToken">The cancellation token used to cancel the upgrade. If this is already set, then this method will attempt to upgrade immediately (succeeding if the lock is currently available).</param>
            public AwaitableDisposable<IDisposable> UpgradeAsync(CancellationToken cancellationToken)
            {
                lock (_asyncReaderWriterLock.SyncObject)
                {
                    if (_upgrade != null)
                        throw new InvalidOperationException("Cannot upgrade.");

                    _upgrade = _asyncReaderWriterLock.UpgradeAsync(cancellationToken);
                }

                _asyncReaderWriterLock.ReleaseWaitersWhenCanceled(_upgrade);
                var ret = new TaskCompletionSource<IDisposable>();
                _upgrade.ContinueWith(t =>
                {
                    if (t.IsCanceled)
                        lock (_asyncReaderWriterLock.SyncObject) { _upgrade = null; }
                    ret.TryCompleteFromCompletedTask(t);
                }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                return new AwaitableDisposable<IDisposable>(ret.Task);
            }

            /// <summary>
            /// Synchronously upgrades the reader lock to a writer lock. Returns a disposable that downgrades the writer lock to a reader lock when disposed. This method may block the calling thread.
            /// </summary>
            /// <param name="cancellationToken">The cancellation token used to cancel the upgrade. If this is already set, then this method will attempt to upgrade immediately (succeeding if the lock is currently available).</param>
            public IDisposable Upgrade(CancellationToken cancellationToken)
            {
                lock (_asyncReaderWriterLock.SyncObject)
                {
                    if (_upgrade != null)
                        throw new InvalidOperationException("Cannot upgrade.");

                    _upgrade = _asyncReaderWriterLock.UpgradeAsync(cancellationToken);
                }

                _asyncReaderWriterLock.ReleaseWaitersWhenCanceled(_upgrade);
                var ret = new TaskCompletionSource<IDisposable>();
                _upgrade.ContinueWith(t =>
                {
                    if (t.IsCanceled)
                        lock (_asyncReaderWriterLock.SyncObject) { _upgrade = null; }
                    ret.TryCompleteFromCompletedTask(t);
                }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                return ret.Task.WaitAndUnwrapException();
            }

            /// <summary>
            /// Upgrades the reader lock to a writer lock. Returns a disposable that downgrades the writer lock to a reader lock when disposed.
            /// </summary>
            public AwaitableDisposable<IDisposable> UpgradeAsync()
            {
                return UpgradeAsync(CancellationToken.None);
            }

            /// <summary>
            /// Synchronously upgrades the reader lock to a writer lock. Returns a disposable that downgrades the writer lock to a reader lock when disposed. This method may block the calling thread.
            /// </summary>
            public IDisposable Upgrade()
            {
                return Upgrade(CancellationToken.None);
            }

            /// <summary>
            /// Downgrades the writer lock to a reader lock.
            /// </summary>
            private void Downgrade()
            {
                List<IDisposable> finishes;
                lock (_asyncReaderWriterLock.SyncObject)
                {
                    finishes = _asyncReaderWriterLock.Downgrade();
                    _upgrade = null;
                }
                foreach (var finish in finishes)
                    finish.Dispose();
            }

            /// <summary>
            /// Release the lock.
            /// </summary>
            public void Dispose()
            {
                _asyncReaderWriterLock.ReleaseUpgradeableReaderLock(_upgrade);
            }

            /// <summary>
            /// The disposable which downgrades an upgradeable reader key.
            /// </summary>
            internal sealed class UpgradeKey : IDisposable
            {
                /// <summary>
                /// The upgradeable reader key to downgrade.
                /// </summary>
                private readonly UpgradeableReaderKey _key;

                /// <summary>
                /// Creates the upgrade key for an upgradeable reader key.
                /// </summary>
                /// <param name="key">The upgradeable reader key to downgrade. May not be <c>null</c>.</param>
                public UpgradeKey(UpgradeableReaderKey key)
                {
                    _key = key;
                }

                /// <summary>
                /// Downgrade the upgradeable reader key.
                /// </summary>
                public void Dispose()
                {
                    _key.Downgrade();
                }
            }
        }

        // ReSharper disable UnusedMember.Local
        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly AsyncReaderWriterLock _rwl;

            public DebugView(AsyncReaderWriterLock rwl)
            {
                _rwl = rwl;
            }

            public int Id { get { return _rwl.Id; } }

            public State State { get { return _rwl.GetStateForDebugger; } }

            public int ReaderCount { get { return _rwl.GetReaderCountForDebugger; } }

            public bool UpgradeInProgress { get { return _rwl.GetUpgradeInProgressForDebugger; } }

            public IAsyncWaitQueue<IDisposable> ReaderWaitQueue { get { return _rwl._readerQueue; } }

            public IAsyncWaitQueue<IDisposable> WriterWaitQueue { get { return _rwl._writerQueue; } }

            public IAsyncWaitQueue<UpgradeableReaderKey> UpgradeableReaderWaitQueue { get { return _rwl._upgradeableReaderQueue; } }

            public IAsyncWaitQueue<IDisposable> UpgradeReaderWaitQueue { get { return _rwl._upgradeReaderQueue; } }
        }
        // ReSharper restore UnusedMember.Local
    }
}

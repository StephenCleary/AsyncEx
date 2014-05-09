using Nito.AsyncEx.Internal.PlatformEnlightenment;
using Nito.AsyncEx.Synchronous;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// Provides a context for asynchronous operations. This class is threadsafe.
    /// </summary>
    /// <remarks>
    /// <para><see cref="Execute()"/> may only be called once. After <see cref="Execute()"/> returns, the async context should be disposed.</para>
    /// </remarks>
    [DebuggerDisplay("Id = {Id}, OperationCount = {_outstandingOperations}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed partial class AsyncContext : IDisposable
    {
        /// <summary>
        /// The queue holding the actions to run.
        /// </summary>
        private readonly TaskQueue _queue;

        /// <summary>
        /// The <see cref="SynchronizationContext"/> for this <see cref="AsyncContext"/>.
        /// </summary>
        private readonly AsyncContextSynchronizationContext _synchronizationContext;

        /// <summary>
        /// The <see cref="TaskScheduler"/> for this <see cref="AsyncContext"/>.
        /// </summary>
        private readonly AsyncContextTaskScheduler _taskScheduler;

        /// <summary>
        /// The <see cref="TaskFactory"/> for this <see cref="AsyncContext"/>.
        /// </summary>
        private readonly TaskFactory _taskFactory;

        /// <summary>
        /// The number of outstanding operations, including actions in the queue.
        /// </summary>
        private int _outstandingOperations;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncContext"/> class. This is an advanced operation; most people should use one of the static <c>Run</c> methods instead.
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public AsyncContext()
        {
            _queue = new TaskQueue();
            _synchronizationContext = new AsyncContextSynchronizationContext(this);
            _taskScheduler = new AsyncContextTaskScheduler(this);
            _taskFactory = new TaskFactory(_taskScheduler);
        }

        /// <summary>
        /// Gets a semi-unique identifier for this asynchronous context. This is the same identifier as the context's <see cref="TaskScheduler"/>.
        /// </summary>
        public int Id
        {
            get { return _taskScheduler.Id; }
        }

        /// <summary>
        /// Increments the outstanding asynchronous operation count.
        /// </summary>
        private void OperationStarted()
        {
            var newCount = Interlocked.Increment(ref _outstandingOperations);
            //Enlightenment.Trace.AsyncContext_OperationIncrement(this, newCount);
        }

        /// <summary>
        /// Decrements the outstanding asynchronous operation count.
        /// </summary>
        private void OperationCompleted()
        {
            var newCount = Interlocked.Decrement(ref _outstandingOperations);
            //Enlightenment.Trace.AsyncContext_OperationDecrement(this, newCount);
            if (newCount == 0)
                _queue.CompleteAdding();
        }

        /// <summary>
        /// Queues a task for execution by <see cref="Execute"/>. If all tasks have been completed and the outstanding asynchronous operation count is zero, then this method has undefined behavior.
        /// </summary>
        /// <param name="task">The task to queue. May not be <c>null</c>.</param>
        /// <param name="propagateExceptions">A value indicating whether exceptions on this task should be propagated out of the main loop.</param>
        private void Enqueue(Task task, bool propagateExceptions)
        {
            //Enlightenment.Trace.AsyncContext_TaskScheduled(this, task);
            OperationStarted();
            task.ContinueWith(_ => OperationCompleted(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            _queue.TryAdd(task, propagateExceptions);
            //if (!_queue.TryAdd(task, propagateExceptions))
            //    Enlightenment.Trace.AsyncContext_TaskQueueFailed(this, task);

            // If we fail to add to the queue, just drop the Task. This is the same behavior as the TaskScheduler.FromCurrentSynchronizationContext(WinFormsSynchronizationContext).
        }

        /// <summary>
        /// Disposes all resources used by this class. This method should NOT be called while <see cref="Execute"/> is executing.
        /// </summary>
        public void Dispose()
        {
            _queue.Dispose();
        }

        /// <summary>
        /// Executes all queued actions. This method returns when all tasks have been completed and the outstanding asynchronous operation count is zero. This method will unwrap and propagate errors from tasks that are supposed to propagate errors.
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public void Execute()
        {
            using (new SynchronizationContextHelpers.SynchronizationContextSwitcher(_synchronizationContext))
            {
                var tasks = _queue.GetConsumingEnumerable();
                foreach (var task in tasks)
                {
                    _taskScheduler.DoTryExecuteTask(task.Item1);

                    // Propagate exception if necessary.
                    if (task.Item2)
                        task.Item1.WaitAndUnwrapException();
                }
            }
        }

        /// <summary>
        /// Queues a task for execution, and begins executing all tasks in the queue. This method returns when all tasks have been completed and the outstanding asynchronous operation count is zero. This method will unwrap and propagate errors from the task.
        /// </summary>
        /// <param name="action">The action to execute. May not be <c>null</c>.</param>
        public static void Run(Action action)
        {
            using (var context = new AsyncContext())
            {
                var task = context._taskFactory.Run(action);
                context.Execute();
                task.WaitAndUnwrapException();
            }
        }

        /// <summary>
        /// Queues a task for execution, and begins executing all tasks in the queue. This method returns when all tasks have been completed and the outstanding asynchronous operation count is zero. Returns the result of the task. This method will unwrap and propagate errors from the task.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="action">The action to execute. May not be <c>null</c>.</param>
        public static TResult Run<TResult>(Func<TResult> action)
        {
            using (var context = new AsyncContext())
            {
                var task = context._taskFactory.Run(action);
                context.Execute();
                return task.WaitAndUnwrapException();
            }
        }

        /// <summary>
        /// Queues a task for execution, and begins executing all tasks in the queue. This method returns when all tasks have been completed and the outstanding asynchronous operation count is zero. This method will unwrap and propagate errors from the task proxy.
        /// </summary>
        /// <param name="action">The action to execute. May not be <c>null</c>.</param>
        public static void Run(Func<Task> action)
        {
            // ReSharper disable AccessToDisposedClosure
            using (var context = new AsyncContext())
            {
                context.OperationStarted();
                var task = context._taskFactory.Run(action).ContinueWith(t =>
                {
                    context.OperationCompleted();
                    t.WaitAndUnwrapException();
                }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                context.Execute();
                task.WaitAndUnwrapException();
            }
            // ReSharper restore AccessToDisposedClosure
        }

        /// <summary>
        /// Queues a task for execution, and begins executing all tasks in the queue. This method returns when all tasks have been completed and the outstanding asynchronous operation count is zero. Returns the result of the task proxy. This method will unwrap and propagate errors from the task proxy.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="action">The action to execute. May not be <c>null</c>.</param>
        public static TResult Run<TResult>(Func<Task<TResult>> action)
        {
            // ReSharper disable AccessToDisposedClosure
            using (var context = new AsyncContext())
            {
                context.OperationStarted();
                var task = context._taskFactory.Run(action).ContinueWith(t =>
                {
                    context.OperationCompleted();
                    return t.WaitAndUnwrapException();
                }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                context.Execute();
                return task.WaitAndUnwrapException();
            }
            // ReSharper restore AccessToDisposedClosure
        }

        /// <summary>
        /// Gets the current <see cref="AsyncContext"/> for this thread, or <c>null</c> if this thread is not currently running in an <see cref="AsyncContext"/>.
        /// </summary>
        public static AsyncContext Current
        {
            get
            {
                var syncContext = SynchronizationContext.Current as AsyncContextSynchronizationContext;
                if (syncContext == null)
                {
                    return null;
                }

                return syncContext.Context;
            }
        }

        /// <summary>
        /// Gets the <see cref="SynchronizationContext"/> for this <see cref="AsyncContext"/>. From inside <see cref="Execute"/>, this value is always equal to <see cref="System.Threading.SynchronizationContext.Current"/>.
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public SynchronizationContext SynchronizationContext
        {
            get { return _synchronizationContext; }
        }

        /// <summary>
        /// Gets the <see cref="TaskScheduler"/> for this <see cref="AsyncContext"/>. From inside <see cref="Execute"/>, this value is always equal to <see cref="TaskScheduler.Current"/>.
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public TaskScheduler Scheduler
        {
            get { return _taskScheduler; }
        }

        /// <summary>
        /// Gets the <see cref="TaskFactory"/> for this <see cref="AsyncContext"/>. Be careful with async delegates; you may need to call <see cref="M:System.Threading.SynchronizationContext.OperationStarted"/> and <see cref="M:System.Threading.SynchronizationContext.OperationCompleted"/> to prevent early termination of this <see cref="AsyncContext"/>.
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public TaskFactory Factory
        {
            get { return _taskFactory; }
        }

        [DebuggerNonUserCode]
        internal sealed class DebugView
        {
            private readonly AsyncContext _context;

            public DebugView(AsyncContext context)
            {
                _context = context;
            }

            public TaskScheduler TaskScheduler
            {
                get { return _context._taskScheduler; }
            }
        }
    }
}

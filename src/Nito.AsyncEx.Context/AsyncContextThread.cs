using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;

namespace Nito.AsyncEx
{
    /// <summary>
    /// A thread that executes actions within an <see cref="AsyncContext"/>.
    /// </summary>
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed class AsyncContextThread : Disposables.SingleDisposable<AsyncContext>
    {
        /// <summary>
        /// The child thread.
        /// </summary>
        private readonly Task _thread;

        /// <summary>
        /// Creates a new <see cref="AsyncContext"/> and increments its operation count.
        /// </summary>
        private static AsyncContext CreateAsyncContext()
        {
            var result = new AsyncContext();
            result.SynchronizationContext.OperationStarted();
            return result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncContextThread"/> class, creating a child thread waiting for commands.
        /// </summary>
        /// <param name="context">The context for this thread.</param>
        private AsyncContextThread(AsyncContext context)
            : base(context)
        {
            Context = context;
            _thread = Task.Factory.StartNew(Execute, CancellationToken.None, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncContextThread"/> class, creating a child thread waiting for commands.
        /// </summary>
        public AsyncContextThread()
            : this(CreateAsyncContext())
        {
        }

        /// <summary>
        /// Gets the <see cref="AsyncContext"/> executed by this thread.
        /// </summary>
        public AsyncContext Context { get; }

        private void Execute()
        {
            using (Context)
            {
                Context.Execute();
            }
        }

        /// <summary>
        /// Permits the thread to exit, if we have not already done so.
        /// </summary>
        private void AllowThreadToExit()
        {
            Context.SynchronizationContext.OperationCompleted();
        }

        /// <summary>
        /// Requests the thread to exit and returns a task representing the exit of the thread. The thread will exit when all outstanding asynchronous operations complete.
        /// </summary>
        public Task JoinAsync()
        {
            Dispose();
            return _thread;
        }

        /// <summary>
        /// Requests the thread to exit and blocks until the thread exits. The thread will exit when all outstanding asynchronous operations complete.
        /// </summary>
        public void Join()
        {
            JoinAsync().WaitAndUnwrapException();
        }

        /// <summary>
        /// Requests the thread to exit.
        /// </summary>
        protected override void Dispose(AsyncContext context)
        {
            AllowThreadToExit();
        }

        /// <summary>
        /// Gets the <see cref="TaskFactory"/> for this thread, which can be used to schedule work to this thread.
        /// </summary>
        public TaskFactory Factory => Context.Factory;

        [DebuggerNonUserCode]
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
        internal sealed class DebugView
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
        {
            private readonly AsyncContextThread _thread;

            public DebugView(AsyncContextThread thread)
            {
                _thread = thread;
            }

            public AsyncContext Context => _thread.Context;

            public object Thread => _thread._thread;
        }
    }
}

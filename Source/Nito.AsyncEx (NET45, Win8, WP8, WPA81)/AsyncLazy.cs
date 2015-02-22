using System;
using System.Threading.Tasks;
using Nito.AsyncEx.Internal;
using System.Diagnostics;
using Nito.AsyncEx.Internal.PlatformEnlightenment;
#if NONATIVETASKS
using Microsoft.Runtime.CompilerServices;
#else
using System.Runtime.CompilerServices;
#endif

namespace Nito.AsyncEx
{
    /// <summary>
    /// Provides support for asynchronous lazy initialization. This type is fully threadsafe.
    /// </summary>
    /// <typeparam name="T">The type of object that is being asynchronously initialized.</typeparam>
    [DebuggerDisplay("Id = {Id}, State = {GetStateForDebugger}")]
    [DebuggerTypeProxy(typeof(AsyncLazy<>.DebugView))]
    public sealed class AsyncLazy<T>
    {
        /// <summary>
        /// The underlying lazy task.
        /// </summary>
        private readonly Lazy<Task<T>> _instance;

        /// <summary>
        /// The semi-unique identifier for this instance. This is 0 if the id has not yet been created.
        /// </summary>
        private int _id;

        [DebuggerNonUserCode]
        internal LazyState GetStateForDebugger
        {
            get
            {
                if (!_instance.IsValueCreated)
                    return LazyState.NotStarted;
                if (!_instance.Value.IsCompleted)
                    return LazyState.Executing;
                return LazyState.Completed;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="factory">The delegate that is invoked on a background thread to produce the value when it is needed. May not be <c>null</c>.</param>
        public AsyncLazy(Func<T> factory)
        {
            _instance = new Lazy<Task<T>>(() =>
            {
                var ret = TaskShim.Run(factory);
                //Enlightenment.Trace.AsyncLazy_Started(this, ret);
                return ret;
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="factory">The asynchronous delegate that is invoked on a background thread to produce the value when it is needed. May not be <c>null</c>.</param>
        public AsyncLazy(Func<Task<T>> factory)
        {
            _instance = new Lazy<Task<T>>(() =>
            {
                var ret = TaskShim.Run(factory);
                //Enlightenment.Trace.AsyncLazy_Started(this, ret);
                return ret;
            });
        }

        /// <summary>
        /// Gets a semi-unique identifier for this asynchronous lazy instance.
        /// </summary>
        public int Id
        {
            get { return IdManager<AsyncLazy<object>>.GetId(ref _id); }
        }

        /// <summary>
        /// Whether the asynchronous factory method has started. This is initially <c>false</c> and becomes <c>true</c> when this instance is awaited or after <see cref="Start"/> is called.
        /// </summary>
        public bool IsStarted
        {
            get { return _instance.IsValueCreated; }
        }

        /// <summary>
        /// Asynchronous infrastructure support. This method permits instances of <see cref="AsyncLazy&lt;T&gt;"/> to be await'ed.
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public TaskAwaiter<T> GetAwaiter()
        {
            return _instance.Value.GetAwaiter();
        }

        /// <summary>
        /// Starts the asynchronous initialization, if it has not already started.
        /// </summary>
        public void Start()
        {
// ReSharper disable UnusedVariable
            var unused = _instance.Value;
// ReSharper restore UnusedVariable
        }

        internal enum LazyState
        {
            NotStarted,
            Executing,
            Completed
        }

        [DebuggerNonUserCode]
        internal sealed class DebugView
        {
            private readonly AsyncLazy<T> _lazy;

            public DebugView(AsyncLazy<T> lazy)
            {
                _lazy = lazy;
            }

            public LazyState State { get { return _lazy.GetStateForDebugger; } }

            public Task Task
            {
                get
                {
                    if (!_lazy._instance.IsValueCreated)
                        throw new InvalidOperationException("Not yet created.");
                    return _lazy._instance.Value;
                }
            }

            public T Value
            {
                get
                {
                    if (!_lazy._instance.IsValueCreated || !_lazy._instance.Value.IsCompleted)
                        throw new InvalidOperationException("Not yet created.");
                    return _lazy._instance.Value.Result;
                }
            }
        }
    }
}

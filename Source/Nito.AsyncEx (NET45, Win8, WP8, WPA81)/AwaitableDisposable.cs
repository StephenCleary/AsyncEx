using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NONATIVETASKS
using Microsoft.Runtime.CompilerServices;
#else
using System.Runtime.CompilerServices;
#endif

namespace Nito.AsyncEx
{
    /// <summary>
    /// An awaitable wrapper around a task whose result is disposable. The wrapper is not disposable, so this prevents usage errors like "using (MyAsync())" when the appropriate usage should be "using (await MyAsync())".
    /// </summary>
    /// <typeparam name="T">The type of the result of the underlying task.</typeparam>
    public struct AwaitableDisposable<T> where T : IDisposable
    {
        /// <summary>
        /// The underlying task.
        /// </summary>
        private readonly Task<T> _task;

        /// <summary>
        /// Initializes a new awaitable wrapper around the specified task.
        /// </summary>
        /// <param name="task">The underlying task to wrap.</param>
        public AwaitableDisposable(Task<T> task)
        {
            _task = task;
        }

        /// <summary>
        /// Returns the underlying task.
        /// </summary>
        public Task<T> AsTask()
        {
            return _task;
        }

        /// <summary>
        /// Implicit conversion to the underlying task.
        /// </summary>
        /// <param name="source">The awaitable wrapper.</param>
        public static implicit operator Task<T>(AwaitableDisposable<T> source)
        {
            return source.AsTask();
        }

        /// <summary>
        /// Infrastructure. Returns the task awaiter for the underlying task.
        /// </summary>
        public TaskAwaiter<T> GetAwaiter()
        {
            return _task.GetAwaiter();
        }

        /// <summary>
        /// Infrastructure. Returns a configured task awaiter for the underlying task.
        /// </summary>
        /// <param name="continueOnCapturedContext">Whether to attempt to marshal the continuation back to the captured context.</param>
        public ConfiguredTaskAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
        {
            return _task.ConfigureAwait(continueOnCapturedContext);
        }
    }
}

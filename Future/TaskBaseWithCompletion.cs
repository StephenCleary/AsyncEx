using Nito.AsyncEx.Synchronous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// A base type for custom tasks. This task can be manually completed.
    /// </summary>
    public class TaskBaseWithCompletion : TaskBase
    {
        private TaskBaseWithCompletion(TaskCompletionSource source)
            : base(source.Task)
        {
            TaskCompletionSource = source;
        }

        /// <summary>
        /// Constructs the base task type. This task can be manually completed.
        /// </summary>
        protected TaskBaseWithCompletion()
            : this(new TaskCompletionSource())
        {
        }

        /// <summary>
        /// Gets the task completion source that can be used to complete this task. Note that the <c>Task</c> of this completion source is not the same as this task. To ensure this task is complete, call <see cref="EnsureCompleted"/> after completing this task completion source.
        /// </summary>
        protected TaskCompletionSource TaskCompletionSource { get; private set; }

        /// <summary>
        /// Ensures that this task has completed. Invoke this after completing <see cref="TaskCompletionSource"/>.
        /// </summary>
        protected void EnsureCompleted()
        {
            this.WaitWithoutException();
        }
    }

    /// <summary>
    /// A base type for custom tasks. This task can be manually completed.
    /// </summary>
    /// <typeparam name="TResult">The type of the task result.</typeparam>
    public class TaskBaseWithCompletion<TResult> : TaskBase<TResult>
    {
        private TaskBaseWithCompletion(TaskCompletionSource<TResult> source)
            : base(source.Task)
        {
            TaskCompletionSource = source;
        }

        /// <summary>
        /// Constructs the base task type. This task can be manually completed.
        /// </summary>
        protected TaskBaseWithCompletion()
            : this(new TaskCompletionSource<TResult>())
        {
        }

        /// <summary>
        /// Gets the task completion source that can be used to complete this task. Note that the <c>Task</c> of this completion source is not the same as this task. To ensure this task is complete, call <see cref="EnsureCompleted"/> after completing this task completion source.
        /// </summary>
        protected TaskCompletionSource<TResult> TaskCompletionSource { get; private set; }

        /// <summary>
        /// Ensures that this task has completed. Invoke this after completing <see cref="TaskCompletionSource"/>.
        /// </summary>
        protected void EnsureCompleted()
        {
            this.WaitWithoutException();
        }
    }
}

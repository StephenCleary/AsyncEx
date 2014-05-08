using Nito.AsyncEx.Synchronous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// A base type for custom tasks. This task will propagate the completion of its source task.
    /// </summary>
    public class TaskBase : Task
    {
        /// <summary>
        /// Constructs the base task type. This task will propagate the completion of its source task.
        /// </summary>
        /// <param name="source">The source task used to complete this task.</param>
        protected TaskBase(Task source)
            : base(s => ((Task)s).WaitAndUnwrapException(), source)
        {
            source.ContinueWith(_ => RunSynchronously(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// A base type for custom tasks. This task will propagate the completion of its source task.
    /// </summary>
    /// <typeparam name="TResult">The type of the task result.</typeparam>
    public class TaskBase<TResult> : Task<TResult>
    {
        /// <summary>
        /// Constructs the base task type. This task will propagate the completion of its source task.
        /// </summary>
        /// <param name="source">The source task used to complete this task.</param>
        protected TaskBase(Task<TResult> source)
            : base(s => ((Task<TResult>)s).WaitAndUnwrapException(), source)
        {
            source.ContinueWith(_ => RunSynchronously(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
    }
}

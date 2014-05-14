using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Nito.AsyncEx
{
    /// <summary>
    /// A progress implementation that sends progress reports to a dataflow block (via <see cref="DataflowBlock.Post"/>). Optionally shuts down the dataflow block when the task completes.
    /// </summary>
    /// <typeparam name="T">The type of progress value.</typeparam>
    public sealed class DataflowProgress<T> : IProgress<T>
    {
        /// <summary>
        /// The dataflow block to pass progress reports to.
        /// </summary>
        private readonly ITargetBlock<T> _block;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataflowProgress&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="block">The dataflow block to pass progress reports to. May not be <c>null</c>.</param>
        public DataflowProgress(ITargetBlock<T> block)
        {
            _block = block;
        }

        void IProgress<T>.Report(T value)
        {
            _block.Post(value);
        }

        /// <summary>
        /// Watches the task, and shuts down the dataflow block (via <see cref="IDataflowBlock.Fault"/> or <see cref="IDataflowBlock.Complete"/>) when the task completes.
        /// </summary>
        /// <param name="task">The task to watch. May not be <c>null</c>.</param>
        public void ObserveTaskForCompletion(Task task)
        {
            task.ContinueWith(_ =>
            {
                if (task.IsFaulted)
                    _block.Fault(task.Exception.InnerException);
                else
                    _block.Complete();
            }, TaskScheduler.Default);
        }
    }
}

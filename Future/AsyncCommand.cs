using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

// Original idea from Daniel Sklenitzka in this SO answer: http://stackoverflow.com/a/15743118/263693

namespace Nito.AsyncEx
{
    /// <summary>
    /// A command that executes an asynchronous delegate.
    /// </summary>
    public class AsyncCommand : ICommand
    {
        /// <summary>
        /// The asynchronous delegate to execute.
        /// </summary>
        private readonly Func<object, Task> _command;

        /// <summary>
        /// Creates a command that executes the specified asynchronous delegate.
        /// </summary>
        /// <param name="command">The asynchronous delegate to execute. May not be <c>null</c>.</param>
        public AsyncCommand(Func<object, Task> command)
        {
            _command = command;
        }

        /// <summary>
        /// Returns a value indicating whether the command may be executed. This implementation always returns <c>true</c>.
        /// </summary>
        /// <param name="parameter">The parameter for this command.</param>
        public virtual bool CanExecute(object parameter = null)
        {
            return true;
        }

        async void ICommand.Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">The parameter for this command.</param>
        public virtual Task ExecuteAsync(object parameter = null)
        {
            return _command(parameter);
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event. This base class never raises this event itself.
        /// </summary>
        protected void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// An event indicating that the return value of <see cref="CanExecute"/> may have changed.
        /// </summary>
        public event EventHandler CanExecuteChanged;
    }

    /// <summary>
    /// A command that executes an asynchronous delegate and returns <c>false</c> for <see cref="CanExecute"/> as long as the asynchronous delegate is executing.
    /// </summary>
    public sealed class SerializedAsyncCommand : AsyncCommand
    {
        /// <summary>
        /// Whether the asynchronous delegate is executing.
        /// </summary>
        private bool _isExecuting;

        /// <summary>
        /// Creates a command that executes the specified asynchronous delegate.
        /// </summary>
        /// <param name="command">The asynchronous delegate to execute. May not be <c>null</c>.</param>
        public SerializedAsyncCommand(Func<object, Task> command)
            : base(command)
        {
        }

        /// <summary>
        /// Returns a value indicating whether the command may be executed. The command may be executed as long as it is not already executing.
        /// </summary>
        /// <param name="parameter">The parameter for this command.</param>
        public override bool CanExecute(object parameter = null)
        {
            return !_isExecuting;
        }

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">The parameter for this command.</param>
        public override async Task ExecuteAsync(object parameter = null)
        {
            _isExecuting = true;
            RaiseCanExecuteChanged();
            var ret = base.ExecuteAsync(parameter);
            try
            {
                await ret;
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }
    }
}

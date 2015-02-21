using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// Watches a task and raises property-changed notifications when the task completes.
    /// </summary>
    public interface INotifyTaskCompletion : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the task being watched. This property never changes and is never <c>null</c>.
        /// </summary>
        Task Task { get; }

        /// <summary>
        /// Gets a task that completes successfully when <see cref="Task"/> completes (successfully, faulted, or canceled). This property never changes and is never <c>null</c>.
        /// </summary>
        Task TaskCompleted { get; }

        /// <summary>
        /// Gets the current task status. This property raises a notification when the task completes.
        /// </summary>
        TaskStatus Status { get; }

        /// <summary>
        /// Gets whether the task has completed. This property raises a notification when the value changes to <c>true</c>.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Gets whether the task is busy (not completed). This property raises a notification when the value changes to <c>false</c>.
        /// </summary>
        bool IsNotCompleted { get; }

        /// <summary>
        /// Gets whether the task has completed successfully. This property raises a notification when the value changes to <c>true</c>.
        /// </summary>
        bool IsSuccessfullyCompleted { get; }

        /// <summary>
        /// Gets whether the task has been canceled. This property raises a notification only if the task is canceled (i.e., if the value changes to <c>true</c>).
        /// </summary>
        bool IsCanceled { get; }

        /// <summary>
        /// Gets whether the task has faulted. This property raises a notification only if the task faults (i.e., if the value changes to <c>true</c>).
        /// </summary>
        bool IsFaulted { get; }

        /// <summary>
        /// Gets the wrapped faulting exception for the task. Returns <c>null</c> if the task is not faulted. This property raises a notification only if the task faults (i.e., if the value changes to non-<c>null</c>).
        /// </summary>
        AggregateException Exception { get; }

        /// <summary>
        /// Gets the original faulting exception for the task. Returns <c>null</c> if the task is not faulted. This property raises a notification only if the task faults (i.e., if the value changes to non-<c>null</c>).
        /// </summary>
        Exception InnerException { get; }

        /// <summary>
        /// Gets the error message for the original faulting exception for the task. Returns <c>null</c> if the task is not faulted. This property raises a notification only if the task faults (i.e., if the value changes to non-<c>null</c>).
        /// </summary>
        string ErrorMessage { get; }
    }

    /// <summary>
    /// Watches a task and raises property-changed notifications when the task completes.
    /// </summary>
    /// <typeparam name="TResult">The type of the task result.</typeparam>
    public interface INotifyTaskCompletion<TResult> : INotifyTaskCompletion
    {
        /// <summary>
        /// Gets the task being watched. This property never changes and is never <c>null</c>.
        /// </summary>
        new Task<TResult> Task { get; }

        /// <summary>
        /// Gets the result of the task. Returns the default value of <typeparamref name="TResult"/> if the task has not completed successfully. This property raises a notification when the task completes successfully.
        /// </summary>
        TResult Result { get; }
    }

    /// <summary>
    /// Factory for task completion notifiers.
    /// </summary>
    public static class NotifyTaskCompletion
    {
        /// <summary>
        /// Creates a new task notifier watching the specified task.
        /// </summary>
        /// <param name="task">The task to watch.</param>
        /// <returns>A new task notifier watching the specified task.</returns>
        public static INotifyTaskCompletion Create(Task task)
        {
            return new NotifyTaskCompletionImplementation(task);
        }

        /// <summary>
        /// Creates a new task notifier watching the specified task.
        /// </summary>
        /// <typeparam name="TResult">The type of the task result.</typeparam>
        /// <param name="task">The task to watch.</param>
        /// <returns>A new task notifier watching the specified task.</returns>
        public static INotifyTaskCompletion<TResult> Create<TResult>(Task<TResult> task)
        {
            return new NotifyTaskCompletionImplementation<TResult>(task);
        }

        /// <summary>
        /// Executes the specified asynchronous code and creates a new task notifier watching the returned task.
        /// </summary>
        /// <param name="asyncAction">The asynchronous code to execute.</param>
        /// <returns>A new task notifier watching the returned task.</returns>
        public static INotifyTaskCompletion Create(Func<Task> asyncAction)
        {
            return Create(asyncAction());
        }

        /// <summary>
        /// Executes the specified asynchronous code and creates a new task notifier watching the returned task.
        /// </summary>
        /// <param name="asyncAction">The asynchronous code to execute.</param>
        /// <returns>A new task notifier watching the returned task.</returns>
        public static INotifyTaskCompletion<TResult> Create<TResult>(Func<Task<TResult>> asyncAction)
        {
            return Create(asyncAction());
        }

        /// <summary>
        /// Watches a task and raises property-changed notifications when the task completes.
        /// </summary>
        private sealed class NotifyTaskCompletionImplementation : INotifyTaskCompletion
        {
            /// <summary>
            /// Initializes a task notifier watching the specified task.
            /// </summary>
            /// <param name="task">The task to watch.</param>
            public NotifyTaskCompletionImplementation(Task task)
            {
                Task = task;
                if (task.IsCompleted)
                {
                    TaskCompleted = TaskConstants.Completed;
                    return;
                }

                var scheduler = (SynchronizationContext.Current == null) ? TaskScheduler.Current : TaskScheduler.FromCurrentSynchronizationContext();
                TaskCompleted = task.ContinueWith(t =>
                {
                    var propertyChanged = PropertyChanged;
                    if (propertyChanged == null)
                        return;

                    propertyChanged(this, new PropertyChangedEventArgs("Status"));
                    propertyChanged(this, new PropertyChangedEventArgs("IsCompleted"));
                    propertyChanged(this, new PropertyChangedEventArgs("IsNotCompleted"));
                    if (t.IsCanceled)
                    {
                        propertyChanged(this, new PropertyChangedEventArgs("IsCanceled"));
                    }
                    else if (t.IsFaulted)
                    {
                        propertyChanged(this, new PropertyChangedEventArgs("IsFaulted"));
                        propertyChanged(this, new PropertyChangedEventArgs("Exception"));
                        propertyChanged(this, new PropertyChangedEventArgs("InnerException"));
                        propertyChanged(this, new PropertyChangedEventArgs("ErrorMessage"));
                    }
                    else
                    {
                        propertyChanged(this, new PropertyChangedEventArgs("IsSuccessfullyCompleted"));
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                scheduler);
            }

            public Task Task { get; private set; }
            public Task TaskCompleted { get; private set; }
            public TaskStatus Status { get { return Task.Status; } }
            public bool IsCompleted { get { return Task.IsCompleted; } }
            public bool IsNotCompleted { get { return !Task.IsCompleted; } }
            public bool IsSuccessfullyCompleted { get { return Task.Status == TaskStatus.RanToCompletion; } }
            public bool IsCanceled { get { return Task.IsCanceled; } }
            public bool IsFaulted { get { return Task.IsFaulted; } }
            public AggregateException Exception { get { return Task.Exception; } }
            public Exception InnerException { get { return (Exception == null) ? null : Exception.InnerException; } }
            public string ErrorMessage { get { return (InnerException == null) ? null : InnerException.Message; } }
            
            public event PropertyChangedEventHandler PropertyChanged;
        }

        /// <summary>
        /// Watches a task and raises property-changed notifications when the task completes.
        /// </summary>
        /// <typeparam name="TResult">The type of the task result.</typeparam>
        private sealed class NotifyTaskCompletionImplementation<TResult> : INotifyTaskCompletion<TResult>
        {
            /// <summary>
            /// Initializes a task notifier watching the specified task.
            /// </summary>
            /// <param name="task">The task to watch.</param>
            public NotifyTaskCompletionImplementation(Task<TResult> task)
            {
                Task = task;
                if (task.IsCompleted)
                {
                    TaskCompleted = TaskConstants.Completed;
                    return;
                }

                var scheduler = (SynchronizationContext.Current == null) ? TaskScheduler.Current : TaskScheduler.FromCurrentSynchronizationContext();
                TaskCompleted = task.ContinueWith(t =>
                {
                    var propertyChanged = PropertyChanged;
                    if (propertyChanged == null)
                        return;

                    propertyChanged(this, new PropertyChangedEventArgs("Status"));
                    propertyChanged(this, new PropertyChangedEventArgs("IsCompleted"));
                    propertyChanged(this, new PropertyChangedEventArgs("IsNotCompleted"));
                    if (t.IsCanceled)
                    {
                        propertyChanged(this, new PropertyChangedEventArgs("IsCanceled"));
                    }
                    else if (t.IsFaulted)
                    {
                        propertyChanged(this, new PropertyChangedEventArgs("IsFaulted"));
                        propertyChanged(this, new PropertyChangedEventArgs("Exception"));
                        propertyChanged(this, new PropertyChangedEventArgs("InnerException"));
                        propertyChanged(this, new PropertyChangedEventArgs("ErrorMessage"));
                    }
                    else
                    {
                        propertyChanged(this, new PropertyChangedEventArgs("IsSuccessfullyCompleted"));
                        propertyChanged(this, new PropertyChangedEventArgs("Result"));
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                scheduler);
            }

            public Task<TResult> Task { get; private set; }
            Task INotifyTaskCompletion.Task { get { return Task; } }
            public Task TaskCompleted { get; private set; }
            public TResult Result { get { return (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : default(TResult); } }
            public TaskStatus Status { get { return Task.Status; } }
            public bool IsCompleted { get { return Task.IsCompleted; } }
            public bool IsNotCompleted { get { return !Task.IsCompleted; } }
            public bool IsSuccessfullyCompleted { get { return Task.Status == TaskStatus.RanToCompletion; } }
            public bool IsCanceled { get { return Task.IsCanceled; } }
            public bool IsFaulted { get { return Task.IsFaulted; } }
            public AggregateException Exception { get { return Task.Exception; } }
            public Exception InnerException { get { return (Exception == null) ? null : Exception.InnerException; } }
            public string ErrorMessage { get { return (InnerException == null) ? null : InnerException.Message; } }

            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
}

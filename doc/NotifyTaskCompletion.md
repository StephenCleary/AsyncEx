## Overview

The `NotifyTaskCompletion` types enable using `Task`s in situations where you need to support data binding. Properties on the notifier type will update when the task completes.

You can create an `INotifyTaskCompletion` or `INotifyTaskCompletion<TResult>` by calling `NotifyTaskCompletion.Create` and passing in the `Task` to observe.

## API

```C#
// Watches a task and raises property-changed notifications when the task completes.
public interface INotifyTaskCompletion : INotifyPropertyChanged
{
  // Gets the task being watched.
  // This property never changes and is never null.
  Task Task { get; }

  // Gets the current task status.
  // This property raises a notification when the task completes.
  TaskStatus Status { get; }

  // Gets whether the task has completed.
  // This property raises a notification when the value changes to true.
  bool IsCompleted { get; }

  // Gets whether the task has completed successfully.
  // This property raises a notification only if the task completes successfully (i.e., if the value changes to true).
  bool IsSuccessfullyCompleted { get; }

  // Gets whether the task has been canceled.
  // This property raises a notification only if the task is canceled (i.e., if the value changes to true).
  bool IsCanceled { get; }

  // Gets whether the task has faulted.
  // This property raises a notification only if the task faults (i.e., if the value changes to true).
  bool IsFaulted { get; }

  // Gets the wrapped faulting exception for the task.
  // Returns null if the task is not faulted.
  // This property raises a notification only if the task faults (i.e., if the value changes to non-null).
  AggregateException Exception { get; }

  // Gets the original faulting exception for the task.
  // Returns null if the task is not faulted.
  // This property raises a notification only if the task faults (i.e., if the value changes to non-null).
  Exception InnerException { get; }

  // Gets the error message for the original faulting exception for the task.
  // Returns null if the task is not faulted.
  // This property raises a notification only if the task faults (i.e., if the value changes to non-null).
  string ErrorMessage { get; }
}

// Watches a task and raises property-changed notifications when the task completes.
public interface INotifyTaskCompletion<TResult> : INotifyTaskCompletion
{
  // Gets the task being watched.
  // This property never changes and is never <c>null</c>.
  new Task<TResult> Task { get; }

  // Gets the result of the task.
  // Returns the default value of <typeparamref name="TResult"/> if the task has not completed successfully.
  // This property raises a notification when the task completes successfully.
  TResult Result { get; }
}

// Factory for task completion notifiers.
public static class NotifyTaskCompletion
{
  // Creates a new task notifier watching the specified task.
  public static INotifyTaskCompletion Create(Task task);

  // Creates a new task notifier watching the specified task.
  public static INotifyTaskCompletion<TResult> Create<TResult>(Task<TResult> task);
}
```

## Platform Support

The full API is supported on all platforms.
## Overview

`TaskCompletionSource` is a [[TaskCompletionSource<T>|http://msdn.microsoft.com/en-us/library/dd449174.aspx]] for tasks without a return type.

## API

```C#
// Represents the producer side of a Task unbound to a delegate, providing access to the consumer side through the Task property.
public sealed class TaskCompletionSource
{
  // Initializes a new instance of the TaskCompletionSource class.
  public TaskCompletionSource();

  // Initializes a new instance of the TaskCompletionSource class with the specified state.
  public TaskCompletionSource(object state);

  // Initializes a new instance of the TaskCompletionSource class with the specified options.
  public TaskCompletionSource(TaskCreationOptions creationOptions);

  // Initializes a new instance of the TaskCompletionSource class with the specified state and options.
  public TaskCompletionSource(object state, TaskCreationOptions creationOptions);

  // Gets the Task created by this TaskCompletionSource.
  public Task Task { get; }

  // Transitions the underlying Task into the TaskStatus.Canceled state.
  public void SetCanceled();

  // Attempts to transition the underlying Task into the TaskStatus.Canceled state.
  public bool TrySetCanceled();

  // Transitions the underlying Task into the TaskStatus.Faulted state.
  public void SetException(Exception exception);

  // Transitions the underlying Task into the TaskStatus.Faulted state.
  public void SetException(IEnumerable<Exception> exceptions);

  // Attempts to transition the underlying Task into the TaskStatus.Faulted state.
  public bool TrySetException(Exception exception);

  // Attempts to transition the underlying Task into the TaskStatus.Faulted state.
  public bool TrySetException(IEnumerable<Exception> exceptions);

  // Transitions the underlying Task into the TaskStatus.RanToCompletion state.
  public void SetResult();

  // Attempts to transition the underlying Task into the TaskStatus.RanToCompletion state.
  public bool TrySetResult();
}
```

## Platform Support

The full API is supported on all platforms.
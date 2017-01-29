## Overview

`TaskCompletionSourceExtensions` provides extension methods for the [[TaskCompletionSource]] and `TaskCompletionSource<T>` types.

To forward the result of one `Task` to another, call `TryCompleteFromCompletedTask`.

`TryCompleteFromEventArgs` can be used to help interoperate with the Event-based Asynchronous Pattern.

The remaining methods `TrySetResultWithBackgroundContinuations`, `TrySetCanceledWithBackgroundContinuations`, and `TrySetExceptionWithBackgroundContinuations` all attempt to transition the underlying `Task` to the appropriate state, but execute `Task` continuations on a thread pool thread, even if they were attached using `TaskContinuationOperations.ExecuteSynchronously`. This is necessary if you are completing a task while holding a lock.

## API

```C#
// Provides extension methods for TaskCompletionSource.
public static class TaskCompletionSourceExtensions
{
  // Attempts to complete a TaskCompletionSource, propagating the completion of "task".
  public static bool TryCompleteFromCompletedTask<TResult, TSourceResult>(this TaskCompletionSource<TResult> @this, Task<TSourceResult> task) where TSourceResult : TResult;

  // Attempts to complete a TaskCompletionSource, propagating the completion of "task".
  public static bool TryCompleteFromCompletedTask(this TaskCompletionSource @this, Task task);

  // Attempts to complete a TaskCompletionSource, propogating the completion of "eventArgs".
  public static bool TryCompleteFromEventArgs<TResult>(this TaskCompletionSource<TResult> @this, AsyncCompletedEventArgs eventArgs, Func<TResult> getResult);

  // Attempts to complete a TaskCompletionSource, propogating the completion of "eventArgs".
  public static bool TryCompleteFromEventArgs(this TaskCompletionSource @this, AsyncCompletedEventArgs eventArgs);

  // Attempts to complete a TaskCompletionSource with the specified value, forcing all continuations onto a threadpool thread even if they specified ExecuteSynchronously.
  public static void TrySetResultWithBackgroundContinuations<TResult>(this TaskCompletionSource<TResult> @this, TResult result);

  // Attempts to complete a TaskCompletionSource, forcing all continuations onto a threadpool thread even if they specified ExecuteSynchronously.
  public static void TrySetResultWithBackgroundContinuations(this TaskCompletionSource @this);

  // Attempts to complete a TaskCompletionSource as canceled, forcing all continuations onto a threadpool thread even if they specified ExecuteSynchronously.
  public static void TrySetCanceledWithBackgroundContinuations<TResult>(this TaskCompletionSource<TResult> @this);

  // Attempts to complete a TaskCompletionSource as canceled, forcing all continuations onto a threadpool thread even if they specified ExecuteSynchronously.
  public static void TrySetCanceledWithBackgroundContinuations(this TaskCompletionSource @this);

  // Attempts to complete a TaskCompletionSource as faulted, forcing all continuations onto a threadpool thread even if they specified ExecuteSynchronously.
  public static void TrySetExceptionWithBackgroundContinuations<TResult>(this TaskCompletionSource<TResult> @this, Exception exception);

  // Attempts to complete a TaskCompletionSource as faulted, forcing all continuations onto a threadpool thread even if they specified ExecuteSynchronously.
  public static void TrySetExceptionWithBackgroundContinuations(this TaskCompletionSource @this, Exception exception);

  // Attempts to complete a TaskCompletionSource as faulted, forcing all continuations onto a threadpool thread even if they specified ExecuteSynchronously.
  public static void TrySetExceptionWithBackgroundContinuations<TResult>(this TaskCompletionSource<TResult> @this, IEnumerable<Exception> exceptions);

  // Attempts to complete a TaskCompletionSource as faulted, forcing all continuations onto a threadpool thread even if they specified ExecuteSynchronously.
  public static void TrySetExceptionWithBackgroundContinuations(this TaskCompletionSource @this, IEnumerable<Exception> exceptions);
}
```

## Platform Support

The full API is supported on all platforms.
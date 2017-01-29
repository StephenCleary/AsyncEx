## Overview

`TaskExtensions` provides one method `OrderByCompletion` which orders a sequence of Tasks by when they complete. The approach taken by AsyncEx is a combination of [[Jon Skeet's approach|https://codeblog.jonskeet.uk/2012/01/16/eduasync-part-19-ordering-by-completion-ahead-of-time/]] and [[Stephen Toub's approach|http://blogs.msdn.com/b/pfxteam/archive/2012/08/02/processing-tasks-as-they-complete.aspx]].

`TaskExtensions` in the `Nito.AsyncEx.Synchronous` namespace provides a handful of extension methods that enable synchronous blocking of a `Task` without wrapping its exception in an `AggregateException`, or without observing the `Task` exception at all. These are advanced methods that should probably not be used, since you [[run the risk of a deadlock|http://blog.stephencleary.com/2012/07/dont-block-on-async-code.html]].

## API

```C#
public static class TaskExtensions
{
  // Creates a new array of tasks which complete in order.
  public static Task<T>[] OrderByCompletion<T>(this IEnumerable<Task<T>> tasks);
}
```

```C#
// (in namespace Nito.AsyncEx.Synchronous):
public static class TaskExtensions
{
  // Waits for the task to complete, unwrapping any exceptions.
  public static void WaitAndUnwrapException(this Task task);

  // Waits for the task to complete, unwrapping any exceptions.
  public static void WaitAndUnwrapException(this Task task, CancellationToken cancellationToken);

  // Waits for the task to complete, unwrapping any exceptions.
  public static TResult WaitAndUnwrapException<TResult>(this Task<TResult> task);

  // Waits for the task to complete, unwrapping any exceptions.
  public static TResult WaitAndUnwrapException<TResult>(this Task<TResult> task, CancellationToken cancellationToken);

  // Waits for the task to complete, but does not raise task exceptions. The task exception (if any) is unobserved.
  public static void WaitWithoutException(this Task task);

  // Waits for the task to complete, but does not raise task exceptions. The task exception (if any) is unobserved.
  public static void WaitWithoutException(this Task task, CancellationToken cancellationToken);
}
```

## Platform Support

The full API is supported on all platforms.
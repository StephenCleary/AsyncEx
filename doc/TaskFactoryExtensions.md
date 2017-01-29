## Overview

`TaskFactoryExtensions` provides a series of `Run` overloads for scheduling `async` as well as synchronous delegates.

## API

```C#
public static class TaskFactoryExtensions
{
  // Queues work to the task factory and returns a task representing that work.
  public static Task Run(this TaskFactory @this, Action action);

  // Queues work to the task factory and returns a task representing that work.
  public static Task<TResult> Run<TResult>(this TaskFactory @this, Func<TResult> action);

  // Queues work to the task factory and returns a proxy task representing that work.
  public static Task Run(this TaskFactory @this, Func<Task> action);

  // Queues work to the task factory and returns a proxy task representing that work.
  public static Task<TResult> Run<TResult>(this TaskFactory @this, Func<Task<TResult>> action);

  // Queues work to the task factory and returns a task representing that work.
  public static Task Run(this TaskFactory @this, Action action, CancellationToken cancellationToken);

  // Queues work to the task factory and returns a task representing that work.
  public static Task<TResult> Run<TResult>(this TaskFactory @this, Func<TResult> action, CancellationToken cancellationToken);

  // Queues work to the task factory and returns a proxy task representing that work.
  public static Task Run(this TaskFactory @this, Func<Task> action, CancellationToken cancellationToken);

  // Queues work to the task factory and returns a proxy task representing that work.
  public static Task<TResult> Run<TResult>(this TaskFactory @this, Func<Task<TResult>> action, CancellationToken cancellationToken);
}
```

## Platform Support

The full API is supported on all platforms.

## Overview

The `AsyncContext` type provides a _context_ for executing asynchronous operations. The `await` keyword requires a _context_ to return back to; for most programs, this is a UI context, and you don't have to worry about it. ASP.NET also provides a proper context and you don't have to use your own.

However, Console applications and Win32 services do not have a suitable context, and `AsyncContext` or `AsyncContextThread` could be used in those situations.

`AsyncContextThread` is a separate thread or task that runs an `AsyncContext`. `AsyncContextThread` does not derive from the `Thread` class. The thread begins running its `AsyncContext` immediately after creation.

`AsyncContextThread` will stay in its loop until it is requested to exit by another thread calling `JoinAsync`. Disposing an `AsyncContextThread` will also request it to exit, but will not wait for it to do so.

Normally, `AsyncContextThread` is used by windows services, but it may be used by other applications that need an independent thread with an `AsyncContext`.

## Console Example Using AsyncContext

When using `AsyncContext`, you normally just call the static `Run` method, as such:

```C#
class Program
{
  static async Task<int> AsyncMain()
  {
    ..
  }

  static int Main(string[] args)
  {
    return AsyncContext.Run(AsyncMain);
  }
}
```

The `Run` method will return when all asynchronous operations have been completed. Any exceptions will be unwrapped and propagated.

## AsyncContext API

```C#
// Provides a context for asynchronous operations. This class is threadsafe.
sealed class AsyncContext : IDisposable
{
  // Queues an action for execution, and begins executing all actions in the queue.
  // This method returns when all actions have been completed and the outstanding asynchronous operation count is zero.
  // Returns the result of the task. This method will unwrap and propagate exceptions.
  public static void Run(Action action);
  public static TResult Run<TResult>(Func<TResult> action);
  public static void Run(Func<Task> action);
  public static TResult Run<TResult>(Func<Task<TResult>> action);

  // Gets a semi-unique identifier for this asynchronous context. This is the same identifier as the context's TaskScheduler.
  public int Id { get; }

  // Gets the current AsyncContext for this thread, or <c>null</c> if this thread is not currently running in an AsyncContext.
  public static AsyncContext Current { get; }
```

## AsyncContextThread API

Unlike `AsyncContext`, `AsyncContextThread` provides properties that can be used to schedule tasks on that thread.

The `Context` property returns the `AsyncContext` being run. The `Factory` property returns a `TaskFactory` which can be used to queue work to the thread.

```C#
public sealed class AsyncContextThread : IDisposable
{
  // Initializes a new instance of the AsyncContextThread class, creating a child thread waiting for commands.
  public AsyncContextThread();

  // Gets the AsyncContext executed by this thread.
  public AsyncContext Context { get; }

  // Gets the TaskFactory for this thread, which can be used to schedule work to this thread.
  public TaskFactory Factory { get; }

  // Requests the thread to exit and returns a task representing the exit of the thread. The thread will exit when all outstanding asynchronous operations complete.
  public Task JoinAsync();

  // Requests the thread to exit.
  public void Dispose();
}
```

## Platform Support

This type is available on all platforms, but may not work due to security restrictions. This is particularly a problem on Silverlight and Windows Phone platforms.

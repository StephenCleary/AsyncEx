## Overview

The `AsyncContext` type provides a _context_ for executing asynchronous operations. The `await` keyword requires a _context_ to return back to. For most client programs, this is a UI context; for most server programs, this is a thread pool context. In most cases, you don't have to worry about it.

However, Console applications and Win32 services use the thread pool context, and `AsyncContext` or `AsyncContextThread` could be useful in those situations.

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

## AsyncContextThread

Unlike `AsyncContext`, `AsyncContextThread` provides properties that can be used to schedule tasks on that thread.

The `Context` property returns the `AsyncContext` being run. The `Factory` property returns a `TaskFactory` which can be used to queue work to the thread.

## Platform Support

This type is available on all platforms, but may not work due to security restrictions. This is particularly a problem on Silverlight and Windows Phone platforms.

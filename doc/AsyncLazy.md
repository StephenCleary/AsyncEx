## Overview

The `AsyncLazy<T>` type enables [[asynchronous lazy initialization|http://blog.stephencleary.com/2012/08/asynchronous-lazy-initialization.html]], similar to [[Stephen Toub's AsyncLazy|http://blogs.msdn.com/b/pfxteam/archive/2011/01/15/10116210.aspx]].

An `AsyncLazy<T>` instance is constructed with a factory method (which may be synchronous or asynchronous). When the `AsyncLazy<T>` instance is `await`ed or its `Start` method is called, the factory method starts on a thread pool thread. The factory method is only executed once. Once the factory method has completed, all future `await`s on that instance complete immediately.

## API

```C#
// Provides support for asynchronous lazy initialization. This type is fully threadsafe.
public sealed class AsyncLazy<T>
{
  // Initializes a new instance of the AsyncLazy<T> class.
  public AsyncLazy(Func<T> factory);

  // Initializes a new instance of the AsyncLazy<T> class.
  public AsyncLazy(Func<Task<T>> factory);

  // Gets a semi-unique identifier for this asynchronous lazy instance.
  public int Id { get; }

  // Asynchronous infrastructure support.
  // This method permits instances of AsyncLazy<T> to be await'ed.
  public <unspecified> GetAwaiter();

  // Starts the asynchronous initialization, if it has not already started.
  public void Start();
}
```

## Platform Support

The full API is supported on all platforms.
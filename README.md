![Logo](AsyncEx.128.png)

# AsyncEx

A helper library for async/await.

Supports .NET 4.5/4.0, iOS, Android, Windows Store 8.1, Windows Phone Silverlight 8.0, Windows Phone Applications 8.1, Silverlight 5.0, and all portable libraries thereof.

Note: iOS applications need to add a special line of code to prevent aggresive compiler optimizations:

    EnlightenmentVerification.EnsureLoaded();

## Getting Started

Install the [NuGet package](http://www.nuget.org/packages/Nito.AsyncEx).

## AsyncLock

A lot of developers start using this library for `AsyncLock`, an async-compatible mutual exclusion mechanism. Using `AsyncLock` is straightforward:

    private readonly AsyncLock _mutex = new AsyncLock();
    public async Task UseLockAsync()
    {
      // AsyncLock can be locked asynchronously
      using (await _mutex.LockAsync())
      {
        // It's safe to await while the lock is held
        await Task.Delay(TimeSpan.FromSeconds(1));
      }
    }

`AsyncLock` also fully supports cancellation:

    public async Task UseLockAsync()
    {
      // Attempt to take the lock only for 2 seconds.
      var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
      
      // If the lock isn't available after 2 seconds, this will
      //  raise OperationCanceledException.
      using (await _mutex.LockAsync(cts.Token))
      {
        await Task.Delay(TimeSpan.FromSeconds(1));
      }
    }

`AsyncLock` also has a synchronous API. This permits some threads to acquire the lock asynchronously while other threads acquire the lock synchronously (blocking the thread).

    public async Task UseLockAsync()
    {
      using (await _mutex.LockAsync())
      {
        await Task.Delay(TimeSpan.FromSeconds(1));
      }
    }

    public void UseLock()
    {
      using (_mutex.Lock())
      {
        Thread.Sleep(TimeSpan.FromSeconds(1));
      }
    }

## Other Coordination Primitives

`AsyncLock` is just the beginning. The AsyncEx library contains a full suite of coordination primitives: `AsyncManualResetEvent`, `AsyncAutoResetEvent`, `AsyncConditionVariable`, `AsyncMonitor`, `AsyncSemaphore`, `AsyncCountdownEvent`, `AsyncBarrier`, and `AsyncReaderWriterLock`.

## MVVM Support

`NotifyTaskCompletion` is a data-binding-friendly wrapper for a Task, raising property-change notifications when the task completes. `PropertyProgress` exposes progress updates as property-change events.

## More Stuff

There's quite a few other helpful types; see [the docs for full details](doc/Home.md)

## Infrequently Asked Questions

### Strong Naming

Need strong-naming? Use [the assembly strong naming toolkit](https://www.nuget.org/packages/Nivot.StrongNaming/1.0.4.2).

### Older Platforms

AsyncEx v3 supported Windows Store 8.0, Windows Phone Silverlight 7.5, and Silverlight 4.0. Support for these platforms has been dropped with AsyncEx v4.

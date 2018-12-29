![Logo](AsyncEx.128.png)

# AsyncEx

A helper library for async/await.

Note: This README is for AsyncEx v5 (prerelease). For AsyncEx v4 (the current version), see [here](https://github.com/StephenCleary/AsyncEx/tree/v4). **If you are seeing an upgrade error when upgrading from v3 to v4, see [this documentation](https://github.com/StephenCleary/AsyncEx/blob/v4/doc/pcl-upgrade.md)**.

Supports `netstandard1.3` (including .NET 4.6, .NET Core 1.0, Xamarin.iOS 10, Xamarin.Android 7, Mono 4.6, and Universal Windows 10).

[![AppVeyor](https://img.shields.io/appveyor/ci/StephenCleary/AsyncEx.svg?style=plastic)](https://ci.appveyor.com/project/StephenCleary/AsyncEx) [![Coveralls](https://img.shields.io/coveralls/StephenCleary/AsyncEx.svg?style=plastic)](https://coveralls.io/r/StephenCleary/AsyncEx)
[![NuGet Pre Release](https://img.shields.io/nuget/vpre/Nito.AsyncEx.svg?style=plastic)](https://www.nuget.org/packages/Nito.AsyncEx/)

[API Docs](http://dotnetapis.com/pkg/Nito.AsyncEx) - [Overview](doc/Home.md) - [Upgrade Guide](doc/upgrade.md)

## Getting Started

Install the [NuGet package](http://www.nuget.org/packages/Nito.AsyncEx).

## AsyncLock

A lot of developers start using this library for `AsyncLock`, an async-compatible mutual exclusion mechanism. Using `AsyncLock` is straightforward:

```C#
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
```

`AsyncLock` also fully supports cancellation:

```C#
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
```

`AsyncLock` also has a synchronous API. This permits some threads to acquire the lock asynchronously while other threads acquire the lock synchronously (blocking the thread).

```C#
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
```

## Other Coordination Primitives

`AsyncLock` is just the beginning. The AsyncEx library contains a full suite of coordination primitives: `AsyncManualResetEvent`, `AsyncAutoResetEvent`, `AsyncConditionVariable`, `AsyncMonitor`, `AsyncSemaphore`, `AsyncCountdownEvent`, and `AsyncReaderWriterLock`.

## More Stuff

There's quite a few other helpful types; see [the docs for full details](doc)

## Infrequently Asked Questions

### Older Platforms

[AsyncEx v4](https://github.com/StephenCleary/AsyncEx/tree/v4) supported .NET 4.0, Windows Store 8.1, Windows Phone Silverlight 8.0, Windows Phone Applications 8.1, and Silverlight 5.0. Support for these platforms has been dropped with AsyncEx v5.

AsyncEx v3 supported Windows Store 8.0, Windows Phone Silverlight 7.5, and Silverlight 4.0. Support for these platforms has been dropped with AsyncEx v4.

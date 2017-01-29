## Overview

The `CancellationTokenExtensions` type provides a single extension method for `CancellationToken`: `AsTask`, which returns a task that will enter the `Canceled` state when the `CancellationToken` is signaled. If the `CancellationToken` is never signaled, the returned task will never complete.

## API

```C#
public static class CancellationTokenExtensions
{
  // Returns a Task that is canceled when this CancellationToken is canceled.
  public static Task AsTask(this CancellationToken cancellationToken);
}
```

## Platform Support

The full API is supported on all platforms.
## Overview

`SynchronizationContextHelpers` provides a property and a type to assist working with [[SynchronizationContext|http://msdn.microsoft.com/en-us/library/System.Threading.SynchronizationContext.aspx]].

`CurrentOrDefault` returns the current `SynchronizationContext` unless it is `null`, in which case it returns a new default `SynchronizationContext` instance (representing the thread pool).

`SynchronizationContextSwitcher` temporarily replaces the current `SynchronizationContext`, and reinstates it when disposed.

## API

```C#
// Provides helper types for SynchronizationContext.
public static class SynchronizationContextHelpers
{
  // Retrieves the current synchronization context, or the default synchronization context if there is no current synchronization context.
  public static SynchronizationContext CurrentOrDefault { get; }

  // Utility class for temporarily switching SynchronizationContext implementations.
  public sealed class SynchronizationContextSwitcher : IDisposable
  {
    // Initializes a new instance of the SynchronizationContextSwitcher class, installing the new SynchronizationContext.
    public SynchronizationContextSwitcher(SynchronizationContext newContext);
  }
}
```

## Platform Support

The full API is available on all platforms; however, the `SynchronizationContextSwitcher` may not work due to security restrictions. This is particularly a problem on Silverlight and Windows Phone platforms.

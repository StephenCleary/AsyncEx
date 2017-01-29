## Overview

Task constants provide pre-constructed constant task values. These are effective when using the [[async fast path|http://blogs.msdn.com/b/lucian/archive/2011/04/15/async-ctp-refresh-design-changes.aspx]].

## API

```C#
public static class TaskConstants
{
  // A task that has been completed with the value "false".
  public static Task<bool> BooleanFalse { get; }

  // A task that has been completed with the value "true".
  public static Task<bool> BooleanTrue { get; }

  // A task that has been completed with the value "-1".
  public static Task<int> Int32NegativeOne { get; }

  // A task that has been completed with the value "0".
  public static Task<int> Int32Zero { get; }

  // A task that has been completed.
  public static Task Completed { get; }

  // A task that will never complete.
  public static Task Never { get; }

  // A task that has been canceled.
  public static Task Canceled { get; }
}

public static class TaskConstants<T>
{
  // A task that has been completed with the default value of "T".
  public static Task<T> Default { get; }

  // A task that will never complete.
  public static Task<T> Never { get; }

  // A task that has been canceled.
  public static Task<T> Canceled { get; }
}
```

## Platform Support

The full API is supported on all platforms.
## Overview

`DataflowProgress` is an [[IProgress<T>|http://msdn.microsoft.com/en-us/library/hh138298.aspx]] implementation that sends progress reports to a dataflow block (via `Post`).

`DataflowProgress` also supports `ObserveTaskForCompletion`, which will complete the dataflow block (via `Complete` or `Fault`) when the `Task` completes.

## API

```C#
// A progress implementation that sends progress reports to a dataflow block.
// Optionally shuts down the dataflow block when the task completes.
public sealed class DataflowProgress<T> : IProgress<T>
{
  // Initializes a new instance of the DataflowProgress<T> class.
  public DataflowProgress(ITargetBlock<T> block);

  // Watches the task, and shuts down the dataflow block when the task completes.
  public void ObserveTaskForCompletion(Task task);
}
```

## Platform Support

This type is not supported on the following platforms:
* Windows Phone Silverlight 7.5.
* iOS.
* Android.
* Silverlight.

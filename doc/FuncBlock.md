## Overview

`FuncBlock` is a Dataflow block that receives data from a constantly-executing `async` method.

The `async` method for a `FuncBlock` is executed on the thread pool. If the method returns or throws `OperationCanceledException`, then the `FuncBlock` is completed. If the method throws some other exception, then the `FuncBlock` is faulted.

The first argument to the `async` method is a `SendAsync` delegate that looks like `Task SendAsync(T item);` that it can use to asynchronously send data to the `FuncBlock`. The optional second argument to the `async` method is a `CancellationToken` that is signaled when the `async` method should exit.

If the `FuncBlock` is requested to cancel (by signaling the `CancellationToken` member of its `DataflowBlockOptions`), then the block is canceled and then the `async` method is canceled by setting its `CancellationToken` and forcing `SendAsync` to throw `OperationCanceledException`.

If the `FuncBlock` is requested to complete (by calling `Complete`), then the block is completed and then the `async` method is canceled by setting its `CancellationToken` and forcing `SendAsync` to throw `OperationCanceledException`.

If the `FuncBlock` is requested to fault (by calling `Fault`), then the block is faulted and then the `async` method is canceled by setting its `CancellationToken` and forcing `SendAsync` to throw `OperationCanceledException`.

## API

```C#
// A dataflow block which uses a function to generate its items.
public sealed class FuncBlock<T> : IReceivableSourceBlock<T>, IDisposable
{
  // Initializes the block with the specified options and a function which takes a cancellation token.
  public FuncBlock(Func<Func<T, Task>, CancellationToken, Task> function, DataflowBlockOptions dataflowBlockOptions = null);

  // Initializes the block with the specified options and a function which does not take a cancellation token.
  public FuncBlock(Func<Func<T, Task>, Task> function, DataflowBlockOptions dataflowBlockOptions = null);

  // Gets the name for this block.
  public string Name { get; }

  // Attempts to synchronously receive an available output item from the block.
  public bool TryReceive(Predicate<T> filter, out T item);

  // Attempts to synchronously receive all available items from the block.
  public bool TryReceiveAll(out IList<T> items);

  // Links the block to the specified ITargetBlock{TInput}.
  public IDisposable LinkTo(ITargetBlock<T> target, DataflowLinkOptions linkOptions);

  // Signals to the block that it should not produce any more messages.
  // This block will complete and then the next call to the function's "send" method will throw OperationCanceledException.
  public void Complete();

  // Gets a <see cref="Task"/> that represents the asynchronous operation and completion of the dataflow block.
  public Task Completion { get; }
}
```

## Platform Support

This type is not supported on the following platforms:
* Windows Phone Silverlight 7.5.
* iOS.
* Android.
* Silverlight.

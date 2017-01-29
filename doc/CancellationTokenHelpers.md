## Overview

`CancellationTokenHelpers` provides the operations `Normalize`, `FromTask`, and `Timeout`; and provides a couple of constants `None` and `Canceled`.

`Normalize` takes any number of `CancellationToken`s, removes any that cannot be canceled, and returns the optimum `NormalizedCancellationToken`. If none of the `CancellationToken`s can be cancelled, `Normalize` returns a `NormalizedCancellationToken` wrapping `CancellationToken.None`; if exactly one of the `CancellationToken`s can be cancelled, `Normalize` returns a `NormalizedCancellationToken` wrapping that one; otherwise, `Normalize` uses `CancellationTokenSource.CreateLinkedTokenSource` to link the input `CancellationToken`s into a single `NormalizedCancellationToken`.

`FromTask` returns a `NormalizedCancellationToken` that is canceled when the specified task completes. It doesn't matter how the task completes: whether successfully, canceled, or faulted, the `CancellationToken` will still be canceled.

## API

```C#
// Helper methods for cancellation tokens.
public static class CancellationTokenHelpers
{
  // A cancellation token that is never cancelled.
  public static CancellationToken None { get; }

  // A cancellation token that is already cancelled.
  public static CancellationToken Canceled { get; }

  // Creates a cancellation token that is canceled after the due time.
  public static NormalizedCancellationToken Timeout(TimeSpan dueTime);
  public static NormalizedCancellationToken Timeout(int dueTime);

  // Reduces a set of cancellation tokens to a single cancellation token,
  //    removing any cancellation tokens that cannot be canceled.
  public static NormalizedCancellationToken Normalize(params CancellationToken[] cancellationTokens);
  public static NormalizedCancellationToken Normalize(IEnumerable<CancellationToken> cancellationTokens);

  // Creates a cancellation token that is canceled when the provided Task completes.
  public static NormalizedCancellationToken FromTask(Task source);
  public static NormalizedCancellationToken FromTask(Task source, TaskContinuationOptions continuationOptions);
}

// A CancellationToken that may or may not also reference its own CancellationTokenSource.
// Instances of this type should always be disposed.
public sealed class NormalizedCancellationToken : IDisposable
{
  // Creates a normalized cancellation token that can never be canceled.
  public NormalizedCancellationToken();

  // Creates a normalized cancellation token from a CancellationTokenSource.
  public NormalizedCancellationToken(CancellationTokenSource cts);

  // Creates a normalized cancellation token from a CancellationToken.
  public NormalizedCancellationToken(CancellationToken token);

  // Gets the CancellationToken for this normalized cancellation token.
  public CancellationToken Token { get; }

  // Releases any resources used by this normalized cancellation token.
  public void Dispose();
}
```

## Platform Support

The full API is supported on all platforms.
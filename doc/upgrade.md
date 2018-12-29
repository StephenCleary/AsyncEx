# Upgrading from v4 to v5

## General

All `EnlightenmentVerification` has been removed; the new library no longer requires enlightenment.

## Coordination Primitives

### AsyncLazy

The constructor for `AsyncLazy<T>` that takes a `Func<T>` has been removed; this will cause a compilation error on upgrade. Replace any such calls with an explicit wrapping in `Task.Run` and pass `AsyncLazyFlags.ExecuteOnCallingThread`:

    // Old code (where T.Create returns a T, not a Task<T>):
    new AsyncLazy<T>(() => T.Create());

    // New code:
    new AsyncLazy<T>(() => Task.Run(() => T.Create()), AsyncLazyFlags.ExecuteOnCallingThread);

This is an unusual use case for `AsyncLazy<T>`; most code uses the constructor that takes a `Func<Task<T>>`, and will not need to change.

### AsyncReaderWriterLock

Upgradeable reader locks no longer exist. The `UpgradeableReaderLock` and `UpgradeableReaderLockAsync` methods have been removed, along with the nested `AsyncReaderWriterLock.UpgradeableReaderKey` type.

### AsyncProducerConsumerQueue

`AsyncProducerConsumerQueue<T>` has been simplified; it is no longer disposable, and the `Try` methods have been removed (they caused a good deal of confusion due to the multiple meanings of the `Try` prefix in .NET coding patterns). This includes `TryDequeue`, `TryDequeueAsync`, `TryEnqueue`, and `TryEnqueueAsync`.

All multi-queue operations have been removed (`DequeueFromAny`, `DequeueFromAnyAsync`, `TryDequeueFromAny`, `TryDequeueFromAnyAsync`, `EnqueueToAny`, `EnqueueToAnyAsync`, `TryEnqueueToAny`, and `TryEnqueueToAnyAsync`).

In addition, the nested `AsyncProducerConsumerQueue<T>.DequeueResult` type is no longer necessary, and is gone.

The obsolete `CompleteAddingAsync` has also been removed; use `CompleteAdding` instead.

### AsyncCountdownEvent

The semantics for `AsyncCountdownEvent` have been modified; its count can now be negative, and the event is only signalled when the count is `0`. Integer overflow or underflow now always cause exceptions, so the `TryAddCount` and `TrySignal` methods have been removed.

### AsyncBarrier

`AsyncBarrier` has been removed. Replace uses of `AsyncBarrier` with appropriate explicit synchronization primitives (e.g., `AsyncManualResetEvent`).

### Others

The `IAsyncWaitQueue<T>` infrastructure has been considerably simplified.

The following types have no backwards-incompatible API changes: `AsyncAutoResetEvent`, `AsyncLock`, `AsyncManualResetEvent`, `AsyncMonitor`, `AsyncSemaphore`, `PauseToken`, `PauseTokenSource`.

## Context

`AsyncContext` has not changed.

`AsyncContextThread` can no longer be an STA thread. The STA constructor for `AsyncContextThread` has been removed.

## Tasks

`TaskConstants.Never` and `TaskConstants<T>.Never` have been removed, as they can easily cause memory leaks if misused.

The non-generic `TaskCompletionSource` has been removed. Use `TaskCompletionSource<object>` instead.

The `WithBackgroundContinuations` extension methods for `TaskCompletionSource<T>` have been removed (`TrySetCanceledWithBackgroundContinuations`, `TrySetExceptionWithBackgroundContinuations`, `TrySetResultWithBackgroundContinuations`); instead, use the `TaskCompletionSourceExtensions.CreateAsyncTaskSource` to create a TCS that always uses asynchronous continuations.

`TaskCompletionSource<T>.TryCompleteFromEventArgs` has been temporarily removed; it will be added back in a future version as part of `Nito.AsyncEx.Interop.Eap`.

All other `Task`/`Task<T>`, `TaskFactory`, and `TaskCompletionSource<T>` extensions have not changed.

## SynchronizationContext

The nested type `SynchronizationContextHelpers.SynchronizationContextSwitcher` is now a top-level type `SynchronizationContextSwitcher`.

`SynchronizationContextHelpers.CurrentOrDefault` has been removed; use `SynchronizationContext.Current ?? new SynchronizationContext()` instead.

## Interop

`FromApm` has been removed from `AsyncFactory`/`AsyncFactory<T>`; use `TaskFactory.FromAsync` instead.

`ToBegin` and `ToEnd` on `AsyncFactory`/`AsyncFactory<T>` have been moved to `ApmAsyncFactory` in the `Nito.AsyncEx.Interop` namespace.

`AsyncFactory.FromWaitHandle` has been moved to `WaitHandleAsyncFactory` in the `Nito.AsyncEx.Interop` namespace.

`AsyncFactory<T>.FromEvent` has been replaced with the more strongly-typed `EventAsyncFactory` in the `Nito.AsyncEx.Interop` namespace.

## Cancellation

`NormalizedCancellationToken` has not changed.

The nested type `CancellationTokenExtensions.CancellationTokenTaskSource` is now a top-level generic type `CancellationTokenTaskSource<T>`.

The obsolete `CancellationToken.AsTask` extension method has been removed; use `CancellationTokenTaskSource<T>` instead.

The `CancellationToken.ToCancellationTokenTaskSource` extension method has been removed; use the new `Task` extensions (`WaitAsync`, `WhenAny`, `WhenAll`) or the `CancellationTokenTaskSource<T>` constructor instead.

`CancellationTokenHelpers` has been split up. `Timeout` and `Normalize` have moved to `NormalizedCancellationToken`. `CancellationTokenHelpers.None` should change to `CancellationToken.None`, and `CancellationTokenHelpers.Canceled` should change to `new CancellationToken(true)`. `CancellationTokenHelpers.FromTask` has been removed and has no replacement; let me know if you want this functionality.

## OOP

The API for `DeferralManager` has changed to be more easy to use. It now follows the pattern of having event arg types implement `IDeferralSource` by forwarding to `DeferralManager.DeferralSource`. This prevents the `DeferralManager` from being exposed to the event args type. Also, `SignalAndWaitAsync` has been renamed to `WaitForDeferralsAsync`.

## MVVM

All MVVM types have been moved to the `Nito.Mvvm.Async` library. It is currently not included in `Nito.AsyncEx` since the MVVM APIs are still too much in flux. Eventually, `Nito.Mvvm.Async` will probably be rolled back in to `Nito.AsyncEx`, but for 5.0.0 it will be a separate install.

`PropertyProgress` has been temporarily removed. It will be replaced with a more powerful type in the future.

`ProducerProgress` has been permanently removed.

`INotifyTaskCompletion` and `NotifyTaskCompletion` have been replaced with `NotifyTask`. `INotifyTaskCompletion<T>` has been replaced with `NotifyTask<TResult>`.

## Misc

`ExceptionHelpers` has not changed.

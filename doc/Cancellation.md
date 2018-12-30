# Cancellation behavior in AsyncEx

Most of the asynchronous coordination primitive APIs take an optional `CancellationToken`. There are four different types of "waiting" that can be done.

Each of the examples here use `AsyncSemaphore.WaitAsync`.

## Unconditional wait

An unconditional wait is the most commonly-used type of wait: the code knows it needs to acquire the semaphore and it will wait however long it takes until the semaphore is available.

The unconditional wait uses the overload without a `CancellationToken`: `WaitAsync()`. You can also pass `CancellationToken.None`, which has the same effect: `WaitAsync(CancellationToken.None)`.

## Cancelable wait

A cancelable wait is where some event can interrupt the wait if it determines that the code doesn’t need that semaphore anymore.

To do a cancelable wait, pass in the `CancellationToken` that is used to cancel the wait. If a wait is canceled, the task returned from `WaitAsync` is canceled, and the semaphore is *not* taken.

## Atomic wait

An atomic wait is where the code will immediately (synchronously) acquire the semaphore if it is available.

To do an atomic wait, pass in a cancellation token that is already canceled. In this case, `WaitAsync` will always return synchronously: a successfully completed task if the semaphore is available (and taken), otherwise a canceled task (and the semaphore is not taken).

Note: atomic waits should be extremely rare! Carefully review your design if you are considering them.

## Timed wait

A timed wait is a special case of a cancelable wait.

To do a timed wait, create a cancellation token to cancel the wait (e.g., using [CancellationTokenSource](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtokensource.-ctor#System_Threading_CancellationTokenSource__ctor_System_TimeSpan_)) and pass that token to `WaitAsync`. When the timer expires, the wait is canceled.

Note: timed waits should be extremely, *extremely* rare! Strongly reconsider your design if you are considering them.

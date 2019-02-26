# Design Guidelines

- Do not name methods starting with a `Try` prefix. Particularly in the asynchronous world, ["Try" is ambiguous](https://github.com/StephenCleary/AsyncEx/issues/141#issuecomment-409287646); does it mean "attempt this synchronously" or "attempt this without exceptions"?
  - `TaskCompletionSourceExtensions.TryCompleteFromCompletedTask` is an exception to this rule; it uses the `Try` prefix to match the existing `TaskCompletionSource<T>` API.
- Try to structure APIs to guide users into success rather than failure.
  - E.g., coordination primitives should avoid exposing their current state via properties because those properties would encourage code with race conditions.
- Do not use strong naming. Currently, adding strong naming creates too much of a maintenance burden; if Microsoft releases better strongname tooling, then this guideline can be reconsidered.

# Additional guidelines for AsyncEx.Coordination

- The API for asynchronous coordination primitives should mimic that of their synchronous counterparts, with the addition of asynchronous APIs and `CancellationToken` support.
  - In particular, if you find yourself wanting to add an API to a coordination primitive, you're almost certainly using the wrong primitive.
  - `AutoResetEvent.Reset` is [an exception to this rule](https://github.com/StephenCleary/AsyncEx/issues/27#issuecomment-133921579).
- Keep all synchronization primitives consistent. If you're adding something to one type, consider **all** other primitives and add it to all of them at the same time.
- [Do not use explicit timeouts (`int` or `TimeSpan` parameters)](https://github.com/StephenCleary/AsyncEx/issues/46#issuecomment-187685580). These are only present on the synchronous primitives because they can be passed down to the Win32 API layer; in the asynchronous primitive world, use `CancellationToken` instead. If you really want overloads with timeout parameters, they are easy to add as extension methods in your own code.
- When unit testing, do not have timing-dependent tests.

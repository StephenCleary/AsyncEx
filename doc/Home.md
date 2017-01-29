AsyncEx is a library designed to assist with programming when using the `async` and `await` keywords.

## Asynchronous Coordination Primitives

Most people just need [[AsyncLock]], but AsyncEx also includes [[AsyncManualResetEvent]], [[AsyncAutoResetEvent]], [[AsyncConditionVariable]], [[AsyncMonitor]], [[AsyncSemaphore]], [[AsyncCountdownEvent]], [[AsyncBarrier]], and [[AsyncReaderWriterLock]].

## Asynchronous and Concurrent Collections

[[AsyncProducerConsumerQueue]] is an `async`-ready producer/consumer queue that works on all platforms.

[[AsyncCollection]] is an `async`-compatible wrapper around `IProducerConsumerCollection`, in the same way that [[BlockingCollection|http://msdn.microsoft.com/en-us/library/dd267312(v=VS.110).aspx]] is a blocking wrapper around `IProducerConsumerCollection`.

You can also use [[ProducerProgress]] to send `IProgress` updates to a producer/consumer collection.

## AsyncLazy

The [[AsyncLazy]] class provides support for [[asynchronous lazy initialization|http://blog.stephencleary.com/2012/08/asynchronous-lazy-initialization.html]].

## AsyncContext

The [[AsyncContext]] class and the related `AsyncContextThread` class provide contexts for asynchronous operations for situations where such a context is lacking (i.e., Console applications and Win32 services).

## Asynchronous events

[[DeferralManager]] simplifies the writing of "command-style" events that provide a `GetDeferral` method.

## MVVM

[[NotifyTaskCompletion]] is a data-binding-friendly wrapper for `Task`, providing property-change notifications when the task completes.

## Interop

The [[AsyncFactory]] classes interoperate between Task-based Asynchronous Programming (`async`) and the Asynchronous Programming Model (`IAsyncResult`). `AsyncFactory<T>` can also create wrapper tasks for most events.

[[TaskCompletionSource]] is a non-generic version of the built-in `TaskCompletionSource<TResult>`.

[[TaskCompletionSourceExtensions]] includes `TryCompleteFromEventArgs`, which helps interoperate between Task-based Asynchronous Programming (`async`) and Event-based Asynchronous Programming (`*Completed`).

## Utility Types and Methods

You can use [[OrderByCompletion|TaskExtensions]] to order tasks by when they complete.

[[CancellationTokenHelpers]] provides some static methods and constants for working with `CancellationToken`s.

## Miscellaneous

[[TaskConstants]] provides static constant `Task<TResult>` values that are useful when taking advantage of the [[async fast path|http://blogs.msdn.com/b/lucian/archive/2011/04/15/async-ctp-refresh-design-changes.aspx]].

You can respond to task progress updates by using one of the implementations of `IProgress<T>`: [[PropertyProgress]], [[ProducerProgress]], or [[DataflowProgress]].

`ObservableProgress`, an `IProgress<T>` implementation that exposes progress updates as an `IObservable<T>` stream, is available [[as a gist|https://gist.github.com/StephenCleary/4248e50b4cb52b933c0d]].

## Extension Methods

[[CancellationToken.AsTask|CancellationTokenExtensions]] will create a task that is canceled when a `CancellationToken` is canceled.

[[TaskCompletionSourceExtensions]] has several extension methods for `TaskCompletionSource`, including propagating results from a completed `Task` or `AsyncCompletedEventArgs`, and for completing the source forcing background continuations.

[[TaskFactoryExtensions]] adds `Run` overloads to task factories.

## Dataflow Support

There is currently only one Dataflow block in the library: [[FuncBlock]], which allows an `async` function to provide data to a dataflow mesh with full cooperative cancellation and error propagation.

## Low-Level Building Blocks

[[ExceptionHelpers.PrepareForRethrow|ExceptionHelpers]] will preserve the stack trace when rethrowing any exception.

For dealing with `SynchronizationContext` directly, the library provides [[CurrentOrDefault and SynchronizationContextSwitcher|SynchronizationContextHelpers]].
AsyncEx is a library designed to assist with programming when using the `async` and `await` keywords.

## Asynchronous Coordination Primitives

Most people just need [AsyncLock](AsyncLock), but AsyncEx also includes [[AsyncManualResetEvent]], [[AsyncAutoResetEvent]], [[AsyncConditionVariable]], [[AsyncMonitor]], [[AsyncSemaphore]], [[AsyncCountdownEvent]], and [[AsyncReaderWriterLock]].

## Asynchronous and Concurrent Collections

[[AsyncProducerConsumerQueue]] is an `async`-ready producer/consumer queue that works on all platforms.

[[AsyncCollection]] is an `async`-compatible wrapper around `IProducerConsumerCollection`, in the same way that [[BlockingCollection|https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.blockingcollection-1]] is a blocking wrapper around `IProducerConsumerCollection`.

## AsyncLazy

The [[AsyncLazy]] class provides support for [[asynchronous lazy initialization|https://blog.stephencleary.com/2012/08/asynchronous-lazy-initialization.html]].

## AsyncContext

The [[AsyncContext]] class and the related `AsyncContextThread` class provide contexts for asynchronous operations for situations where such a context is lacking (i.e., Console applications and Win32 services).

## Interop

The [[ApmAsyncFactory]] classes interoperate between Task-based Asynchronous Programming (`async`) and the Asynchronous Programming Model (`IAsyncResult`).

## Utility Types and Methods

You can use [[OrderByCompletion|TaskExtensions]] to order tasks by when they complete.

## Miscellaneous

[[TaskConstants]] provides static constant `Task<TResult>` values.

## Extension Methods

[[TaskCompletionSourceExtensions]] has several extension methods for `TaskCompletionSource<T>`, including propagating results from a completed `Task`.

[[TaskFactoryExtensions]] adds `Run` overloads to task factories.

## Low-Level Building Blocks

[[ExceptionHelpers.PrepareForRethrow|ExceptionHelpers]] will preserve the stack trace when rethrowing any exception.

## Overview

An `AsyncCollection` is an `async`-compatible wrapper around [IProducerConsumerCollection](https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.iproducerconsumercollection-1) collections such as [ConcurrentQueue](https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentqueue-1) or [ConcurrentBag](https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentbag-1).

This makes `AsyncCollection` an `async` near-equivalent of [BlockingCollection](https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.blockingcollection-1), which is a blocking wrapper around [IProducerConsumerCollection](https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.iproducerconsumercollection-1).

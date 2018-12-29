## Overview

The `AsyncLazy<T>` type enables [asynchronous lazy initialization](https://blog.stephencleary.com/2012/08/asynchronous-lazy-initialization.html), similar to [Stephen Toub's AsyncLazy](https://blogs.msdn.microsoft.com/pfxteam/2011/01/15/asynclazyt/).

An `AsyncLazy<T>` instance is constructed with a factory method. When the `AsyncLazy<T>` instance is `await`ed or its `Start` method is called, the factory method starts on a thread pool thread. The factory method is only executed once. Once the factory method has completed, all future `await`s on that instance complete immediately.

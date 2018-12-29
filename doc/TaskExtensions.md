## Overview

`TaskExtensions` provides one method `OrderByCompletion` which orders a sequence of Tasks by when they complete. The approach taken by AsyncEx is a combination of [Jon Skeet's approach](https://codeblog.jonskeet.uk/2012/01/16/eduasync-part-19-ordering-by-completion-ahead-of-time/) and [Stephen Toub's approach](https://blogs.msdn.microsoft.com/pfxteam/2012/08/02/processing-tasks-as-they-complete/).

`TaskExtensions` in the `Nito.AsyncEx.Synchronous` namespace provides a handful of extension methods that enable synchronous blocking of a `Task` without wrapping its exception in an `AggregateException`, or without observing the `Task` exception at all. These are advanced methods that should probably not be used, since you [run the risk of a deadlock](https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html).

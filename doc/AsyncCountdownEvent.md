## Overview

This is the `async`-ready almost-equivalent of [CountdownEvent](https://docs.microsoft.com/en-us/dotnet/api/system.threading.countdownevent), similar to Stephen Toub's [AsyncCountdownEvent](https://blogs.msdn.microsoft.com/pfxteam/2012/02/11/building-async-coordination-primitives-part-3-asynccountdownevent/). It's only an *almost* equivalent because the `AsyncCountdownEvent` does not allow itself to be reset.

An `AsyncCountdownEvent` starts out **unset** and becomes **set** only once, when its **count** reaches zero. Its current count can be manipulated by any other tasks up until the time it reaches zero. When the count reaches zero, all waiting tasks are released.

The task returned from `WaitAsync` will enter the `Completed` state when the `AsyncCountdownEvent` has counted down to zero and enters the **set** state.

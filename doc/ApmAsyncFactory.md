## Overview

The `ApmAsyncFactory` type enables easy interoperation with the Asynchronous Programming Model (APM).

APM is the old-style approach to asynchronous programming that used `Begin`/`End` method pairs with `IAsyncResult` representing the asynchronous operation. The `FromApm` methods on `ApmAsyncFactory` convert from APM to TAP, and the `ToBegin` and `ToEnd` methods convert from TAP to APM.

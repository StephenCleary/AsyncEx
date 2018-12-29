## Overview

There is one static method on the `ExceptionHelpers` type: `PrepareForRethrow`.

The purpose of this method is to preserve the original stack trace of the exception so that it is not overwritten when the exception is rethrown.

The return value of this method should always be thrown immediately; that is, every call to this method should look like this:

```C#
Exception ex = ...; // get saved exception
throw ExceptionHelpers.PrepareForRethrow(ex);
```

`PrepareForRethrow` uses [ExceptionDispatchInfo](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.exceptionservices.exceptiondispatchinfo).

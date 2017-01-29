## Overview

There is one static method on the `ExceptionHelpers` type: `PrepareForRethrow`.

The purpose of this method is to preserve the original stack trace of the exception so that it is not overwritten when the exception is rethrown.

The return value of this method should always be thrown immediately; that is, every call to this method should look like this:

```C#
Exception ex = ...; // get saved exception
throw ExceptionHelpers.PrepareForRethrow(ex);
```

`PrepareForRethrow` will use [[ExceptionDispatchInfo|http://msdn.microsoft.com/en-us/library/system.runtime.exceptionservices.exceptiondispatchinfo.aspx]] if it is running on a machine with .NET 4.5 installed. If only .NET 4.0 is available, this method will fall back to the undocumented but somewhat common reflection hack of [[calling Exception.PrepForRemoting|http://connect.microsoft.com/VisualStudio/feedback/details/633822/allow-preserving-stack-traces-when-rethrowing-exceptions]]. If both of these approaches fail (which may happen in Silverlight or partial-trust scenarios), the original stack trace is added to the `Data` property of the exception.

## API

```C#
// Provides helper (non-extension) methods dealing with exceptions.
public static class ExceptionHelpers
{
  // Attempts to prepare the exception for re-throwing by preserving the stack trace. The returned exception should be immediately thrown.
  Exception PrepareForRethrow(Exception exception);
}
```

## Platform Support

The full API is supported on all platforms.
## Overview

The `AsyncFactory` and `AsyncFactory<T>` types enable easy interoperation with the Asynchronous Programming Model (APM). `AsyncFactory<T>` also has a `FromEvent` method that creates a wrapper task for a specific event.

APM is the old-style approach to asynchronous programming that used `Begin`/`End` method pairs with `IAsyncResult` representing the asynchronous operation. The `FromApm` methods on `AsyncFactory` convert from APM to TAP, and the `ToBegin` and `ToEnd` methods convert from TAP to APM.

## API

```C#
public static class AsyncFactory
{
  public static Task FromApm(Func<AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod);
  public static Task FromApm<TArg0>(Func<TArg0, AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, TArg0 arg0);
  public static Task FromApm<TArg0, TArg1>(Func<TArg0, TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, TArg0 arg0, TArg1 arg1);
  ... // So on, up through 14 arguments (TArg0-TArg13).

  public static IAsyncResult ToBegin(Task task, AsyncCallback callback, object state);
  public static void ToEnd(IAsyncResult asyncResult);
}

public static class AsyncFactory<TResult>
{
  // Gets a task that will complete the next time an event is raised.
  // The event type must follow the standard <c>void EventHandlerType(object, TResult)</c> pattern.
  // If eventName is not passed, this method will attempt to find the intended event.
  public static Task<TResult> FromEvent(object target, string eventName);
  public static Task<TResult> FromEvent(object target);

  public static Task<TResult> FromApm(Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod);
  public static Task<TResult> FromApm<TArg0>(Func<TArg0, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg0 arg0);
  public static Task<TResult> FromApm<TArg0, TArg1>(Func<TArg0, TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, TArg0 arg0, TArg1 arg1);
  ... // So on, up through 14 arguments (TArg0-TArg13).

  public static IAsyncResult ToBegin(Task<TResult> task, AsyncCallback callback, object state);
  public static TResult ToEnd(IAsyncResult asyncResult);
}
```

## Platform Support

The full API is supported on all platforms.
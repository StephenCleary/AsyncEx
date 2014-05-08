using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Runtime.CompilerServices;

internal static class TaskShim
{
    public static void AssertDenyChildAttach(Task task)
    {
    }

    public static TimeSpan InfiniteTimeSpan
    {
        get { return new TimeSpan(0, 0, 0, 0, -1); }
    }

    public static Task Run(Action func)
    {
        return TaskEx.Run(func);
    }

    public static Task Run(Func<Task> func)
    {
        return TaskEx.Run(func);
    }

    public static Task<T> Run<T>(Func<T> func)
    {
        return TaskEx.Run(func);
    }

    public static Task<T> Run<T>(Func<Task<T>> func)
    {
        return TaskEx.Run(func);
    }

    public static Task<T> FromResult<T>(T value)
    {
        return TaskEx.FromResult(value);
    }

    public static Task<T[]> WhenAll<T>(IEnumerable<Task<T>> tasks)
    {
        return TaskEx.WhenAll(tasks);
    }

    public static Task<T[]> WhenAll<T>(params Task<T>[] tasks)
    {
        return TaskEx.WhenAll(tasks);
    }

    public static Task WhenAll(IEnumerable<Task> tasks)
    {
        return TaskEx.WhenAll(tasks);
    }

    public static Task WhenAll(params Task[] tasks)
    {
        return TaskEx.WhenAll(tasks);
    }

    public static YieldAwaitable Yield()
    {
        return TaskEx.Yield();
    }

    public static Task Delay(int milliseconds)
    {
        return TaskEx.Delay(milliseconds);
    }

    public static Task<Task> WhenAny(params Task[] tasks)
    {
        return TaskEx.WhenAny(tasks);
    }

    public static Task<Task> WhenAny(IEnumerable<Task> tasks)
    {
        return TaskEx.WhenAny(tasks);
    }

    public static Task<Task<T>> WhenAny<T>(params Task<T>[] tasks)
    {
        return TaskEx.WhenAny(tasks);
    }

    public static Task<Task<T>> WhenAny<T>(IEnumerable<Task<T>> tasks)
    {
        return TaskEx.WhenAny(tasks);
    }
}

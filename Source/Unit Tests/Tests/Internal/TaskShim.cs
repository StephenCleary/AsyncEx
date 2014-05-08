using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

internal static class TaskShim
{
    public static void AssertDenyChildAttach(Task task)
    {
        Assert.IsTrue((task.CreationOptions & TaskCreationOptions.DenyChildAttach) == TaskCreationOptions.DenyChildAttach);
    }

    public static TimeSpan InfiniteTimeSpan
    {
        get { return Timeout.InfiniteTimeSpan; }
    }

    public static Task Run(Action func)
    {
        return Task.Run(func);
    }

    public static Task Run(Func<Task> func)
    {
        return Task.Run(func);
    }

    public static Task<T> Run<T>(Func<T> func)
    {
        return Task.Run(func);
    }

    public static Task<T> Run<T>(Func<Task<T>> func)
    {
        return Task.Run(func);
    }

    public static Task<T> FromResult<T>(T value)
    {
        return Task.FromResult(value);
    }

    public static Task<T[]> WhenAll<T>(IEnumerable<Task<T>> tasks)
    {
        return Task.WhenAll(tasks);
    }

    public static Task<T[]> WhenAll<T>(params Task<T>[] tasks)
    {
        return Task.WhenAll(tasks);
    }

    public static Task WhenAll(params Task[] tasks)
    {
        return Task.WhenAll(tasks);
    }

    public static Task WhenAll(IEnumerable<Task> tasks)
    {
        return Task.WhenAll(tasks);
    }

    public static YieldAwaitable Yield()
    {
        return Task.Yield();
    }

    public static Task Delay(int milliseconds)
    {
        return Task.Delay(milliseconds);
    }

    public static Task<Task> WhenAny(params Task[] tasks)
    {
        return Task.WhenAny(tasks);
    }

    public static Task<Task> WhenAny(IEnumerable<Task> tasks)
    {
        return Task.WhenAny(tasks);
    }

    public static Task<Task<T>> WhenAny<T>(params Task<T>[] tasks)
    {
        return Task.WhenAny(tasks);
    }

    public static Task<Task<T>> WhenAny<T>(IEnumerable<Task<T>> tasks)
    {
        return Task.WhenAny(tasks);
    }
}

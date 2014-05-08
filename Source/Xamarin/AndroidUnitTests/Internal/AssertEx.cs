using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx.Internal;
using Tests;

/// <summary>
/// Provides static methods useful for testing asynchronous methods and tasks.
/// </summary>
internal static class AssertEx
{
    /// <summary>
    /// Ensures that a (synchronous) delegate throws an exception.
    /// </summary>
    /// <typeparam name="TException">The type of exception to expect.</typeparam>
    /// <param name="action">The synchronous delegate to test.</param>
    /// <param name="allowDerivedTypes">Whether derived types should be accepted.</param>
    public static void ThrowsException<TException>(Action action, bool allowDerivedTypes = true)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            if (allowDerivedTypes && !(ex is TException))
                Assert.Fail("Delegate threw exception of type " + ex.GetType().Name + ", but " + typeof(TException).Name + " or a derived type was expected.");
            if (!allowDerivedTypes && ex.GetType() != typeof(TException))
                Assert.Fail("Delegate threw exception of type " + ex.GetType().Name + ", but " + typeof(TException).Name + " was expected.");
        }
    }

    /// <summary>
    /// Ensures that a task throws an exception.
    /// </summary>
    /// <typeparam name="TException">The type of exception to expect.</typeparam>
    /// <param name="task">The task to observe.</param>
    /// <param name="allowDerivedTypes">Whether derived types should be accepted.</param>
    public static Task ThrowsExceptionAsync<TException>(Task task, bool allowDerivedTypes = true)
    {
        return ThrowsExceptionAsync<TException>(() => task, allowDerivedTypes);
    }

    /// <summary>
    /// Ensures that an asynchronous delegate throws an exception.
    /// </summary>
    /// <typeparam name="TException">The type of exception to expect.</typeparam>
    /// <param name="action">The asynchronous delegate to test.</param>
    /// <param name="allowDerivedTypes">Whether derived types should be accepted.</param>
    public static async Task ThrowsExceptionAsync<TException>(Func<Task> action, bool allowDerivedTypes = true)
    {
        try
        {
            await action().ConfigureAwait(false);
            Assert.Fail("Delegate did not throw expected exception " + typeof(TException).Name + ".");
        }
        catch (Exception ex)
        {
            if (allowDerivedTypes && !(ex is TException))
                Assert.Fail("Delegate threw exception of type " + ex.GetType().Name + ", but " + typeof(TException).Name + " or a derived type was expected.");
            if (!allowDerivedTypes && ex.GetType() != typeof(TException))
                Assert.Fail("Delegate threw exception of type " + ex.GetType().Name + ", but " + typeof(TException).Name + " was expected.");
        }
    }

    /// <summary>
    /// Attempts to ensure that a task never completes.
    /// </summary>
    /// <param name="task">The task to observe.</param>
    /// <param name="timeout">The amount of time to (asynchronously) wait for the task to complete.</param>
    public static async Task NeverCompletesAsync(Task task, int timeout = 500)
    {
        if (task.IsCompleted)
            Assert.Fail("Task completed unexpectedly.");
        var completedTask = await Task.WhenAny(task, AndroidWorkarounds.Delay(timeout)).ConfigureAwait(false);
        if (completedTask == task)
            Assert.Fail("Task completed unexpectedly.");
        var __ = task.ContinueWith(_ => Assert.Fail("Task completed unexpectedly."));
    }

    /// <summary>
    /// Ensures that a task is canceled.
    /// </summary>
    /// <param name="task">The task to observe.</param>
    public static async Task CompletesCanceledAsync(Task task)
    {
        try
        {
			await task.ConfigureAwait(false);
            Assert.Fail("Task expected to cancel completed successfully.");
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            Assert.Fail("Task expected to cancel completed with failure: " + ex.GetType().ToString() + ": " + ex.Message);
        }
    }
}

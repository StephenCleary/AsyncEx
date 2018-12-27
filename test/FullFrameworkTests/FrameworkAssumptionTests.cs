using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using Xunit;
using System.Runtime.CompilerServices;

namespace UnitTests
{
    public class FrameworkAssumptionTests
    {
        [Fact]
        public async Task ForceInline_WithRunContinuationsAsynchronouslyFlag_RunsContinuationsSynchronouslyAndHidesInlineScheduler()
        {
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var continuationThreadId = -1;
            var continuation = WaitAsync();
            await Task.Run(() =>
            {
                var completionThreadId = Thread.CurrentThread.ManagedThreadId;
                tcs.TrySetResult(null);
                Assert.True(continuation.IsCompleted);
                Assert.Equal(completionThreadId, continuationThreadId);
            });
            await continuation;

            async Task WaitAsync()
            {
                await tcs.Task.ForceInline();
                continuationThreadId = Thread.CurrentThread.ManagedThreadId;
                Assert.NotSame(ForceInlineTaskScheduler.Instance, TaskScheduler.Current);
            }
        }
    }

    public sealed class ForceInlineTaskScheduler: TaskScheduler
    {
        private ForceInlineTaskScheduler() { }
        public static ForceInlineTaskScheduler Instance { get; } = new ForceInlineTaskScheduler();
        protected override IEnumerable<Task> GetScheduledTasks() => Enumerable.Empty<Task>();
        protected override void QueueTask(Task task) => TryExecuteTask(task);
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => TryExecuteTask(task);
    }

    /// <summary>
    /// An awaitable object that forces tasks to continue inline.
    /// </summary>
    public struct ForceInlineAwaiter: INotifyCompletion
    {
        private readonly Task _task;

        public ForceInlineAwaiter(Task task)
        {
            _task = task;
        }

        /// <summary>
        /// Gets a value indicating whether the awaitable task has completed.
        /// </summary>
        public bool IsCompleted => _task.IsCompleted;

        /// <summary>
        /// Schedules some action to execute after the awaitable task has completed.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public void OnCompleted(Action action)
        {
            _task.ContinueWith(_ => action(),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.DenyChildAttach | TaskContinuationOptions.HideScheduler,
                ForceInlineTaskScheduler.Instance);
        }

        /// <summary>
        /// Gets the result of the awaited task.
        /// </summary>
        public void GetResult()
        {
            _task.GetAwaiter().GetResult();
        }

        public ForceInlineAwaiter GetAwaiter() => this;
    }

    public static class TaskExtensions
    {
        public static ForceInlineAwaiter ForceInline(this Task task) => new ForceInlineAwaiter(task);
    }
}

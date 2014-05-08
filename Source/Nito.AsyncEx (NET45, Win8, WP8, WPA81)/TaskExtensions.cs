using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// Provides extension methods for tasks.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Creates a new array of tasks which complete in order.
        /// </summary>
        /// <typeparam name="T">The type of the results of the tasks.</typeparam>
        /// <param name="tasks">The tasks to order by completion.</param>
        public static Task<T>[] OrderByCompletion<T>(this IEnumerable<Task<T>> tasks)
        {
            // This is a combination of Jon Skeet's approach and Stephen Toub's approach:
            //  http://msmvps.com/blogs/jon_skeet/archive/2012/01/16/eduasync-part-19-ordering-by-completion-ahead-of-time.aspx
            //  http://blogs.msdn.com/b/pfxteam/archive/2012/08/02/processing-tasks-as-they-complete.aspx

            // Reify the source task sequence.
            var taskArray = tasks.ToArray();

            // Allocate a TCS array and an array of the resulting tasks.
            var numTasks = taskArray.Length;
            var tcs = new TaskCompletionSource<T>[numTasks];
            var ret = new Task<T>[numTasks];

            // As each task completes, complete the next tcs.
            int lastIndex = -1;
            Action<Task<T>> continuation = task =>
            {
                var index = Interlocked.Increment(ref lastIndex);
                tcs[index].TryCompleteFromCompletedTask(task);
            };

            // Fill out the arrays and attach the continuations.
            for (int i = 0; i != numTasks; ++i)
            {
                tcs[i] = new TaskCompletionSource<T>();
                ret[i] = tcs[i].Task;
                taskArray[i].ContinueWith(continuation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }

            return ret;
        }
    }
}

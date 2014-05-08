using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Runtime.CompilerServices;

namespace Nito.AsyncEx.Internal
{
    internal static class TaskShim
    {
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
    }
}

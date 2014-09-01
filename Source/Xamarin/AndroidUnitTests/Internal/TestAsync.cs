using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;

internal static class Test
{
    public static void Async(Func<Task> test)
    {
		AsyncContext.Run (() => Task.Run(test));
    }
}
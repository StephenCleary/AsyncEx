using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Instead of this, use https://github.com/svick/AsyncParallel

namespace ClassLibrary1
{
    public static class Class1
    {
        public static Task ForEachAsync<T>(this IEnumerable<T> source,
            int degreeOfParallelism, Func<T, Task> body)
        {
            var partitions = Partitioner.Create(source).GetPartitions(degreeOfParallelism);
            var tasks = partitions.Select(async partition =>
            {
                using (partition)
                    while (partition.MoveNext())
                        await body(partition.Current);
            });
            return Task.WhenAll(tasks);
        }
    }
}

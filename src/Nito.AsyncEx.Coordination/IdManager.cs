using System.Threading;

namespace Nito.AsyncEx
{
    /// <summary>
    /// Allocates Ids for instances on demand. 0 is an invalid/unassigned Id. Ids may be non-unique in very long-running systems. This is similar to the Id system used by <see cref="System.Threading.Tasks.Task"/> and <see cref="System.Threading.Tasks.TaskScheduler"/>.
    /// </summary>
    /// <typeparam name="TTag">The type for which ids are generated.</typeparam>
// ReSharper disable UnusedTypeParameter
    internal static class IdManager<TTag>
// ReSharper restore UnusedTypeParameter
    {
        /// <summary>
        /// The last id generated for this type. This is 0 if no ids have been generated.
        /// </summary>
// ReSharper disable StaticFieldInGenericType
        private static int _lastId;
// ReSharper restore StaticFieldInGenericType

        /// <summary>
        /// Returns the id, allocating it if necessary.
        /// </summary>
        /// <param name="id">A reference to the field containing the id.</param>
        public static int GetId(ref int id)
        {
            // If the Id has already been assigned, just use it.
            if (id != 0)
                return id;

            // Determine the new Id without modifying "id", since other threads may also be determining the new Id at the same time.
            int newId;

            // The Increment is in a while loop to ensure we get a non-zero Id:
            //  If we are incrementing -1, then we want to skip over 0.
            //  If there are tons of Id allocations going on, we want to skip over 0 no matter how many times we get it.
            do
            {
                newId = Interlocked.Increment(ref _lastId);
            } while (newId == 0);

            // Update the Id unless another thread already updated it.
            Interlocked.CompareExchange(ref id, newId, 0);

            // Return the current Id, regardless of whether it's our new Id or a new Id from another thread.
            return id;
        }
    }
}

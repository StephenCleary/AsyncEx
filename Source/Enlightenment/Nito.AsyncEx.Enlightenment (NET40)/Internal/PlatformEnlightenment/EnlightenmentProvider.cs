using System;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    /// <summary>
    /// The platform enlightenment provider for .NET 4.0.
    /// </summary>
    public sealed partial class EnlightenmentProvider : IEnlightenmentProvider
    {
        T IEnlightenmentProvider.CreateEnlightenment<T>()
        {
            var type = typeof(T);
            if (type == typeof(ISynchronizationContextEnlightenment))
                return (T)(object)new SynchronizationContextEnlightenment();
            if (type == typeof(ITraceEnlightenment))
                return (T)(object)new TraceEnlightenment();
            if (type == typeof(IConcurrentCollectionsEnlightenment))
                return (T)(object)new ConcurrentCollectionsEnlightenment();
            if (type == typeof(ILazyEnlightenment))
                return (T)(object)new LazyEnlightenment();
            if (type == typeof(IThreadPoolEnlightenment))
                return (T)(object)new ThreadPoolEnlightenment();
            if (type == typeof(IThreadIdentityEnlightenment))
                return (T)(object)new ThreadIdentityEnlightenment();
            if (type == typeof(ISingleThreadedApartmentEnlightenment))
                return (T)(object)new SingleThreadedApartmentEnlightenment();

            return null;
        }
    }
}

using System;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    /// <summary>
    /// The platform enlightenment provider for .NET 4.5.
    /// </summary>
    public sealed partial class EnlightenmentProvider : IEnlightenmentProvider
    {
        T IEnlightenmentProvider.CreateEnlightenment<T>()
        {
            var type = typeof(T);
            if (type == typeof(IExceptionEnlightenment))
                return (T)(object)new ExceptionEnlightenment();
            if (type == typeof(IAsyncEnlightenment))
                return (T)(object)new AsyncEnlightenment();
            if (type == typeof(ISynchronizationContextEnlightenment))
                return (T)(object)new SynchronizationContextEnlightenment();
            if (type == typeof(IConcurrentCollectionsEnlightenment))
                return (T)(object)new ConcurrentCollectionsEnlightenment();
            if (type == typeof(ILazyEnlightenment))
                return (T)(object)new LazyEnlightenment();
            if (type == typeof(IThreadPoolEnlightenment))
                return (T)(object)new ThreadPoolEnlightenment();
            if (type == typeof(ISingleThreadedApartmentEnlightenment))
                return (T)(object)new SingleThreadedApartmentEnlightenment();

            return null;
        }
    }
}

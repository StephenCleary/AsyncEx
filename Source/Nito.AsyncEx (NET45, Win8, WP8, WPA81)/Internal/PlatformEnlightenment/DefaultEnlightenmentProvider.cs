using System;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    /// <summary>
    /// The default enlightenment provider, used when the platform enlightenment provider could not be found.
    /// </summary>
    public sealed partial class DefaultEnlightenmentProvider : IEnlightenmentProvider
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
            if (type == typeof(ITraceEnlightenment))
                return (T)(object)new TraceEnlightenment();
            if (type == typeof(IConcurrentCollectionsEnlightenment))
                return (T)(object)new ConcurrentCollectionsEnlightenment();
            if (type == typeof(ILazyEnlightenment))
                return (T)(object)new LazyEnlightenment();
            if (type == typeof(IThreadPoolEnlightenment))
                return (T)(object)new ThreadPoolEnlightenment(new ReflectionExpressionProvider());
            if (type == typeof(IThreadIdentityEnlightenment))
                return (T)(object)new ThreadIdentityEnlightenment();
            if (type == typeof(ISingleThreadedApartmentEnlightenment))
                return (T)(object)new SingleThreadedApartmentEnlightenment();

            throw new NotImplementedException();
        }
    }
}

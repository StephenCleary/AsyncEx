using System;
using System.Threading;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    /// <summary>
    /// The platform enlightenment provider for Silverlight 4.
    /// </summary>
    public sealed partial class EnlightenmentProvider : IEnlightenmentProvider
    {
        T IEnlightenmentProvider.CreateEnlightenment<T>()
        {
            var type = typeof(T);
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

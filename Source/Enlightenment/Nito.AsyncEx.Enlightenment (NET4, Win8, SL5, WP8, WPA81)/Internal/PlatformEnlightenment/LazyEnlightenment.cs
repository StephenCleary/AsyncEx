using System;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    public sealed class LazyEnlightenment<T>
    {
        public LazyEnlightenment(Func<T> factory)
        {
            throw Enlightenment.Exception();
        }
    }
}

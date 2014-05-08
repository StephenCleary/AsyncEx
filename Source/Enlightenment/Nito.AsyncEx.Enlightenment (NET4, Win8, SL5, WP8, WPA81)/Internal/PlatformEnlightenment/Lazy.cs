using System;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    public sealed class Lazy<T>
    {
        public Lazy(Func<T> factory)
        {
            throw Enlightenment.Exception();
        }

        public bool IsValueCreated { get { throw Enlightenment.Exception(); } }

        public T Value { get { throw Enlightenment.Exception(); } }
    }
}

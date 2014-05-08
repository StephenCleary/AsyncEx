using System;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    public sealed class Lazy<T>
    {
        private readonly System.Lazy<T> _lazy;

        public Lazy(Func<T> factory)
        {
            _lazy = new System.Lazy<T>(factory);
        }

        public bool IsValueCreated { get { return _lazy.IsValueCreated; } }

        public T Value { get { return _lazy.Value; } }
    }
}

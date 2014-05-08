using System;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    partial class EnlightenmentProvider
    {
        private sealed class LazyEnlightenment : ILazyEnlightenment
        {
            ILazy<T> ILazyEnlightenment.CreateLazy<T>(Func<T> factory)
            {
                return new PlatformLazy<T>(factory);
            }

            private sealed class PlatformLazy<T> : Lazy<T>, ILazy<T>
            {
                public PlatformLazy(Func<T> factory)
                    : base(factory)
                {
                }
            }
        }
    }
}

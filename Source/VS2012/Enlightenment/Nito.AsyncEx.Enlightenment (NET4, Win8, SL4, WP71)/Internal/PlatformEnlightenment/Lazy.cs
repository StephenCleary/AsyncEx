using System.Threading;
using Nito.AsyncEx.Internal.PlatformEnlightenment;

namespace System.Threading
{
    public enum LazyThreadSafetyMode
    {
        None = 0,
        PublicationOnly = 1,
        ExecutionAndPublication = 2
    }
}

// ReSharper disable once CheckNamespace
namespace System
{
    public class Lazy<T>
    {
        public Boolean IsValueCreated
        {
            get
            {
                throw Enlightenment.Exception();
            }
        }

        public T Value
        {
            get
            {
                throw Enlightenment.Exception();
            }
        }

        public Lazy()
        {
            throw Enlightenment.Exception();
        }

        public Lazy(Boolean isThreadSafe)
        {
            throw Enlightenment.Exception();
        }

        public Lazy(Func<T> valueFactory)
        {
            throw Enlightenment.Exception();
        }

        public Lazy(Func<T> valueFactory, Boolean isThreadSafe)
        {
            throw Enlightenment.Exception();
        }

        public Lazy(Func<T> valueFactory, LazyThreadSafetyMode mode)
        {
            throw Enlightenment.Exception();
        }

        public Lazy(LazyThreadSafetyMode mode)
        {
            throw Enlightenment.Exception();
        }

        public override String ToString()
        {
            throw Enlightenment.Exception();
        }
    }
}

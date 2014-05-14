using System.Threading;
using System.Threading.Tasks;
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
        private readonly object _mutex;
        private readonly Func<T> _valueFactory;
        private Task<T> _result;

        public Boolean IsValueCreated
        {
            get
            {
                lock (_mutex)
                    return _result != null;
            }
        }

        public T Value
        {
            get
            {
                lock (_mutex)
                {
                    if (_result == null)
                        _result = CreateValueAsTask();
                }
                return _result.GetAwaiter().GetResult();
            }
        }

        public Lazy()
            : this(Activator.CreateInstance<T>, LazyThreadSafetyMode.ExecutionAndPublication)
        {
        }

        public Lazy(Boolean isThreadSafe)
            : this(Activator.CreateInstance<T>, isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None)
        {
        }

        public Lazy(Func<T> valueFactory)
            : this(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication)
        {
        }

        public Lazy(Func<T> valueFactory, Boolean isThreadSafe)
            : this(valueFactory, isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None)
        {
        }

        public Lazy(Func<T> valueFactory, LazyThreadSafetyMode mode)
        {
            if (mode != LazyThreadSafetyMode.ExecutionAndPublication)
                throw new NotImplementedException("Thread safety modes other than ExecutionAndPublication are not supported by this polyfill.");
            _valueFactory = valueFactory;
            _mutex = new object();
        }

        public Lazy(LazyThreadSafetyMode mode)
            : this(Activator.CreateInstance<T>, LazyThreadSafetyMode.ExecutionAndPublication)
        {
        }

#pragma warning disable 1998
        private async Task<T> CreateValueAsTask()
#pragma warning restore 1998
        {
            return _valueFactory();
        }

        public override String ToString()
        {
            return IsValueCreated ? Value.ToString() : "(value not created)";
        }
    }
}

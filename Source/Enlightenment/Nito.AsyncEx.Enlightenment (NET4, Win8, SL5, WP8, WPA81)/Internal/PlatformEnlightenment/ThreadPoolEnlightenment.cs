using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    public static class ThreadPoolEnlightenment
    {
        public static IDisposable RegisterWaitForSingleObject(WaitHandle handle, Action<object, bool> callback, object state, TimeSpan timeout)
        {
            throw Enlightenment.Exception();
        }
    }
}

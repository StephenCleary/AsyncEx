using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    partial class EnlightenmentProvider
    {
        private sealed class ThreadPoolEnlightenment : IThreadPoolEnlightenment
        {
            IDisposable IThreadPoolEnlightenment.RegisterWaitForSingleObject(WaitHandle handle, Action<object, bool> callback, object state, TimeSpan timeout)
            {
                return DefaultEnlightenmentProvider.ThreadPoolEnlightenment.RegisterWaitForSingleObjectFallback(handle, callback, state, timeout);
            }
        }
    }
}

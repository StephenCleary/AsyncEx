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
            var registration = ThreadPool.RegisterWaitForSingleObject(handle, (innerState, timedOut) => callback(innerState, timedOut), state, timeout, true);
            return new WaitHandleRegistration(registration);
        }

        private sealed class WaitHandleRegistration : IDisposable
        {
            private readonly RegisteredWaitHandle _registration;

            public WaitHandleRegistration(RegisteredWaitHandle registration)
            {
                _registration = registration;
            }

            public void Dispose()
            {
                _registration.Unregister(null);
            }
        }
    }
}

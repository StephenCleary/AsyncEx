using System;
using System.Threading;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    partial class EnlightenmentProvider
    {
        /// <summary>
        /// This enlightenment will work on most platforms (net4/sl4/wp75).
        /// However, it is not supported in portable libraries.
        /// </summary>
        private sealed class ThreadPoolEnlightenment : IThreadPoolEnlightenment
        {
            IDisposable IThreadPoolEnlightenment.RegisterWaitForSingleObject(WaitHandle handle, Action<object, bool> callback, object state, TimeSpan timeout)
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
}

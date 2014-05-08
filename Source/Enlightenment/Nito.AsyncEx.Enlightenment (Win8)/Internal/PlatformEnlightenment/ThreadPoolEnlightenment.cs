using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    public static class ThreadPoolEnlightenment
    {
        public static IDisposable RegisterWaitForSingleObject(WaitHandle handle, Action<object, bool> callback, object state, TimeSpan timeout)
        {
            var ret = new CancelWaitHandle();
            Task.Run(() =>
            {
                var result = WaitHandle.WaitAny(new[] { handle, ret.WaitHandle }, timeout);
                ret.SetCallbackThreadId(Enlightenment.ThreadIdentity.CurrentManagedThreadId);
                if (result == WaitHandle.WaitTimeout)
                    callback(state, true);
                else if (result == 0)
                    callback(state, false);
                ret.CallbackCompleted();
            });
            return ret;
        }

        private sealed class CancelWaitHandle : IDisposable
        {
            private readonly ManualResetEvent _mre = new ManualResetEvent(false);
            private int? _callbackThreadId;
            private readonly ManualResetEvent _completed = new ManualResetEvent(false);

            public WaitHandle WaitHandle { get { return _mre; } }

            void IDisposable.Dispose()
            {
                _mre.Set();
                int? callbackThreadId;
                lock (_mre)
                {
                    callbackThreadId = _callbackThreadId;
                }
                if (callbackThreadId != Enlightenment.ThreadIdentity.CurrentManagedThreadId)
                    _completed.WaitOne();
            }

            public void SetCallbackThreadId(int callbackThreadId)
            {
                lock (_mre)
                {
                    _callbackThreadId = callbackThreadId;
                }
            }

            public void CallbackCompleted()
            {
                _completed.Set();
            }
        }
    }
}

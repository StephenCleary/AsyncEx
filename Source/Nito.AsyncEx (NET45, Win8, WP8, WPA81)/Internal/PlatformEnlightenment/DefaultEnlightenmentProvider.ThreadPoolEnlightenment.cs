using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    partial class DefaultEnlightenmentProvider
    {
        /// <summary>
        /// The default thread pool enlightenment, which will use <c>ThreadPool</c> if it exists, falling back on manual tasks.
        /// </summary>
        public sealed class ThreadPoolEnlightenment : IThreadPoolEnlightenment
        {
            /// <summary>
            /// A delegate that will call <c>ThreadPool.RegisterWaitForSingleObject</c>.
            /// </summary>
            private readonly Func<WaitHandle, Action<object, bool>, object, TimeSpan, object> _registerWaitForSingleObject;

            /// <summary>
            /// A delegate that will call <c>RegisteredWaitHandle.Unregister</c>.
            /// </summary>
            private readonly Action<object> _unregister;

            /// <summary>
            /// Examines the current runtime and initializes the delegates appropriately.
            /// </summary>
            public ThreadPoolEnlightenment(IReflectionExpressionProvider r)
            {
                // Equivalent to:
                //  object RWFSO(WaitHandle handle, Action<object, bool> callback, object state, TimeSpan timeout)
                //  { return ThreadPool.RegisterWaitForSingleObject(handle, (innerState, timedOut) => callback(innerState, timedOut), state, timeout); }
                var handle = Expression.Parameter(typeof(WaitHandle), "handle");
                var callback = Expression.Parameter(typeof(Action<object, bool>), "callback");
                var state = Expression.Parameter(typeof(object), "state");
                var timeout = Expression.Parameter(typeof(TimeSpan), "timeout");
                var innerState = Expression.Parameter(typeof(object), "innerState");
                var timedOut = Expression.Parameter(typeof(bool), "timedOut");
                var lambda = r.Lambda(r.Type("System.Threading.WaitOrTimerCallback"), r.Invoke(callback, innerState, timedOut), innerState, timedOut);
                var rwfso = r.Call(r.Type("System.Threading.ThreadPool"), "RegisterWaitForSingleObject", handle, lambda, state, timeout);
                _registerWaitForSingleObject = r.Compile<Func<WaitHandle, Action<object, bool>, object, TimeSpan, object>>(rwfso, handle, callback, state, timeout);

                // Equivalent to:
                //  void Unregister(object registration)
                //  { ((RegisteredWaitHandle)registration).Unregister(null); }
                var registration = Expression.Parameter(typeof(object), "registration");
                var unregister = r.Call(r.Convert(registration, r.Type("System.Threading.RegisteredWaitHandle")), "Unregister", r.Constant(null, typeof(WaitHandle)));
                _unregister = r.Compile<Action<object>>(unregister, registration);
            }

            IDisposable IThreadPoolEnlightenment.RegisterWaitForSingleObject(WaitHandle handle, Action<object, bool> callback, object state, TimeSpan timeout)
            {
                if (_registerWaitForSingleObject == null || _unregister == null)
                    return RegisterWaitForSingleObjectFallback(handle, callback, state, timeout);

                var registration = _registerWaitForSingleObject(handle, callback, state, timeout);
                return new WaitHandleRegistration(_unregister, registration);
            }

            private sealed class WaitHandleRegistration : IDisposable
            {
                private readonly Action<object> _unregister;
                private readonly object _registration;

                public WaitHandleRegistration(Action<object> unregister, object registration)
                {
                    _unregister = unregister;
                    _registration = registration;
                }

                public void Dispose()
                {
                    _unregister(_registration);
                }
            }

            public static IDisposable RegisterWaitForSingleObjectFallback(WaitHandle handle, Action<object, bool> callback, object state, TimeSpan timeout)
            {
                var ret = new CancelWaitHandle();
                TaskShim.Run(() =>
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
}

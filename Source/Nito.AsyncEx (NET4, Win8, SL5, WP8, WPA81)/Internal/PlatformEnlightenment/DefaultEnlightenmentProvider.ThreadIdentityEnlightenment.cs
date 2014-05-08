using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    partial class DefaultEnlightenmentProvider
    {
        /// <summary>
        /// The default thread identity enlightenment, which will use <c>Environment.CurrentManagedThreadId</c> or <c>Thread.CurrentThread.ManagedThreadId</c>.
        /// </summary>
        public sealed class ThreadIdentityEnlightenment : IThreadIdentityEnlightenment
        {
            /// <summary>
            /// A delegate that will call <c>Environment.CurrentManagedThreadId</c>.
            /// </summary>
            private readonly Func<int> _environmentCurrentManagedThreadId;

            /// <summary>
            /// A delegate that will call <c>Thread.CurrentThread.ManagedThreadId</c>.
            /// </summary>
            private readonly Func<int> _threadCurrentThreadManagedThreadId;

            /// <summary>
            /// Examines the current runtime and initializes the delegates appropriately.
            /// </summary>
            public ThreadIdentityEnlightenment(IReflectionExpressionProvider r)
            {
                var currentManagedThreadId = r.Property(typeof(Environment), "CurrentManagedThreadId");
                _environmentCurrentManagedThreadId = r.Compile<Func<int>>(currentManagedThreadId);
                if (_environmentCurrentManagedThreadId != null)
                    return;

                var currentThread = r.Property(r.Type("System.Threading.Thread"), "CurrentThread");
                var managedThreadId = r.Property(currentThread, "ManagedThreadId");
                _threadCurrentThreadManagedThreadId = r.Compile<Func<int>>(managedThreadId);
            }

            int IThreadIdentityEnlightenment.CurrentManagedThreadId
            {
                get
                {
                    if (_environmentCurrentManagedThreadId != null)
                        return _environmentCurrentManagedThreadId();
                    if (_threadCurrentThreadManagedThreadId != null)
                        return _threadCurrentThreadManagedThreadId();
                    throw new NotSupportedException();
                }
            }
        }
    }
}

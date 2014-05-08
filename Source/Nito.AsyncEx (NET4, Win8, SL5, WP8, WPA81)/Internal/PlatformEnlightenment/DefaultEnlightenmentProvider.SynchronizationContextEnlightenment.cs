using System;
using System.Linq.Expressions;
using System.Threading;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    partial class DefaultEnlightenmentProvider
    {
        /// <summary>
        /// The default synchronization context enlightenment, which will use <c>SynchronizationContext.SetSynchronizationContext</c> if possible.
        /// </summary>
        public sealed class SynchronizationContextEnlightenment : ISynchronizationContextEnlightenment
        {
            /// <summary>
            /// A delegate that will call <c>SynchronizationContext.SetSynchronizationContext</c>.
            /// </summary>
            private readonly Action<SynchronizationContext> _setSynchronizationContext;

            /// <summary>
            /// Examines the current runtime and initializes the delegates appropriately.
            /// </summary>
            public SynchronizationContextEnlightenment(IReflectionExpressionProvider r)
            {
                var context = Expression.Parameter(typeof(SynchronizationContext), "context");
                var setSynchronizationContext = r.Call(typeof(SynchronizationContext), "SetSynchronizationContext", context);
                _setSynchronizationContext = r.Compile<Action<SynchronizationContext>>(setSynchronizationContext, context);
            }

            void ISynchronizationContextEnlightenment.SetCurrent(SynchronizationContext context)
            {
                // According to MSDN, SetSynchronizationContext is implemented on WP70, SL3, NET40, and Win8, so we should never see this exception.
                if (_setSynchronizationContext == null)
                    throw new NotSupportedException("SynchronizationContext.SetSynchronizationContext is not supported on this platform.");

                // However, partial-trust Silverlight may throw a MethodAccessException here.
                _setSynchronizationContext(context);
            }
        }
    }
}

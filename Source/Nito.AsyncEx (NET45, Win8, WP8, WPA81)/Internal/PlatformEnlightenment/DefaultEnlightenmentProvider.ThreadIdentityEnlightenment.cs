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
        /// The default thread identity enlightenment, which will use <c>Environment.CurrentManagedThreadId</c>.
        /// </summary>
        public sealed class ThreadIdentityEnlightenment : IThreadIdentityEnlightenment
        {
            int IThreadIdentityEnlightenment.CurrentManagedThreadId
            {
                get { return Environment.CurrentManagedThreadId; }
            }
        }
    }
}

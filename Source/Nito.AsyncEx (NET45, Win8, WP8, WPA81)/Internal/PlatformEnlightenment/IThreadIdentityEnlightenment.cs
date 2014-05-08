using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    /// <summary>
    /// A portable interface to the current thread.
    /// </summary>
    public interface IThreadIdentityEnlightenment
    {
        /// <summary>
        /// Gets the managed thread identifier for the current thread.
        /// </summary>
        int CurrentManagedThreadId { get; }
    }
}

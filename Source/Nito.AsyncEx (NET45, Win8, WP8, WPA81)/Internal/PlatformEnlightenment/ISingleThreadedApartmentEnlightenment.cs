using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    /// <summary>
    /// A portable interface to STA operations. Note that STA is not available on all platforms.
    /// </summary>
    public interface ISingleThreadedApartmentEnlightenment
    {
        object Start(Action execute, bool sta);

        Task JoinAsync(object thread);
    }
}

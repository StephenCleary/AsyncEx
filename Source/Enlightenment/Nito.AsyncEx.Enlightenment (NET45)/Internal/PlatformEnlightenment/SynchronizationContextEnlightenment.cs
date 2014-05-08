using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    public static class SynchronizationContextEnlightenment
    {
        public static void SetCurrentSynchronizationContext(SynchronizationContext context)
        {
            SynchronizationContext.SetSynchronizationContext(context);
        }
    }
}

using System.Threading;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    partial class EnlightenmentProvider
    {
        /// <summary>
        /// This enlightenment will work on all platforms (net40/win8/sl5/wp8/wpa81).
        /// However, it is not supported in portable libraries for: net40/win8/sl5/wp8/wpa81.
        /// </summary>
        private sealed class SynchronizationContextEnlightenment : ISynchronizationContextEnlightenment
        {
            void ISynchronizationContextEnlightenment.SetCurrent(SynchronizationContext context)
            {
                SynchronizationContext.SetSynchronizationContext(context);
            }
        }
    }
}

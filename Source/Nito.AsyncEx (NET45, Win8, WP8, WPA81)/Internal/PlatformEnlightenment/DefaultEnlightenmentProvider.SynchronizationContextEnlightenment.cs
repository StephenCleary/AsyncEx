using System.Threading;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    partial class DefaultEnlightenmentProvider
    {
        /// <summary>
        /// This enlightenment will work on all platforms (net4/win8/sl4/wp75).
        /// However, it is not supported in portable libraries for: net40/net403/sl5/wp70/wp71.
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

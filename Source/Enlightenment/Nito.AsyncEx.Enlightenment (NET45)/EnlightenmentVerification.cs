using Nito.AsyncEx.Internal.PlatformEnlightenment;

namespace Nito.AsyncEx
{
    /// <summary>
    /// Verifies platform enlightenment.
    /// </summary>
    public static class EnlightenmentVerification
    {
        /// <summary>
        /// Returns a value indicating whether the correct platform enlightenment provider has been loaded.
        /// </summary>
        public static bool EnsureLoaded()
        {
            return true;
        }
    }
}

using Nito.AsyncEx.Internal.PlatformEnlightenment;
using System;

namespace Nito.AsyncEx
{
    /// <summary>
    /// Provides helper (non-extension) methods dealing with exceptions.
    /// </summary>
    public static class ExceptionHelpers
    {
        /// <summary>
        /// Attempts to prepare the exception for re-throwing by preserving the stack trace. The returned exception should be immediately thrown.
        /// </summary>
        /// <param name="exception">The exception. May not be <c>null</c>.</param>
        /// <returns>The <see cref="Exception"/> that was passed into this method.</returns>
        public static Exception PrepareForRethrow(Exception exception)
        {
            return ExceptionEnlightenment.PrepareForRethrow(exception);
        }
    }
}

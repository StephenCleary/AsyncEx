using System;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    /// <summary>
    /// Provides exception-related services.
    /// </summary>
    public interface IExceptionEnlightenment
    {
        /// <summary>
        /// Attempts to prepare the exception for re-throwing by preserving the stack trace. Returns the exception passed into this method. The returned exception should be immediately thrown.
        /// </summary>
        /// <param name="exception">The exception. May not be <c>null</c>.</param>
        /// <returns>The <see cref="Exception"/> that was passed into this method.</returns>
        Exception PrepareForRethrow(Exception exception);
    }
}

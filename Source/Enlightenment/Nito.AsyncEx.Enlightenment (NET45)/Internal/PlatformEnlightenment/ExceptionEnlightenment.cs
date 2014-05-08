using System;
using System.Runtime.ExceptionServices;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    public static class ExceptionEnlightenment
    {
        public static Exception PrepareForRethrow(Exception exception)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
            return exception;
        }
    }
}

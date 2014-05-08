using System;
using System.Runtime.ExceptionServices;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    partial class EnlightenmentProvider
    {
        private sealed class ExceptionEnlightenment : IExceptionEnlightenment
        {
            Exception IExceptionEnlightenment.PrepareForRethrow(Exception exception)
            {
                ExceptionDispatchInfo.Capture(exception).Throw();
                return exception;
            }
        }
    }
}

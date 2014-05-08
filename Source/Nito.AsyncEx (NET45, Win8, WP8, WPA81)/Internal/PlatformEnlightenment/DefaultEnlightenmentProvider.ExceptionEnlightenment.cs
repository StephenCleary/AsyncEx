using System;
using System.Runtime.ExceptionServices;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    partial class DefaultEnlightenmentProvider
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

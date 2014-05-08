using System;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    public static class ExceptionEnlightenment
    {
        public static Exception PrepareForRethrow(Exception exception)
        {
            throw Enlightenment.Exception();
        }
    }
}

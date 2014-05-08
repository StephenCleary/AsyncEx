using System;
using System.Threading.Tasks;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    public static class AsyncEnlightenment
    {
        public static TaskCreationOptions AddDenyChildAttach(TaskCreationOptions options)
        {
            return options | TaskCreationOptions.DenyChildAttach;
        }
    }
}

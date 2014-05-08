using System.Threading.Tasks;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    partial class EnlightenmentProvider
    {
        private sealed class AsyncEnlightenment : IAsyncEnlightenment
        {
            TaskCreationOptions IAsyncEnlightenment.AddDenyChildAttach(TaskCreationOptions options)
            {
                return options | TaskCreationOptions.DenyChildAttach;
            }
        }
    }
}

using System.Threading;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    partial class EnlightenmentProvider
    {
        private sealed class ThreadIdentityEnlightenment : IThreadIdentityEnlightenment
        {
            int IThreadIdentityEnlightenment.CurrentManagedThreadId
            {
                get { return Thread.CurrentThread.ManagedThreadId; }
            }
        }
    }
}

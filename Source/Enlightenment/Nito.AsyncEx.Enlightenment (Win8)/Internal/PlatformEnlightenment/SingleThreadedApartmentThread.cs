using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    public sealed class SingleThreadedApartmentThread
    {
        private readonly Task _thread;

        public SingleThreadedApartmentThread(Action execute, bool sta)
        {
            if (sta)
                throw new NotSupportedException("STA threads are not supported on this platform.");
            _thread = Task.Factory.StartNew(execute, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public Task JoinAsync()
        {
            return _thread;
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    partial class EnlightenmentProvider
    {
        private sealed class SingleThreadedApartmentEnlightenment : ISingleThreadedApartmentEnlightenment
        {
            object ISingleThreadedApartmentEnlightenment.Start(Action execute, bool sta)
            {
                if (sta)
                    throw new NotSupportedException("STA threads are not supported on this platform.");
                return Task.Factory.StartNew(execute, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }

            Task ISingleThreadedApartmentEnlightenment.JoinAsync(object thread)
            {
                return (Task)thread;
            }
        }
    }
}

using System;
using System.Threading.Tasks;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    public sealed class SingleThreadedApartmentThread
    {
        public SingleThreadedApartmentThread(Action execute, bool sta)
        {
            throw Enlightenment.Exception();
        }

        public Task JoinAsync()
        {
            throw Enlightenment.Exception();
        }
    }
}

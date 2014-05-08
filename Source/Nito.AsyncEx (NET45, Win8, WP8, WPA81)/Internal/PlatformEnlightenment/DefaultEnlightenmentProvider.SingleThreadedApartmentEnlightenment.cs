using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    partial class DefaultEnlightenmentProvider
    {
        /// <summary>
        /// The default STA enlightenment, which will always use a thread pool thread.
        /// </summary>
        public sealed class SingleThreadedApartmentEnlightenment : ISingleThreadedApartmentEnlightenment
        {
            object ISingleThreadedApartmentEnlightenment.Start(Action execute, bool sta)
            {
                if (sta)
                    throw new NotSupportedException("The platform enlightenment assembly could not be found, so STA threads are not available.");
                return Task.Factory.StartNew(execute, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }

            Task ISingleThreadedApartmentEnlightenment.JoinAsync(object thread)
            {
                return (Task)thread;
            }
        }
    }
}

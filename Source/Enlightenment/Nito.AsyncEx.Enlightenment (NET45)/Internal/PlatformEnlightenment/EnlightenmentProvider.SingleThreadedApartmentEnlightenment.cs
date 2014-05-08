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
                return sta ? new ThreadTask(execute) : (object)Task.Factory.StartNew(execute, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }

            Task ISingleThreadedApartmentEnlightenment.JoinAsync(object thread)
            {
                var ret = thread as Task;
                if (ret != null)
                    return ret;
                return ((ThreadTask)thread).Task;
            }

            private sealed class ThreadTask
            {
                private readonly TaskCompletionSource _tcs;
                private readonly Thread _thread;

                public ThreadTask(Action execute)
                {
                    _tcs = new TaskCompletionSource();
                    _thread = new Thread(() =>
                    {
                        try
                        {
                            execute();
                        }
                        finally
                        {
                            _tcs.TrySetResultWithBackgroundContinuations();
                        }
                    });
                    _thread.SetApartmentState(ApartmentState.STA);
                    _thread.Name = "STA AsyncContextThread (Nito.AsyncEx)";
                    _thread.Start();
                }

                public Task Task { get { return _tcs.Task; } }
            }
        }
    }
}

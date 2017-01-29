## Overview

This is the `async`-ready equivalent of [[Barrier|http://msdn.microsoft.com/en-us/library/system.threading.barrier.aspx]], similar to Stephen Toub's [[AsyncBarrier|http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266932.aspx]].

An `AsyncBarrier` progresses through a series of **phases**, at each phase waiting for a number of **participants** to signal their completion. Once all participants for a phase have signaled, an optional post-phase action is executed, and then all the participants are released to the next phase.

The number of participants may be changed dynamically by calling `AddParticipants` or `RemoveParticipantsAsync`. For a participant to be removed, it must _not_ have signaled the end of the current phase. Participants may not be added or removed during the post-phase action. The task returned from `RemoveParticipantsAsync` will enter the `Completed` state when the current phase is completed; this task can just be ignored.

The post-phase action is executed on a thread pool thread, and it may be synchronous or asynchronous. The phase is not considered complete until the post-phase action has completed.

Each participant signals its completion of the current phase by calling `SignalAndWaitAsync`. The task returned from `SignalAndWaitAsync` will enter the `Completed` state when the current phase is completed.

## API

```C#
// An async-compatible barrier.
public sealed class AsyncBarrier
{
  // Creates an async-compatible barrier.
  public AsyncBarrier(int participants);

  // Creates an async-compatible barrier.
  public AsyncBarrier(int participants, Action<AsyncBarrier> postPhaseAction);

  // Creates an async-compatible barrier.
  public AsyncBarrier(int participants, Func<AsyncBarrier, Task> postPhaseAction);

  // Gets a semi-unique identifier for this asynchronous barrier.
  public int Id { get; }

  // Gets the current phase of the barrier.
  public long CurrentPhaseNumber { get; }

  // Gets the number of participants in this barrier.
  public int ParticipantCount { get; }

  // Gets the number of participants for this phase that have not yet signalled.
  public int ParticipantsRemaining { get; }

  // Signals a completion to this barrier and asynchronously waits for the phase to complete.
  // This method may not be called during the post-phase action.
  public Task SignalAndWaitAsync(int count = 1);

  // Adds participants to the barrier.
  // Returns the current phase.
  // This method may not be called during the post-phase action.
  public long AddParticipants(int count = 1);

  // Removes participants from the barrier.
  // These participants must not have signalled the barrier for this phase yet.
  // This method may not be called during the post-phase action.
  public Task RemoveParticipantsAsync(int count = 1);
}
```

## Platform Support

The full API is supported on all platforms.
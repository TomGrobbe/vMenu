using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Events;

/// <summary>Raises a watcher's event, one subscriber at a time.</summary>
internal static class Dispatch
{
    internal static void Raise<TPayload>(Action<TPayload>? subscribers, TPayload payload, string name)
        where TPayload : struct
    {
        if (subscribers is null)
        {
            return;
        }

        // Allocates a Delegate[] per raise. These fire on a state change rather than on every poll,
        // so that is a handful of arrays a minute, not one a frame.
        foreach (var subscriber in subscribers.GetInvocationList())
        {
            try
            {
                ((Action<TPayload>)subscriber)(payload);
            }
            catch (Exception exception)
            {
                Log.Error($"[Events] a {name} subscriber threw: {exception}");
            }
        }
    }

    internal static void RaiseAsync<TPayload>(Func<TPayload, Task>? subscribers, TPayload payload, string name)
        where TPayload : struct
    {
        if (subscribers is null)
        {
            return;
        }

        foreach (var subscriber in subscribers.GetInvocationList())
        {
            Task started;

            // Everything up to the handler's first await runs right here, so it needs catching
            // separately from whatever the returned task goes on to do.
            try
            {
                started = ((Func<TPayload, Task>)subscriber)(payload);
            }
            catch (Exception exception)
            {
                Log.Error($"[Events] a {name} subscriber threw: {exception}");

                continue;
            }

            // Deliberately not awaited: a handler waiting on a model load or a reply from the server
            // must not hold up the other handlers or the watcher's next poll.
            _ = Observe(started, name);
        }
    }

    /// <summary>Watches a task nobody is awaiting, so a failure is logged rather than lost.</summary>
    private static async Task Observe(Task task, string name)
    {
        try
        {
            await task;
        }
        catch (Exception exception)
        {
            Log.Error($"[Events] a {name} subscriber threw: {exception}");
        }
    }
}

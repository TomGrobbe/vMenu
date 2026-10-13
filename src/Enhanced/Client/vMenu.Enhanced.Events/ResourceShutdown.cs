using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Events;

public static class ResourceShutdown
{
    private const string StopEvent = "onResourceStop";

    private static bool _registered;

    // Raised once, when this resource is stopping.
    public static event Action? Stopping;

    // Call once at startup, with this resource's own name.
    public static void Initialize(string resource)
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        // The event fires for every resource on the server, so the name has to be checked. Reacting to
        // somebody else's restart by tearing down our own blips would be a strange bug to chase.
        API.OnEvent(StopEvent, new Action<string>(stopped =>
        {
            if (stopped == resource)
            {
                Raise();
            }
        }), false);
    }

    private static void Raise()
    {
        // Each handler is given its own chance. One throwing must not take the rest of the cleanup down with
        // it, because this is the last opportunity any of them get.
        foreach (var handler in Stopping?.GetInvocationList() ?? [])
        {
            try
            {
                ((Action)handler)();
            }
            catch (Exception exception)
            {
                Log.Error($"[Shutdown] A cleanup handler threw and was skipped: {exception}");
            }
        }
    }
}

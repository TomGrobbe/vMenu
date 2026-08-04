using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Actions;

namespace vMenu.Enhanced.Actions;

/// <summary>Asks the server to do something, and waits for its answer.</summary>
// The runtime has no request/response primitive, so a reply is matched to its request by an id this
// side chose. Handlers are registered imperatively because attribute discovery only scans the
// assembly named as the client_script, and this one is a project reference.
public static class ServerActions
{
    private const int TimeoutMs = 10000;

    private static readonly Dictionary<int, TaskCompletionSource<ActionResult>> Pending = [];

    private static int _lastRequestId;

    private static bool _registered;

    /// <summary>Call before building menus.</summary>
    public static void RegisterEventHandlers()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnNetEvent(ActionEvents.Result, new Action<int, int, string[]>(OnResult), false);
    }

    /// <summary>
    /// Runs an action on the server. Never throws for a refusal: everything that can go wrong,
    /// including no reply at all, comes back as an <see cref="ActionStatus"/>.
    /// </summary>
    public static async Task<ActionResult> InvokeAsync(string actionId, params string[] args)
    {
        if (!_registered)
        {
            // The reply would arrive with nothing listening, then be waited out to the timeout.
            API.Log.Error($"[Actions] '{actionId}' was invoked before RegisterEventHandlers ran.");

            return ActionResult.From(ActionStatus.Failed);
        }

        var requestId = ++_lastRequestId;

        var pending = new TaskCompletionSource<ActionResult>();

        Pending[requestId] = pending;

        try
        {
            API.EmitServer(ActionEvents.Invoke, actionId, requestId, args);

            var timeout = API.Delay(TimeoutMs);

            var finishedFirst = await Task.WhenAny(pending.Task, timeout);

            if (finishedFirst == timeout)
            {
                API.Log.Error($"[Actions] '{actionId}' got no answer within {TimeoutMs}ms.");

                return ActionResult.From(ActionStatus.Timeout);
            }

            return await pending.Task;
        }
        finally
        {
            Pending.Remove(requestId);
        }
    }

    private static void OnResult(int requestId, int status, string[] data)
    {
        if (!Pending.TryGetValue(requestId, out var pending))
        {
            // A reply that lost the race with the timeout. Its caller has already been told.
            API.Log.Debug($"[Actions] Reply {requestId} arrived with nothing waiting for it.");

            return;
        }

        pending.TrySetResult(new ActionResult((ActionStatus)status, data ?? []));
    }
}

using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;

namespace vMenu.Enhanced.Actions.Server;

public static class ActionRegistry
{
    private const string Ungated = "<ungated>";

    private const string DroppedEvent = "playerDropped";

    private static readonly Dictionary<string, RegisteredAction> Actions = new(StringComparer.Ordinal);

    private static bool _registered;

    public static void Register(
        string actionId,
        string permission,
        Func<Player, string[], Task<ActionResponse>> handler,
        ActionRateLimit? rateLimit = null)
    {
        if (!Actions.TryAdd(actionId, new RegisteredAction(permission, handler, rateLimit)))
        {
            Log.Error($"[Actions] '{actionId}' is registered twice. The second registration is ignored.");
        }
    }

    public static void Register(
        string actionId,
        string permission,
        Func<Player, string[], ActionResponse> handler,
        ActionRateLimit? rateLimit = null) =>
        Register(actionId, permission, (source, args) => Task.FromResult(handler(source, args)), rateLimit);

    public static void RegisterUngated(
        string actionId,
        Func<Player, string[], Task<ActionResponse>> handler,
        ActionRateLimit? rateLimit = null) =>
        Register(actionId, Ungated, handler, rateLimit);

    public static void RegisterUngated(
        string actionId,
        Func<Player, string[], ActionResponse> handler,
        ActionRateLimit? rateLimit = null) =>
        Register(actionId, Ungated, handler, rateLimit);

    public static void RegisterEventHandlers()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnNetEvent(ActionEvents.Invoke, new Action<Player, string, int, string[]>(OnInvoke), false);

        API.OnEvent(DroppedEvent, new Action<int, string?>(OnPlayerDropped), false);

        Log.Debug($"[Actions] Listening with {Actions.Count} action(s) registered.");
    }

    // A named method, not a lambda: the binder reads FromSourceAttribute off the delegate's MethodInfo,
    // and without it the Player binds to wire argument 0 and every argument after it shifts by one.
    // Hands off rather than awaiting, because the delegate the runtime binds has to be an Action, so a
    // handler that suspends does not hold up the events queued behind it. The client matches a reply to
    // its request by id and waits ten seconds for one.
    private static void OnInvoke([FromSource] Player source, string actionId, int requestId, string[] args) =>
        _ = InvokeAsync(source, actionId, requestId, args ?? []);

    // Nothing observes the returned task, so anything escaping this method would be lost rather than
    // reaching the runtime's own handler logging. Hence the outer catch.
    private static async Task InvokeAsync(Player source, string actionId, int requestId, string[] args)
    {
        try
        {
            if (!Actions.TryGetValue(actionId, out var action))
            {
                Log.Warning($"[Actions] {source} asked for '{actionId}', which nothing is registered for.");

                Reply(source, requestId, ActionStatus.UnknownAction, []);

                return;
            }

            if (!string.Equals(action.Permission, Ungated, StringComparison.Ordinal)
                && !ServerPermissions.IsPlayerAllowed(source, action.Permission))
            {
                // Not a warning: a stale menu reaches this as readily as somebody poking at the event.
                Log.Info($"[Actions] {source.Name} was denied '{actionId}': missing {action.Permission}.");

                Reply(source, requestId, ActionStatus.Denied, []);

                return;
            }

            if (action.RateLimit is { } rateLimit && !rateLimit.TryTake(source, out var retryAfter))
            {
                Reply(
                    source,
                    requestId,
                    ActionStatus.RateLimited,
                    [retryAfter.ToString(CultureInfo.InvariantCulture)]);

                return;
            }

            ActionResponse response;

            try
            {
                response = await action.Handler(source, args);
            }
            catch (Exception exception)
            {
                // Every other action, for every other player, is registered against this same event.
                Log.Error($"[Actions] '{actionId}' threw for {source.Name}: {exception}");

                response = ActionResponse.Failed();
            }

            Reply(source, requestId, response.Status, response.Data);
        }
        catch (Exception exception)
        {
            // No reply attempt: whatever just failed may well be the reply. The caller times out.
            Log.Error($"[Actions] Dispatching '{actionId}' for {source} failed: {exception}");
        }
    }

    private static void OnPlayerDropped([FromSource] int source, string? reason = null)
    {
        if (source <= 0)
        {
            return;
        }

        foreach (var action in Actions.Values)
        {
            action.RateLimit?.Forget(source);
        }
    }

    // A handler that awaited can outlive the player it was answering, and the emit asserts a handle that
    // is still connected.
    private static void Reply(Player source, int requestId, ActionStatus status, string[] data)
    {
        if (!Native.DoesPlayerExist(source.Handle.ToString(CultureInfo.InvariantCulture)))
        {
            Log.Debug($"[Actions] Dropping reply {requestId}: {source} is gone.");

            return;
        }

        API.EmitClient(source.Handle, ActionEvents.Result, requestId, (int)status, data);
    }

    private sealed record RegisteredAction(
        string Permission,
        Func<Player, string[], Task<ActionResponse>> Handler,
        ActionRateLimit? RateLimit);
}

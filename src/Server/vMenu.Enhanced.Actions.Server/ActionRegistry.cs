using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Permissions.Server;

namespace vMenu.Enhanced.Actions.Server;

/// <summary>
/// Server side of the action layer: one net event for every client requested action.
/// </summary>
/// <remarks>
/// The permission check lives here rather than in the handlers, so a new action cannot forget it.
/// Handlers are registered imperatively because attribute discovery only scans the assembly named as
/// the <c>server_script</c>, and this one is a project reference.
/// </remarks>
public static class ActionRegistry
{
    private static readonly Dictionary<string, RegisteredAction> Actions = new(StringComparer.Ordinal);

    private static bool _registered;

    /// <param name="handler">
    /// Runs on the main thread, having already passed the permission check. Anything it throws is
    /// logged and answered as <see cref="ActionStatus.Failed"/>.
    /// </param>
    public static void Register(string actionId, string permission, Func<Player, string[], ActionResponse> handler)
    {
        if (!Actions.TryAdd(actionId, new RegisteredAction(permission, handler)))
        {
            API.Log.Error($"[Actions] '{actionId}' is registered twice. The second registration is ignored.");
        }
    }

    /// <summary>Call after every action has been registered.</summary>
    public static void RegisterEventHandlers()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnNetEvent(ActionEvents.Invoke, new Action<Player, string, int, string[]>(OnInvoke), false);

        API.Log.Debug($"[Actions] Listening with {Actions.Count} action(s) registered.");
    }

    /// <summary>
    /// A named method, not a lambda: the binder reads <see cref="FromSourceAttribute"/> off the
    /// delegate's <c>MethodInfo</c>. Without it the <see cref="Player"/> binds to wire argument 0 and
    /// every argument after it shifts by one.
    /// </summary>
    private static void OnInvoke([FromSource] Player source, string actionId, int requestId, string[] args)
    {
        if (!Actions.TryGetValue(actionId, out var action))
        {
            API.Log.Warn($"[Actions] {source} asked for '{actionId}', which nothing is registered for.");

            Reply(source, requestId, ActionStatus.UnknownAction, []);

            return;
        }

        if (!ServerPermissions.IsPlayerAllowed(source, action.Permission))
        {
            // Not a warning: a stale menu reaches this as readily as somebody poking at the event.
            API.Log.Info($"[Actions] {source} was denied '{actionId}': missing {action.Permission}.");

            Reply(source, requestId, ActionStatus.Denied, []);

            return;
        }

        ActionResponse response;

        try
        {
            response = action.Handler(source, args ?? []);
        }
        catch (Exception exception)
        {
            // Every other action, for every other player, is registered against this same event.
            API.Log.Error($"[Actions] '{actionId}' threw for {source}: {exception}");

            response = ActionResponse.Failed();
        }

        Reply(source, requestId, response.Status, response.Data);
    }

    /// <summary>Asserts the main thread, which is where a net event handler runs. Do not await first.</summary>
    private static void Reply(Player source, int requestId, ActionStatus status, string[] data) =>
        API.EmitClient(source.Handle, ActionEvents.Result, requestId, (int)status, data);

    private sealed record RegisteredAction(string Permission, Func<Player, string[], ActionResponse> Handler);
}

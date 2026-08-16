using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Logging;
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
    private const string Ungated = "<ungated>";

    private static readonly Dictionary<string, RegisteredAction> Actions = new(StringComparer.Ordinal);

    private static bool _registered;

    public static void Register(
        string actionId,
        string permission,
        Func<Player, string[], Task<ActionResponse>> handler)
    {
        if (!Actions.TryAdd(actionId, new RegisteredAction(permission, handler)))
        {
            Log.Error($"[Actions] '{actionId}' is registered twice. The second registration is ignored.");
        }
    }

    public static void Register(string actionId, string permission, Func<Player, string[], ActionResponse> handler) =>
        Register(actionId, permission, (source, args) => Task.FromResult(handler(source, args)));

    public static void RegisterUngated(string actionId, Func<Player, string[], Task<ActionResponse>> handler) =>
        Register(actionId, Ungated, handler);

    public static void RegisterUngated(string actionId, Func<Player, string[], ActionResponse> handler) =>
        Register(actionId, Ungated, handler);

    public static void RegisterEventHandlers()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnNetEvent(ActionEvents.Invoke, new Action<Player, string, int, string[]>(OnInvoke), false);

        Log.Debug($"[Actions] Listening with {Actions.Count} action(s) registered.");
    }

    /// <summary>
    /// A named method, not a lambda: the binder reads <see cref="FromSourceAttribute"/> off the
    /// delegate's <c>MethodInfo</c>. Without it the <see cref="Player"/> binds to wire argument 0 and
    /// every argument after it shifts by one.
    /// </summary>
    /// <remarks>
    /// Hands off rather than awaiting, because the delegate the runtime binds has to be an
    /// <see cref="Action"/>. A handler that suspends therefore does not hold up the events queued
    /// behind it, and answers whenever it is done: the client matches a reply to its request by id
    /// and waits ten seconds for one.
    /// </remarks>
    private static void OnInvoke([FromSource] Player source, string actionId, int requestId, string[] args) =>
        _ = InvokeAsync(source, actionId, requestId, args ?? []);

    /// <summary>
    /// Nothing observes the returned task, so anything escaping this method would be lost rather
    /// than reaching the runtime's own handler logging. Hence the outer catch.
    /// </summary>
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

    /// <summary>
    /// A handler that awaited can outlive the player it was answering, and the emit asserts a
    /// handle that is still connected.
    /// </summary>
    private static void Reply(Player source, int requestId, ActionStatus status, string[] data)
    {
        if (!Native.DoesPlayerExist(source.Handle.ToString(CultureInfo.InvariantCulture)))
        {
            Log.Debug($"[Actions] Dropping reply {requestId}: {source} is gone.");

            return;
        }

        API.EmitClient(source.Handle, ActionEvents.Result, requestId, (int)status, data);
    }

    private sealed record RegisteredAction(string Permission, Func<Player, string[], Task<ActionResponse>> Handler);
}

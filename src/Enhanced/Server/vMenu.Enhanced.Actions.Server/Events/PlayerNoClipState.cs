using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Data.PlayerState;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Serialization.Server;
using vMenu.Enhanced.Webhooks.Server;

namespace vMenu.Enhanced.Actions.Server.Events;

public static class PlayerNoClipState
{
    private static readonly HashSet<int> Active = [];

    private static readonly HashSet<int> Granted = [];

    private static readonly HashSet<int> Forced = [];

    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnNetEvent(PlayerStateEvents.ReportNoClip, new Action<Player, bool>(OnReported), false);
    }

    public static bool IsActive(int serverId) => Active.Contains(serverId);

    public static bool IsGranted(int serverId) => Granted.Contains(serverId);

    // Almost always false.
    public static bool HasAny => Active.Count > 0 || Granted.Count > 0 || Forced.Count > 0;

    public static void SetGranted(int serverId, bool granted)
    {
        if (granted)
        {
            Granted.Add(serverId);

            return;
        }

        Granted.Remove(serverId);
    }

    public static void SetForced(int serverId, bool forced)
    {
        if (forced)
        {
            Forced.Add(serverId);

            return;
        }

        Forced.Remove(serverId);
    }

    public static void Prune(HashSet<int> connected)
    {
        if (Active.Count == 0 && Granted.Count == 0 && Forced.Count == 0)
        {
            return;
        }

        Active.RemoveWhere(serverId => !connected.Contains(serverId));

        Granted.RemoveWhere(serverId => !connected.Contains(serverId));

        Forced.RemoveWhere(serverId => !connected.Contains(serverId));
    }

    // A named method rather than a lambda, so the binder finds FromSourceAttribute on it. Without that
    // the Player binds to wire argument 0 and the flag shifts by one.
    private static void OnReported([FromSource] Player source, bool active)
    {
        if (active
            && !Granted.Contains(source.Handle)
            && !Forced.Contains(source.Handle)
            && !ServerPermissions.IsPlayerAllowed(source, Global.NoClip))
        {
            active = false;
        }

        if (!active)
        {
            Forced.Remove(source.Handle);
        }

        var changed = active ? Active.Add(source.Handle) : Active.Remove(source.Handle);

        if (!changed)
        {
            return;
        }

        ServerStateBags.SetPlayer(source.Handle, PlayerStateKeys.NoClip, active);

        WebhookLog.Action(WebhookActor.For(source), active ? "turned noclip on." : "turned noclip off.");

        Log.Debug($"[NoClip] {source.Name} ({source.Handle}) is {(active ? "now" : "no longer")} noclipping.");
    }
}

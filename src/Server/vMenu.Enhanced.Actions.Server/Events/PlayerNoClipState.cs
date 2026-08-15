using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.PlayerState;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Serialization.Server;

using MiscSettingsPermissions = vMenu.Enhanced.Data.Permissions.Menus.MiscSettings;

namespace vMenu.Enhanced.Actions.Server.Events;

/// <summary>
/// Who is currently in noclip, as far as the server is concerned.
/// </summary>
public static class PlayerNoClipState
{
    /// <summary>Everyone the server currently believes is noclipping.</summary>
    private static readonly HashSet<int> Active = [];

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

    /// <summary>Whether this player is noclipping.</summary>
    public static bool IsActive(int serverId) => Active.Contains(serverId);

    /// <summary>Whether anybody at all is, which is almost always false.</summary>
    public static bool HasAny => Active.Count > 0;

    /// <summary>
    /// Forgets anybody who is no longer connected.
    /// </summary>
    public static void Prune(HashSet<int> connected)
    {
        if (Active.Count == 0)
        {
            return;
        }

        Active.RemoveWhere(serverId => !connected.Contains(serverId));
    }

    /// <summary>
    /// A client saying it has entered or left noclip.
    /// </summary>
    /// <remarks>
    /// A named method rather than a lambda, so the binder finds <see cref="FromSourceAttribute" /> on
    /// it. Without that the <see cref="Player" /> binds to wire argument 0 and the flag shifts by one.
    /// </remarks>
    private static void OnReported([FromSource] Player source, bool active)
    {
        // Somebody without the permission cannot be in noclip, so a claim that they are is either a
        // stale message from a revoke or somebody trying it on. Either way the answer is the same:
        // they are visible. Not logged as a warning, because a revoke racing a toggle is normal.
        if (active && !ServerPermissions.IsPlayerAllowed(source, MiscSettingsPermissions.NoClip))
        {
            active = false;
        }

        var changed = active ? Active.Add(source.Handle) : Active.Remove(source.Handle);

        if (!changed)
        {
            return;
        }

        ServerStateBags.SetPlayer(source.Handle, PlayerStateKeys.NoClip, active);

        Log.Trace($"[NoClip] {source.Name} ({source.Handle}) is {(active ? "now" : "no longer")} noclipping.");
    }
}

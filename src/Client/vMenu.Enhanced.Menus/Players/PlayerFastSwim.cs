using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Players;

/// <summary>
/// The player swims as fast as the game will let them.
/// </summary>
public static class PlayerFastSwim
{
    /// <summary>The game clamps anything above this back down, so it is the real ceiling.</summary>
    private const float Fast = 1.49f;

    private const float Normal = 1f;

    private static bool _watching;

    /// <summary>What the player asked for and what the server allows, which together are the only answer.</summary>
    public static bool Enabled => UserDefaults.PlayerFastSwim.Value && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(PlayerOptionsPermissions.FastSwim);

    /// <summary>Call once at startup, before permissions have arrived.</summary>
    public static void Initialize()
    {
        ClientPermissions.PermissionsChanged += Apply;

        Apply();
    }

    public static void SetEnabled(bool enabled)
    {
        // The checkbox follows the permission, but a revoke can land between the two.
        if (enabled && !IsAllowed)
        {
            return;
        }

        UserDefaults.PlayerFastSwim.Value = enabled;

        Apply();
    }

    private static void Apply()
    {
        var on = Enabled;

        Watch(on);

        Native.SetSwimMultiplierForPlayer(Native.PlayerId(), on ? Fast : Normal);
    }

    private static void Watch(bool watching)
    {
        if (watching == _watching)
        {
            return;
        }

        _watching = watching;

        if (watching)
        {
            LocalPlayerTicks.PlayerPedIdChanged += OnPedChanged;

            return;
        }

        LocalPlayerTicks.PlayerPedIdChanged -= OnPedChanged;
    }

    // The multiplier travels with the ped's movement data rather than with the player, so a respawn
    // or a model swap hands back one swimming at normal speed.
    private static void OnPedChanged(PlayerPedIdChanged _) => Apply();
}

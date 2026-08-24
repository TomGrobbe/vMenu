using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerFastRun
{
    // The game clamps anything above this back down, so it is the real ceiling.
    private const float Fast = 1.49f;

    private const float Normal = 1f;

    private static bool _watching;

    public static bool Enabled => UserDefaults.PlayerFastRun.Value && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(PlayerOptionsPermissions.FastRun);

    // Call once at startup, before permissions have arrived.
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

        UserDefaults.PlayerFastRun.Value = enabled;

        Apply();
    }

    private static void Apply()
    {
        var on = Enabled;

        Watch(on);

        Native.SetRunSprintMultiplierForPlayer(Native.PlayerId(), on ? Fast : Normal);
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

    // The multiplier travels with the ped's movement data rather than with the player, so a respawn or a
    // model swap hands back one running at normal speed.
    private static void OnPedChanged(PlayerPedIdChanged _) => Apply();
}

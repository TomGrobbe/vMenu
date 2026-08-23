using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;

using NoClipState = vMenu.Enhanced.NoClip.NoClip;
using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerFreeze
{
    // Deliberately not a UserDefault: rejoining unable to move, with nothing saying why, is a trap.
    private static bool _wanted;

    private static bool _watching;

    public static bool Enabled => _wanted && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(PlayerOptionsPermissions.Freeze);

    public static void Initialize()
    {
        ClientPermissions.PermissionsChanged += Apply;

        Apply();
    }

    public static void SetEnabled(bool enabled)
    {
        if (enabled && !IsAllowed)
        {
            return;
        }

        _wanted = enabled;

        Apply();
    }

    public static void Reapply() => Apply();

    private static void Apply()
    {
        var on = Enabled;

        Watch(on);

        // Noclip moves the entity by writing this same flag. EntityReleased calls Reapply.
        if (NoClipState.IsActive)
        {
            return;
        }

        var ped = Native.PlayerPedId();

        if (ped == 0 || !Native.DoesEntityExist(ped))
        {
            return;
        }

        Native.FreezeEntityPosition(ped, on);
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
            LocalPlayerTicks.PlayerPedRevived += OnRevived;

            return;
        }

        LocalPlayerTicks.PlayerPedIdChanged -= OnPedChanged;
        LocalPlayerTicks.PlayerPedRevived -= OnRevived;
    }

    private static void OnPedChanged(PlayerPedIdChanged _) => Apply();

    private static void OnRevived(PlayerPedRevived _) => Apply();
}

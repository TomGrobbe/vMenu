using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerInvisible
{
    private static bool _watching;

    public static bool Enabled => UserDefaults.PlayerInvisible.Value && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(PlayerOptionsPermissions.Invisible);

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

        UserDefaults.PlayerInvisible.Value = enabled;

        Apply();
    }

    public static void Reapply() => Apply();

    private static void Apply()
    {
        var on = Enabled;

        Watch(on);

        var ped = API.PlayerPedId();

        if (ped == 0 || !API.DoesEntityExist(ped))
        {
            return;
        }

        API.SetEntityVisible(ped, !on, false);
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

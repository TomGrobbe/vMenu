using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerNoHelmet
{
    private const int DisableAutoEquipHelmetsInBikes = 380;

    private const int DisableAutoEquipHelmetsInAircraft = 381;

    private static bool _watching;

    public static bool Enabled => UserDefaults.PlayerNoHelmet.Value && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(PlayerOptionsPermissions.NoHelmet);

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

        UserDefaults.PlayerNoHelmet.Value = enabled;

        Apply();
    }

    private static void Apply()
    {
        var on = Enabled;

        Watch(on);

        var ped = Native.PlayerPedId();

        if (ped == 0 || !Native.DoesEntityExist(ped))
        {
            return;
        }

        Native.SetPedConfigFlag(ped, DisableAutoEquipHelmetsInBikes, on);
        Native.SetPedConfigFlag(ped, DisableAutoEquipHelmetsInAircraft, on);
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

    private static void OnPedChanged(PlayerPedIdChanged _) => Apply();
}

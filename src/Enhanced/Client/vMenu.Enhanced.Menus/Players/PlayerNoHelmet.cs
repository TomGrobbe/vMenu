using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Menus.Common;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerNoHelmet
{
    private const int DisableAutoEquipHelmetsInBikes = 380;

    private const int DisableAutoEquipHelmetsInAircraft = 381;

    public static readonly ToggleFeature Toggle = new(
        UserDefaults.PlayerNoHelmet,
        PlayerOptionsPermissions.NoHelmet,
        Apply,
        ToggleWatch.PedChanged);

    private static void Apply(bool on)
    {
        var ped = Native.PlayerPedId();

        if (ped == 0 || !Native.DoesEntityExist(ped))
        {
            return;
        }

        Native.SetPedConfigFlag(ped, DisableAutoEquipHelmetsInBikes, on);
        Native.SetPedConfigFlag(ped, DisableAutoEquipHelmetsInAircraft, on);
    }
}

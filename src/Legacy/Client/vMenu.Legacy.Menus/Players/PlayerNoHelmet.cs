using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Menus.Common;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

using vMenu.Legacy.Hosting;

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
        var ped = API.PlayerPedId();

        if (ped == 0 || !API.DoesEntityExist(ped))
        {
            return;
        }

        API.SetPedConfigFlag(ped, DisableAutoEquipHelmetsInBikes, on);
        API.SetPedConfigFlag(ped, DisableAutoEquipHelmetsInAircraft, on);
    }
}

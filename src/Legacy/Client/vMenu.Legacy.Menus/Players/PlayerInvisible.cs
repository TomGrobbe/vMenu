using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Menus.Common;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerInvisible
{
    public static readonly ToggleFeature Toggle = new(
        UserDefaults.PlayerInvisible,
        PlayerOptionsPermissions.Invisible,
        Apply,
        ToggleWatch.PedChanged | ToggleWatch.PedRevived);

    private static void Apply(bool on)
    {
        var ped = API.PlayerPedId();

        if (ped == 0 || !API.DoesEntityExist(ped))
        {
            return;
        }

        API.SetEntityVisible(ped, !on, false);
    }
}

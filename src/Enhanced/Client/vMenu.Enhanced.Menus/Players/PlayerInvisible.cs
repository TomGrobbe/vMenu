using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Menus.Common;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

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
        var ped = Native.PlayerPedId();

        if (ped == 0 || !Native.DoesEntityExist(ped))
        {
            return;
        }

        Native.SetEntityVisible(ped, !on, false);
    }
}

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Menus.Common;

using NoClipState = vMenu.Enhanced.NoClip.NoClip;
using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerFreeze
{
    // Deliberately not a UserDefault: rejoining unable to move, with nothing saying why, is a trap.
    public static readonly ToggleFeature Toggle = new(
        PlayerOptionsPermissions.Freeze,
        Apply,
        ToggleWatch.PedChanged | ToggleWatch.PedRevived);

    private static void Apply(bool on)
    {
        // Noclip moves the entity by writing this same flag. EntityReleased reapplies.
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
}

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Menus.Common;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerNoRagdoll
{
    private static readonly PedProtection.Claim Protection = PedProtection.Register();

    // Dying ragdolls the ped whatever the flag said, and getting back up in the same body keeps the
    // handle, so a ped change alone never catches it.
    public static readonly ToggleFeature Toggle = new(
        UserDefaults.PlayerNoRagdoll,
        PlayerOptionsPermissions.NoRagdoll,
        Apply,
        ToggleWatch.PedChanged | ToggleWatch.PedRevived);

    private static void Apply(bool on)
    {
        Protection.Set(on, PedProtections.NotKnockedOffBike);

        var ped = API.PlayerPedId();

        if (ped == 0 || !API.DoesEntityExist(ped))
        {
            return;
        }

        API.SetPedCanRagdoll(ped, !on);
    }
}

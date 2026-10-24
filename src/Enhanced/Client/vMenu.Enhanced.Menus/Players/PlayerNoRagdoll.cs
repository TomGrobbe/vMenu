using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Menus.Common;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

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

        var ped = Native.PlayerPedId();

        if (ped == 0 || !Native.DoesEntityExist(ped))
        {
            return;
        }

        Native.SetPedCanRagdoll(ped, !on);
    }
}

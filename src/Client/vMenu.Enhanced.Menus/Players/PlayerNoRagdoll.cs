using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerNoRagdoll
{
    private static readonly PedProtection.Claim Protection = PedProtection.Register();

    private static bool _watching;

    public static bool Enabled => UserDefaults.PlayerNoRagdoll.Value && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(PlayerOptionsPermissions.NoRagdoll);

    // Call once at startup, before permissions have arrived.
    public static void Initialize()
    {
        ClientPermissions.PermissionsChanged += Apply;

        Apply();
    }

    public static void SetEnabled(bool enabled)
    {
        // The checkbox follows the permission, but a revoke can land between the two.
        if (enabled && !IsAllowed)
        {
            return;
        }

        UserDefaults.PlayerNoRagdoll.Value = enabled;

        Apply();
    }

    private static void Apply()
    {
        var on = Enabled;

        Protection.Set(on, PedProtections.NotKnockedOffBike);

        Watch(on);

        var ped = Native.PlayerPedId();

        if (ped == 0 || !Native.DoesEntityExist(ped))
        {
            return;
        }

        Native.SetPedCanRagdoll(ped, !on);
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

    // A respawn hands the player a new ped, on the game's defaults, so the answer has to be written again.
    private static void OnPedChanged(PlayerPedIdChanged _) => Apply();

    // Dying ragdolls the ped whatever the flag said, and getting back up in the same body keeps the
    // handle, so the change above never fires for it.
    private static void OnRevived(PlayerPedRevived _) => Apply();
}

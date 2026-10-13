using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerGodMode
{
    private const PedProtections Protections =
        PedProtections.NotDraggedOut | PedProtections.NotShotInVehicle;

    private const int RepairIntervalMs = 1000;

    private static readonly PedProtection.Claim Protection = PedProtection.Register();

    private static TickHandle? _tick;

    public static bool Enabled => UserDefaults.PlayerGodMode.Value && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(PlayerOptionsPermissions.Godmode);

    // Call once at startup, before permissions have arrived.
    public static void Initialize()
    {
        ClientPermissions.PermissionsChanged += Apply;

        _tick = TickRegistry.Register(
            "Player.GodMode", Hold, TickRate.Every(RepairIntervalMs), () => Enabled);

        Apply();
    }

    public static void SetEnabled(bool enabled)
    {
        // The checkbox follows the permission, but a revoke can land between the two.
        if (enabled && !IsAllowed)
        {
            return;
        }

        UserDefaults.PlayerGodMode.Value = enabled;

        Apply();
    }

    // Writes the flag out again, for anything that has reset it behind vMenu's back.
    public static void Reapply()
    {
        Apply();

        PedProtection.Reapply();
    }

    private static void Apply()
    {
        var on = Enabled;

        Protection.Set(on, Protections);

        Write(on);

        if (!on)
        {
            ClearPedInvincibility();
        }

        _tick?.Reevaluate();
    }

    private static void Hold() => Write(true);

    private static void Write(bool on) =>
        Native.SetPlayerInvincibleKeepRagdollEnabled(Native.PlayerId(), on);

    private static void ClearPedInvincibility()
    {
        var ped = Native.PlayerPedId();

        if (ped != 0 && Native.DoesEntityExist(ped))
        {
            Native.SetEntityInvincible(ped, false, false);
        }
    }
}

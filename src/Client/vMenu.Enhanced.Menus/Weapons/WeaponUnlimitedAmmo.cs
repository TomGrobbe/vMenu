using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using WeaponOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeaponOptions;

namespace vMenu.Enhanced.Menus.Weapons;

public static class WeaponUnlimitedAmmo
{
    private static TickHandle? _tick;

    // Every weapon the tick has switched this on for, so it can be switched back off again. The game
    // keeps the flag on the weapon rather than on the ped, so stopping the tick stops it being set again
    // but leaves every weapon it already ran on firing forever.
    private static readonly HashSet<uint> Applied = [];

    public static bool Enabled => UserDefaults.WeaponsUnlimitedAmmo.Value && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(WeaponOptionsPermissions.UnlimitedAmmo);

    // Call once at startup, before permissions have arrived. A permission arriving or being revoked
    // re-runs every tick condition through the registry, so this does not subscribe to that itself.
    public static void Initialize() =>
        _tick = TickRegistry.Register("Weapons.UnlimitedAmmo", Apply, TickRate.PerFrame, () => Enabled);

    public static void SetEnabled(bool enabled)
    {
        // The checkbox follows the permission, but a revoke can land between the two.
        if (enabled && !IsAllowed)
        {
            return;
        }

        UserDefaults.WeaponsUnlimitedAmmo.Value = enabled;

        if (!enabled)
        {
            Clear();
        }

        _tick?.Reevaluate();
    }

    private static void Apply()
    {
        var ped = Native.PlayerPedId();

        // Told which weapon rather than left to the flag alone, because the game applies it to the one being
        // held and the player can switch at any moment.
        var weapon = Native.GetSelectedPedWeapon(ped);

        Native.SetPedInfiniteAmmo(ped, true, weapon);

        Applied.Add(weapon);
    }

    private static void Clear()
    {
        var ped = Native.PlayerPedId();

        foreach (var weapon in Applied)
        {
            Native.SetPedInfiniteAmmo(ped, false, weapon);
        }

        Applied.Clear();
    }
}

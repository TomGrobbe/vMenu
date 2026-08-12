using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using WeaponOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeaponOptions;

namespace vMenu.Enhanced.Menus.Weapons;

public static class WeaponNoReload
{
    /// <summary>
    /// Slow on purpose. This sets a flag the game keeps rather than a per frame effect, so the tick
    /// is only here to put it back after something else clears it, such as a model change.
    /// </summary>
    private const int KeepAliveMs = 500;

    private static TickHandle? _tick;

    /// <summary>What the player asked for and what the server allows, which together are the only answer.</summary>
    public static bool Enabled => UserDefaults.WeaponsNoReload.Value && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(WeaponOptionsPermissions.NoReload);

    /// <summary>Call once at startup, before permissions have arrived.</summary>
    public static void Initialize() =>
        _tick = TickRegistry.Register(
            "Weapons.NoReload",
            Apply,
            TickRate.Every(KeepAliveMs),
            () => Enabled,
            onStopped: Clear);

    public static void SetEnabled(bool enabled)
    {
        // The checkbox follows the permission, but a revoke can land between the two.
        if (enabled && !IsAllowed)
        {
            return;
        }

        UserDefaults.WeaponsNoReload.Value = enabled;

        _tick?.Reevaluate();
    }

    private static void Apply() => Native.SetPedInfiniteAmmoClip(Native.PlayerPedId(), true);

    // Cleared when the loop stops, otherwise switching the option off would leave the flag on until
    // the player changed ped.
    private static void Clear() => Native.SetPedInfiniteAmmoClip(Native.PlayerPedId(), false);
}

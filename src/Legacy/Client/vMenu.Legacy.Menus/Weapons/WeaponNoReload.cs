using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Menus.Common;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using WeaponOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeaponOptions;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Weapons;

public static class WeaponNoReload
{
    // Slow on purpose. This sets a flag the game keeps rather than a per frame effect, so the tick is
    // only here to put it back after something else clears it, such as a model change.
    private const int KeepAliveMs = 500;

    private static TickHandle? _tick;

    public static readonly ToggleFeature Toggle = new(
        UserDefaults.WeaponsNoReload,
        WeaponOptionsPermissions.NoReload,
        _ => _tick?.Reevaluate());

    // Call once at startup, before permissions have arrived.
    public static void Initialize()
    {
        _tick = TickRegistry.Register(
            "Weapons.NoReload",
            Hold,
            TickRate.Every(KeepAliveMs),
            () => Toggle.Enabled,
            onStopped: Clear);

        Toggle.Initialize();
    }

    private static void Hold() => API.SetPedInfiniteAmmoClip(API.PlayerPedId(), true);

    // Cleared when the loop stops, otherwise switching the option off would leave the flag on until the
    // player changed ped.
    private static void Clear() => API.SetPedInfiniteAmmoClip(API.PlayerPedId(), false);
}

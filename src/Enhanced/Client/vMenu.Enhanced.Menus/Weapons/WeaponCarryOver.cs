using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Menus.Weapons.Saved;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using WeaponLoadoutsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeaponLoadouts;

namespace vMenu.Enhanced.Menus.Weapons;

// Changing the model builds a whole new ped, so the weapons go with the old one. This reads them
// beforehand and hands them back afterwards. It cannot be driven by PlayerPedIdChanged or
// PlayerPedModelChanged: both are polled and both fire once the swap is over, by which point the ped
// holding the weapons no longer exists.
public static class WeaponCarryOver
{
    public static bool Enabled => UserDefaults.WeaponsKeepOnPedChange.Value && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(WeaponLoadoutsPermissions.KeepOnPedChange);

    public static void SetEnabled(bool enabled)
    {
        // The checkbox follows the permission, but a revoke can land between the two.
        if (enabled && !IsAllowed)
        {
            return;
        }

        UserDefaults.WeaponsKeepOnPedChange.Value = enabled;
    }

    // Null when the option is off, so the caller does no work and pays nothing. Kept in memory rather
    // than through WeaponLoadoutStore.SavePending: that writes a KVP because a player can disconnect
    // while dead and still expect their weapons back, but a model change is over in a few frames.
    public static WeaponLoadout? Capture() =>
        Enabled ? WeaponLoadoutStore.Capture(string.Empty) : null;

    public static async Task RestoreAsync(WeaponLoadout? carried)
    {
        if (carried is null)
        {
            return;
        }

        // Cleared first rather than appended to, so a model that comes holding something of its own does not
        // leave the player with more than they had. Permissions ignored because they were carrying these a
        // moment ago, and a refresh landing mid swap should not be what disarms them.
        var report = await WeaponLoadoutApply.ApplyAsync(carried, append: false, ignorePermissions: true);

        Log.Debug($"[Weapons] Carried {report.Given} weapon(s) through the ped change, {report.Skipped} skipped.");
    }
}

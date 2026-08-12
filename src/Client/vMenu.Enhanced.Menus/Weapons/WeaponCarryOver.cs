using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Menus.Weapons.Saved;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using WeaponLoadoutsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeaponLoadouts;

namespace vMenu.Enhanced.Menus.Weapons;

/// <summary>
/// Carrying the weapons a player is holding through a change of ped.
/// </summary>
/// <remarks>
/// Changing the model builds a whole new ped, so the weapons go with the old one. This reads them
/// beforehand and hands them back afterwards, which is what vMenu has always done, and is the same
/// shape the health and armour either side of the swap already use.
/// <para>
/// It cannot be driven by <c>PlayerPedIdChanged</c> or <c>PlayerPedModelChanged</c>: both are polled
/// and both fire once the swap is over, by which point the ped holding the weapons no longer exists.
/// </para>
/// </remarks>
public static class WeaponCarryOver
{
    /// <summary>What the player asked for and what the server allows, which together are the only answer.</summary>
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

    /// <summary>
    /// Reads what the player is carrying, to be handed back once the new ped exists.
    /// </summary>
    /// <returns>Null when the option is off, so the caller does no work and pays nothing.</returns>
    // Kept in memory rather than through WeaponLoadoutStore.SavePending. That writes a KVP because a
    // player can disconnect while dead and still expect their weapons back; a model change is over in
    // a few frames, so there is nothing to survive and no reason to serialise.
    public static WeaponLoadout? Capture() =>
        Enabled ? WeaponLoadoutStore.Capture(string.Empty) : null;

    /// <summary>Hands back what <see cref="Capture"/> read, onto whatever ped the player has now.</summary>
    public static async Task RestoreAsync(WeaponLoadout? carried)
    {
        if (carried is null)
        {
            return;
        }

        // Cleared first rather than appended to, so a model that comes holding something of its own
        // does not leave the player with more than they had.
        //
        // Permissions ignored because they were carrying these a moment ago, and a permission refresh
        // landing mid swap should not be what disarms them.
        var report = await WeaponLoadoutApply.ApplyAsync(carried, append: false, ignorePermissions: true);

        API.Log.Debug($"[Weapons] Carried {report.Given} weapon(s) through the ped change, {report.Skipped} skipped.");
    }
}

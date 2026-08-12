using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using WeaponLoadoutsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeaponLoadouts;

namespace vMenu.Enhanced.Menus.Weapons.Saved;

/// <summary>
/// Giving a player their weapons back after they respawn, and when they first arrive.
/// </summary>
// Driven by the death and revive events rather than by watching the player's health every frame,
// which is what vMenu used to do. PlayerPedRevived also says whether it was a respawn or a revive
// where they stood, and only the first of those takes the weapons away.
public static class WeaponLoadoutRespawn
{
    /// <summary>How long to wait for the player to be looking at the world before handing over anyway.</summary>
    private const int VisibleTimeoutMs = 60000;

    private const int VisibleCheckMs = 100;

    /// <summary>What the player asked for and what the server allows, which together are the only answer.</summary>
    public static bool Enabled => UserDefaults.WeaponLoadoutOnRespawn.Value && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(WeaponLoadoutsPermissions.EquipOnRespawn);

    /// <summary>Stops a later permission refresh handing the loadout out a second time.</summary>
    private static bool _restoredOnJoin;

    /// <summary>Call once at startup, before permissions have arrived.</summary>
    public static void Initialize()
    {
        LocalPlayerTicks.PlayerPedDied += OnDied;
        LocalPlayerTicks.PlayerPedRevivedAsync += OnRevivedAsync;
    }

    /// <summary>
    /// Hands the player their default loadout when they arrive. Call once, after permissions have
    /// landed. Later calls do nothing, so a permission refresh cannot repeat it.
    /// </summary>
    // Gated on the same setting and permission as the respawn restore, because to a player, joining
    // and respawning ask the same question: do I start out carrying my own weapons.
    public static async Task RestoreOnJoinAsync()
    {
        if (_restoredOnJoin)
        {
            return;
        }

        _restoredOnJoin = true;

        if (!Enabled)
        {
            return;
        }

        var name = UserDefaults.WeaponLoadoutDefaultName.Value;

        // No fallback to the pending loadout, unlike a respawn. Pending is whatever they were holding
        // when they last died, which on a fresh join is either nothing or left over from a session
        // that has since ended.
        if (name.Length == 0)
        {
            return;
        }

        // The weapon list decides which of the saved weapons this server still hands out, and
        // ApplyAsync skips every one of them while it is not here yet.
        await WeaponSync.WaitForFirstAsync();

        if (WeaponLoadoutStore.Load(name) is not { } loadout)
        {
            return;
        }

        await WaitUntilVisibleAsync();

        // Permissions enforced, unlike the respawn path: nothing was taken away here, so this is the
        // player being handed weapons fresh and the server's answer is the one that counts.
        var report = await WeaponLoadoutApply.ApplyAsync(loadout, append: true, ignorePermissions: false);

        API.Log.Debug($"[Weapons] Restored {report.Given} weapon(s) from loadout '{name}' on join, {report.Skipped} skipped.");
    }

    public static void SetEnabled(bool enabled)
    {
        // The checkbox follows the permission, but a revoke can land between the two.
        if (enabled && !IsAllowed)
        {
            return;
        }

        UserDefaults.WeaponLoadoutOnRespawn.Value = enabled;
    }

    /// <summary>Holds off until the player is actually standing in the world.</summary>
    // Weapons handed over during the loading screen or a player switch are wiped by the spawn that
    // follows, which is what makes a restore look like it did nothing at all. The dead check is here
    // for the respawn path: the revive event fires the moment the game clears the dead flag, which is
    // a good while before the player is standing anywhere.
    private static async Task WaitUntilVisibleAsync()
    {
        var deadline = Native.GetGameTimer() + VisibleTimeoutMs;

        while (!Native.NetworkIsSessionStarted()
            || Native.IsPlayerSwitchInProgress()
            || !Native.IsScreenFadedIn()
            || Native.IsEntityDead(Native.PlayerPedId(), false))
        {
            // Handed over even if the wait never came good. A player stuck on a black screen has
            // bigger problems, and weapons they might lose beat weapons they never get.
            if (Native.GetGameTimer() > deadline)
            {
                return;
            }

            await API.Delay(VisibleCheckMs);
        }
    }

    private static void OnDied(PlayerPedDied _)
    {
        if (!Enabled)
        {
            return;
        }

        // Taken even when a default loadout is set, so a player who has not set one still gets back
        // whatever they were carrying.
        WeaponLoadoutStore.SavePending();
    }

    private static async Task OnRevivedAsync(PlayerPedRevived revived)
    {
        // A revive where they stood never took the weapons away, so putting them back would only
        // duplicate what is already there.
        if (!revived.Respawned || !Enabled)
        {
            return;
        }

        var name = UserDefaults.WeaponLoadoutDefaultName.Value;

        var loadout = name.Length > 0
            ? WeaponLoadoutStore.Load(name)
            : WeaponLoadoutStore.LoadPending();

        // Falls back to what they were carrying when the named default has since been deleted, so
        // the option still does something rather than quietly nothing.
        loadout ??= WeaponLoadoutStore.LoadPending();

        if (loadout is null)
        {
            return;
        }

        // The same wait the join path uses. A respawn is not over when the dead flag clears: the game
        // is still placing the player, and anything written to their weapons before that is written
        // to a ped the spawn is about to rebuild. The weapons themselves tended to survive it, their
        // components and tint did not.
        await WaitUntilVisibleAsync();

        // Permissions ignored on purpose: this is handing back what they were already carrying a
        // moment ago, and a player should not lose it to a permission refresh mid death.
        var report = await WeaponLoadoutApply.ApplyAsync(loadout, append: true, ignorePermissions: true);

        WeaponLoadoutStore.ClearPending();

        API.Log.Debug($"[Weapons] Restored {report.Given} weapon(s) after respawn, {report.Skipped} skipped.");
    }
}

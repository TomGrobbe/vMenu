using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions;

namespace vMenu.Enhanced.Menus.Weapons.Saved;

// Given is what the player actually ended up with. Skipped is what was in the loadout but could not
// be handed over, either because the server no longer has it or because this player is not allowed it.
public readonly record struct ApplyReport(int Given, int Skipped);

public static class WeaponLoadoutApply
{
    private const string Unarmed = "weapon_unarmed";

    private const int ComponentTimeoutMs = 1000;

    // The natives do not always take on the frame they are called, but a weapon whose maximum is below
    // the saved number would never agree, so this gives up rather than spinning.
    private const int AmmoTimeoutMs = 500;

    // append keeps what the player is already carrying instead of clearing it first. ignorePermissions is
    // for handing back what they were carrying a moment ago, where the question was settled then.
    public static async Task<ApplyReport> ApplyAsync(WeaponLoadout loadout, bool append, bool ignorePermissions)
    {
        if (!append)
        {
            WeaponInventory.RemoveAll();
        }

        var given = 0;
        var skipped = 0;

        foreach (var saved in loadout.Weapons)
        {
            var hash = API.Hash(saved.SpawnName);

            // A weapon whose model has left the game files since the loadout was saved. Giving it would do
            // nothing at best, so it is named in the log and passed over.
            if (!Native.IsWeaponValid(hash))
            {
                Log.Debug($"[Weapons] Skipping '{saved.SpawnName}' from loadout '{loadout.Name}': this game no longer has that weapon.");

                skipped++;

                continue;
            }

            if (!ignorePermissions && !IsAllowed(saved.SpawnName))
            {
                skipped++;

                continue;
            }

            await GiveAsync(saved, hash);

            given++;
        }

        // Left unarmed rather than holding whatever came last, which would otherwise be a loaded weapon
        // pointed at whoever is standing there.
        Native.SetCurrentPedWeapon(Native.PlayerPedId(), API.Hash(Unarmed), true);

        return new ApplyReport(given, skipped);
    }

    // A weapon the owner has since taken out of config/weapons.json counts as not allowed, so a loadout
    // cannot be used to keep something the server has stopped handing out.
    private static bool IsAllowed(string spawnName) =>
        WeaponSync.Find(spawnName) is { } known
        && ClientWeaponPermissions.CanUseWeapon(known.SpawnName, known.Category);

    private static async Task GiveAsync(SavedWeapon saved, uint hash)
    {
        var ped = Native.PlayerPedId();

        // Handed over loaded rather than empty. A weapon given with no rounds is one the game has no reason
        // to build, and components asked of it in that state are the ones that go missing.
        Native.GiveWeaponToPed(ped, hash, WeaponInventory.MaxAmmo(hash), false, false);

        foreach (var component in saved.Components)
        {
            var componentHash = API.Hash(component);

            // A component this weapon no longer takes could never report as fitted, so without this the retry
            // below spends its whole second on it before moving on.
            if (!Native.DoesWeaponTakeWeaponComponent(hash, componentHash))
            {
                Log.Debug($"[Weapons] '{saved.SpawnName}' no longer takes '{component}', so it was left off.");

                continue;
            }

            if (!await FitAsync(ped, hash, componentHash))
            {
                Log.Debug($"[Weapons] '{component}' would not fit on '{saved.SpawnName}' within {ComponentTimeoutMs}ms.");
            }
        }

        if (saved.Tint > 0)
        {
            Native.SetPedWeaponTintIndex(ped, hash, saved.Tint);
        }

        await FillAsync(ped, hash);
    }

    // False when the component never took, which is the caller's cue to say so.
    private static async Task<bool> FitAsync(int ped, uint weaponHash, uint componentHash)
    {
        var deadline = Native.GetGameTimer() + ComponentTimeoutMs;

        Native.GiveWeaponComponentToPed(ped, weaponHash, componentHash);

        while (!Native.HasPedGotWeaponComponent(ped, weaponHash, componentHash))
        {
            if (Native.GetGameTimer() > deadline)
            {
                return false;
            }

            Native.GiveWeaponComponentToPed(ped, weaponHash, componentHash);

            await API.Delay(0);
        }

        return true;
    }

    // We're filling ammo to the max because MK2 weapons with custom ammo is a PITA to deal with
    // it never restores properly. Stupid game.
    private static async Task FillAsync(int ped, uint weaponHash)
    {
        var wanted = WeaponInventory.MaxAmmo(weaponHash);
        var deadline = Native.GetGameTimer() + AmmoTimeoutMs;

        Native.SetAmmoInClip(ped, weaponHash, Native.GetMaxAmmoInClip(ped, weaponHash, false));

        Native.SetPedAmmo(ped, weaponHash, wanted, false);

        // More than asked for is fine and is left alone. Unlimited ammo holds the count at the weapon's
        // maximum, and an equality check would force every weapon into the player's hands in turn.
        while (Native.GetAmmoInPedWeapon(ped, weaponHash) < wanted)
        {
            if (Native.GetGameTimer() > deadline)
            {
                return;
            }

            Native.SetCurrentPedWeapon(ped, weaponHash, true);
            Native.SetPedAmmo(ped, weaponHash, wanted, false);

            await API.Delay(0);
        }
    }

}

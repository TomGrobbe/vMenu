using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Permissions;

using WeaponOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeaponOptions;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Weapons;

// The verbs live here rather than in the menu so the rows stay declarative, and so the permission
// check sits next to the thing it guards instead of only on the row that happens to offer it.
public static class WeaponInventory
{
    private const string Unarmed = "weapon_unarmed";

    // What the game hands out when it is not told a number.
    private const int FallbackAmmo = 250;

    public static bool Has(uint weaponHash) =>
        NativeFixer.HasPedGotWeapon(API.PlayerPedId(), weaponHash, 0);

    public static int TintCount(uint weaponHash) =>
        Math.Max(1, API.GetWeaponTintCount(weaponHash));

    public static int Tint(uint weaponHash) =>
        API.GetPedWeaponTintIndex(API.PlayerPedId(), weaponHash);

    public static void SetTint(uint weaponHash, int tint)
    {
        if (!ClientPermissions.IsAllowed(WeaponOptionsPermissions.Modify) || !Has(weaponHash))
        {
            return;
        }

        API.SetPedWeaponTintIndex(API.PlayerPedId(), weaponHash, tint);
    }

    public static int MaxAmmo(uint weaponHash) =>
        NativeFixer.GetMaxAmmo(API.PlayerPedId(), weaponHash, out var ammo) ? ammo : FallbackAmmo;

    public static int Ammo(uint weaponHash) =>
        API.GetAmmoInPedWeapon(API.PlayerPedId(), weaponHash);

    // Gives the weapon full, or takes it away if it is already held.
    public static bool Toggle(uint weaponHash)
    {
        if (Has(weaponHash))
        {
            API.RemoveWeaponFromPed(API.PlayerPedId(), weaponHash);

            return false;
        }

        Give(weaponHash);

        return true;
    }

    public static void Give(uint weaponHash)
    {
        API.GiveWeaponToPed(API.PlayerPedId(), weaponHash, MaxAmmo(weaponHash), false, true);

        Refill(weaponHash);
    }

    // Does nothing when the weapon is not held, so nothing is given by accident.
    public static bool Refill(uint weaponHash)
    {
        if (!Has(weaponHash))
        {
            return false;
        }

        var ped = API.PlayerPedId();

        API.SetAmmoInClip(ped, weaponHash, API.GetMaxAmmoInClip(ped, weaponHash, false));
        NativeFixer.SetPedAmmo(ped, weaponHash, MaxAmmo(weaponHash), false);

        return true;
    }

    // Weapons this player is not allowed are skipped, whether they came with the game or with an addon.
    public static int GiveAll()
    {
        var given = 0;

        foreach (var (weapon, _) in Allowed())
        {
            Give(Hashing.Joaat(weapon));

            given++;
        }

        // Left unarmed rather than holding whatever happened to be given last, which would otherwise be a
        // loaded weapon pointed at whoever is standing there.
        API.SetCurrentPedWeapon(API.PlayerPedId(), Hashing.Joaat(Unarmed), true);

        return given;
    }

    public static void RemoveAll() => API.RemoveAllPedWeapons(API.PlayerPedId(), true);

    public static int RefillAll()
    {
        var refilled = 0;

        foreach (var (weapon, _) in Listed())
        {
            if (Refill(Hashing.Joaat(weapon)))
            {
                refilled++;
            }
        }

        return refilled;
    }

    // Walks the same list RefillAll does, so the two cannot cover different weapons.
    public static int SetAllAmmo(int ammo)
    {
        var ped = API.PlayerPedId();
        var changed = 0;

        foreach (var (weapon, _) in Listed())
        {
            var hash = Hashing.Joaat(weapon);

            if (!Has(hash))
            {
                continue;
            }

            NativeFixer.SetPedAmmo(ped, hash, Math.Min(ammo, MaxAmmo(hash)), false);

            changed++;
        }

        return changed;
    }

    public static bool HasComponent(uint weaponHash, uint componentHash) =>
        API.HasPedGotWeaponComponent(API.PlayerPedId(), weaponHash, componentHash);

    public static void ToggleComponent(uint weaponHash, uint componentHash)
    {
        if (!ClientPermissions.IsAllowed(WeaponOptionsPermissions.Modify) || !Has(weaponHash))
        {
            return;
        }

        var ped = API.PlayerPedId();

        if (HasComponent(weaponHash, componentHash))
        {
            API.RemoveWeaponComponentFromPed(ped, weaponHash, componentHash);
            return;
        }

        // Read back and re-applied around the swap: fitting a component resets the weapon's ammo, which
        // would quietly empty a gun the player had just filled.
        var reserve = API.GetAmmoInPedWeapon(ped, weaponHash);

        NativeFixer.GetAmmoInClip(ped, weaponHash, out var clip);

        API.GiveWeaponComponentToPed(ped, weaponHash, componentHash);

        API.SetAmmoInClip(ped, weaponHash, clip);
        NativeFixer.SetPedAmmo(ped, weaponHash, reserve, false);
    }

    internal static IEnumerable<(string SpawnName, string Category)> Listed()
    {
        foreach (var category in WeaponSync.Categories)
        {
            foreach (var weapon in category.Weapons)
            {
                yield return (weapon.SpawnName, category.Name);
            }
        }
    }

    internal static IEnumerable<(string SpawnName, string Category)> Allowed()
    {
        if (!ClientPermissions.IsAllowed(WeaponOptionsPermissions.Spawn))
        {
            yield break;
        }

        foreach (var listed in Listed())
        {
            if (ClientWeaponPermissions.CanUseWeapon(listed.SpawnName, listed.Category)
                && CatalogValidity.IsValidWeapon(listed.SpawnName))
            {
                yield return listed;
            }
        }
    }
}

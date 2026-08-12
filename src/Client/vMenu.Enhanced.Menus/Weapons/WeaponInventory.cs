using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Permissions;

using WeaponOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeaponOptions;

namespace vMenu.Enhanced.Menus.Weapons;

/// <summary>
/// Handing weapons out, taking them back, and everything done to one that is already held.
/// </summary>
// The verbs live here rather than in the menu so the rows stay declarative, and so the permission
// check sits next to the thing it guards instead of only on the row that happens to offer it.
public static class WeaponInventory
{
    private const string Unarmed = "weapon_unarmed";

    /// <summary>What the game hands out when it is not told a number.</summary>
    private const int FallbackAmmo = 250;

    public static bool Has(uint weaponHash) =>
        Native.HasPedGotWeapon(Native.PlayerPedId(), weaponHash, 0);

    public static int TintCount(uint weaponHash) =>
        Math.Max(1, Native.GetWeaponTintCount(weaponHash));

    public static int Tint(uint weaponHash) =>
        Native.GetPedWeaponTintIndex(Native.PlayerPedId(), weaponHash);

    public static void SetTint(uint weaponHash, int tint)
    {
        if (!ClientPermissions.IsAllowed(WeaponOptionsPermissions.Modify) || !Has(weaponHash))
        {
            return;
        }

        Native.SetPedWeaponTintIndex(Native.PlayerPedId(), weaponHash, tint);
    }

    public static int MaxAmmo(uint weaponHash) =>
        Native.GetMaxAmmo(Native.PlayerPedId(), weaponHash, out var ammo) ? ammo : FallbackAmmo;

    public static int Ammo(uint weaponHash) =>
        Native.GetAmmoInPedWeapon(Native.PlayerPedId(), weaponHash);

    /// <summary>Gives the weapon full, or takes it away if it is already held.</summary>
    public static bool Toggle(uint weaponHash)
    {
        if (Has(weaponHash))
        {
            Native.RemoveWeaponFromPed(Native.PlayerPedId(), weaponHash);

            return false;
        }

        Give(weaponHash);

        return true;
    }

    public static void Give(uint weaponHash)
    {
        Native.GiveWeaponToPed(Native.PlayerPedId(), weaponHash, MaxAmmo(weaponHash), false, true);

        Refill(weaponHash);
    }

    /// <summary>Tops a weapon up. Does nothing when it is not held, so nothing is given by accident.</summary>
    public static bool Refill(uint weaponHash)
    {
        if (!Has(weaponHash))
        {
            return false;
        }

        var ped = Native.PlayerPedId();

        Native.SetAmmoInClip(ped, weaponHash, Native.GetMaxAmmoInClip(ped, weaponHash, false));
        Native.SetPedAmmo(ped, weaponHash, MaxAmmo(weaponHash), false);

        return true;
    }

    /// <summary>
    /// Every weapon this player is allowed to have, full. Weapons they are not allowed are skipped,
    /// whether they came with the game or with an addon.
    /// </summary>
    public static int GiveAll()
    {
        var given = 0;

        foreach (var (weapon, category) in Allowed())
        {
            Give(API.Hash(weapon));

            given++;
        }

        // Left unarmed rather than holding whatever happened to be given last, which would otherwise
        // be a loaded weapon pointed at whoever is standing there.
        Native.SetCurrentPedWeapon(Native.PlayerPedId(), API.Hash(Unarmed), true);

        return given;
    }

    public static void RemoveAll() => Native.RemoveAllPedWeapons(Native.PlayerPedId(), true);

    /// <summary>Tops up every weapon already held.</summary>
    public static int RefillAll()
    {
        var refilled = 0;

        foreach (var (weapon, _) in Listed())
        {
            if (Refill(API.Hash(weapon)))
            {
                refilled++;
            }
        }

        return refilled;
    }

    /// <summary>
    /// Sets the same round count on every weapon already held. Walks the same list
    /// <see cref="RefillAll"/> does, so the two cannot cover different weapons.
    /// </summary>
    public static int SetAllAmmo(int ammo)
    {
        var ped = Native.PlayerPedId();
        var changed = 0;

        foreach (var (weapon, _) in Listed())
        {
            var hash = API.Hash(weapon);

            if (!Has(hash))
            {
                continue;
            }

            Native.SetPedAmmo(ped, hash, Math.Min(ammo, MaxAmmo(hash)), false);

            changed++;
        }

        return changed;
    }

    public static bool HasComponent(uint weaponHash, uint componentHash) =>
        Native.HasPedGotWeaponComponent(Native.PlayerPedId(), weaponHash, componentHash);

    /// <summary>Attaches the component, or takes it off if it is already on.</summary>
    public static void ToggleComponent(uint weaponHash, uint componentHash)
    {
        if (!ClientPermissions.IsAllowed(WeaponOptionsPermissions.Modify) || !Has(weaponHash))
        {
            return;
        }

        var ped = Native.PlayerPedId();

        if (HasComponent(weaponHash, componentHash))
        {
            Native.RemoveWeaponComponentFromPed(ped, weaponHash, componentHash);
            return;
        }

        // Read back and re-applied around the swap: fitting a component resets the weapon's ammo,
        // which would quietly empty a gun the player had just filled.
        var reserve = Native.GetAmmoInPedWeapon(ped, weaponHash);

        Native.GetAmmoInClip(ped, weaponHash, out var clip);

        Native.GiveWeaponComponentToPed(ped, weaponHash, componentHash);

        Native.SetAmmoInClip(ped, weaponHash, clip);
        Native.SetPedAmmo(ped, weaponHash, reserve, false);
    }

    /// <summary>Every listed weapon, paired with the category it came from.</summary>
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

    /// <summary>Every listed weapon this player is allowed to take out of the menu.</summary>
    internal static IEnumerable<(string SpawnName, string Category)> Allowed()
    {
        if (!ClientPermissions.IsAllowed(WeaponOptionsPermissions.Spawn))
        {
            yield break;
        }

        foreach (var listed in Listed())
        {
            if (ClientWeaponPermissions.CanUseWeapon(listed.SpawnName, listed.Category))
            {
                yield return listed;
            }
        }
    }
}

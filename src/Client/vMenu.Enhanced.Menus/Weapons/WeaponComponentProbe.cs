using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Weapons;

namespace vMenu.Enhanced.Menus.Weapons;

// Which components a weapon actually takes, asked of the game rather than declared anywhere. Probed
// on first open and kept, not built up front: the config lists a few hundred components and the menu
// holds a hundred weapons, so asking about every pair at startup would be tens of thousands of
// native calls for weapons nobody opens.
internal static class WeaponComponentProbe
{
    private static readonly Dictionary<uint, IReadOnlyList<WeaponComponentEntry>> Cache = [];

    internal static IReadOnlyList<WeaponComponentEntry> For(uint weaponHash)
    {
        if (Cache.TryGetValue(weaponHash, out var cached))
        {
            return cached;
        }

        var accepted = new List<WeaponComponentEntry>();

        foreach (var component in WeaponSync.Components)
        {
            if (Native.DoesWeaponTakeWeaponComponent(weaponHash, API.Hash(component.SpawnName)))
            {
                accepted.Add(component);
            }
        }

        Cache[weaponHash] = accepted;

        return accepted;
    }

    // Call when the component list is replaced, so a stale probe is not kept.
    internal static void Forget() => Cache.Clear();
}

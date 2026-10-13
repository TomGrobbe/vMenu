using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Weapons;

internal static class WeaponHashNames
{
    private static Dictionary<uint, (string Label, string SpawnName)>? _byHash;

    internal static string? Resolve(uint hash) =>
        Index().TryGetValue(hash, out var weapon) ? WeaponNames.Resolve(weapon.Label, weapon.SpawnName) : null;

    internal static void Forget() => _byHash = null;

    private static Dictionary<uint, (string Label, string SpawnName)> Index()
    {
        if (_byHash is not null)
        {
            return _byHash;
        }

        if (WeaponSync.Categories.Count == 0)
        {
            return [];
        }

        var index = new Dictionary<uint, (string Label, string SpawnName)>();

        foreach (var category in WeaponSync.Categories)
        {
            foreach (var weapon in category.Weapons)
            {
                if (!string.IsNullOrWhiteSpace(weapon.SpawnName))
                {
                    index[Hashing.Joaat(weapon.SpawnName)] = (weapon.Label, weapon.SpawnName);
                }
            }
        }

        _byHash = index;

        return index;
    }
}

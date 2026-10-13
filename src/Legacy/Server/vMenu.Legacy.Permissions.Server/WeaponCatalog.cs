using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Data.Permissions.Menus;
using vMenu.Enhanced.Data.Weapons;
using vMenu.Enhanced.Logging;

using vMenu.Enhanced.Serialization.Server;

namespace vMenu.Enhanced.Permissions.Server;

public static partial class WeaponCatalog
{
    internal const string ConfigFile = "config/weapons.json";

    private const string Unarmed = "weapon_unarmed";

    private static readonly List<WeaponCategory> Categories = [];

    internal static void Load(string? contents)
    {
        Categories.Clear();

        if (string.IsNullOrWhiteSpace(contents))
        {
            Log.Warning($"[Permissions] No {ConfigFile} found. The weapon options menu starts empty.");
            return;
        }

        JToken document;

        try
        {
            document = ServerJson.Parse(contents);
        }
        catch (JsonException exception)
        {
            Log.Error($"[Permissions] {ConfigFile} could not be parsed, so the weapon options menu starts empty: {exception.Message}");
            return;
        }

        if (document is not JObject root)
        {
            Log.Error($"[Permissions] {ConfigFile} has to hold a single object of categories, so the weapon options menu starts empty.");
            return;
        }

        Register(root);
    }

    private static void Register(JObject root)
    {
        var segments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var claimedWeapons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in root.Properties())
        {
            var name = property.Name.Trim();
            var segment = CategoryName.ToPermissionSegment(name);

            if (segment.Length == 0)
            {
                Log.Warning($"[Permissions] Skipping weapon category '{property.Name}': its name has no letters or digits in it, so it could never be granted.");
                continue;
            }

            var permission = WeaponCategories.ForCategory(segment);

            // A name matching one vMenu declares itself would quietly hijack that permission.
            if (PermissionRegistry.TryGet(permission, out _))
            {
                Log.Warning($"[Permissions] Skipping weapon category '{name}': '{permission}' is a permission vMenu already declares, so pick a different name.");
                continue;
            }

            if (!segments.Add(segment))
            {
                Log.Warning($"[Permissions] Skipping weapon category '{name}': another category already claims '{permission}'.");
                continue;
            }

            if (property.Value is not JObject weaponList)
            {
                Log.Warning($"[Permissions] Skipping weapon category '{name}': its value has to be a list of weapon spawn names and the text to show for them.");
                continue;
            }

            var weapons = Claim(weaponList, name, claimedWeapons);

            if (weapons.Count == 0)
            {
                Log.Warning($"[Permissions] Skipping weapon category '{name}': it has no weapons in it, so it would show up empty.");
                continue;
            }

            Categories.Add(new WeaponCategory { Name = name, Weapons = weapons });

            PermissionRegistry.RegisterDynamic(permission, ConfigFile);

            Log.Debug($"[Permissions] Weapon category '{name}' holds {weapons.Count} weapon(s) and is granted by '{permission}'.");
        }
    }

    private static List<WeaponEntry> Claim(JObject weapons, string category, HashSet<string> claimedWeapons)
    {
        var kept = new List<WeaponEntry>();

        foreach (var weapon in weapons.Properties())
        {
            if (weapon.Name.Trim().ToLowerInvariant() is not { Length: > 0 } spawnName)
            {
                continue;
            }

            if (spawnName == Unarmed)
            {
                Log.Warning($"[Permissions] Skipping '{Unarmed}' in weapon category '{category}': every player already has it, so there would be nothing to hand out.");
                continue;
            }

            if (!PermissionPath.IsValidSegment(spawnName))
            {
                Log.Warning($"[Permissions] Skipping '{spawnName}' in weapon category '{category}': only letters, digits and underscores are usable in a permission, so this one could never be whitelisted.");
                continue;
            }

            if (weapon.Value.Type != JTokenType.String)
            {
                Log.Warning($"[Permissions] Skipping '{spawnName}' in weapon category '{category}': the text to show for it has to be written in quotes.");
                continue;
            }

            if (!claimedWeapons.Add(spawnName))
            {
                Log.Warning($"[Permissions] '{spawnName}' is listed in more than one weapon category, so it stays in the first one.");
                continue;
            }

            var label = ((string?)weapon.Value)?.Trim();

            kept.Add(new WeaponEntry
            {
                SpawnName = spawnName,
                Label = string.IsNullOrEmpty(label) ? spawnName : label,
            });
        }

        return kept;
    }
}

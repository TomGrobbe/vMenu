using System.Text.Json;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Data.Permissions.Menus;
using vMenu.Enhanced.Data.Weapons;
using vMenu.Enhanced.Serialization.Server;

namespace vMenu.Enhanced.Permissions.Server;

public static class WeaponCatalog
{
    private const string ConfigFile = "config/weapons.json";

    private const string Unarmed = "weapon_unarmed";

    private static readonly JsonDocumentOptions ParseOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private static readonly List<WeaponCategory> Categories = [];

    private static string _payload = "[]";

    public static void LoadAndRegister()
    {
        Categories.Clear();
        _payload = "[]";

        var contents = Native.LoadResourceFile(Native.GetCurrentResourceName(), ConfigFile);

        if (string.IsNullOrWhiteSpace(contents))
        {
            API.Log.Info($"[Permissions] No {ConfigFile} found. The weapon options menu starts empty.");
            return;
        }

        JsonDocument document;

        try
        {
            document = JsonDocument.Parse(contents, ParseOptions);
        }
        catch (JsonException exception)
        {
            API.Log.Error($"[Permissions] {ConfigFile} could not be parsed, so the weapon options menu starts empty: {exception.Message}");
            return;
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                API.Log.Error($"[Permissions] {ConfigFile} has to hold a single object of categories, so the weapon options menu starts empty.");
                return;
            }

            Register(document.RootElement);
        }

        _payload = ServerJson.Serialize(Categories);
    }

    
    public static void RegisterEventHandlers() =>
        API.OnNetEvent(WeaponEvents.Request, new Action<Player>(OnRequested), false);

    private static void OnRequested([FromSource] Player source) =>
        API.EmitClient(source.Handle, WeaponEvents.Set, _payload, WeaponComponentCatalog.Payload);

    private static void Register(JsonElement root)
    {
        var segments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var claimedWeapons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in root.EnumerateObject())
        {
            var name = property.Name.Trim();
            var segment = CategoryName.ToPermissionSegment(name);

            if (segment.Length == 0)
            {
                API.Log.Warn($"[Permissions] Skipping weapon category '{property.Name}': its name has no letters or digits in it, so it could never be granted.");
                continue;
            }

            var permission = WeaponCategories.ForCategory(segment);

            // A name matching one vMenu declares itself would quietly hijack that permission.
            if (PermissionRegistry.TryGet(permission, out _))
            {
                API.Log.Warn($"[Permissions] Skipping weapon category '{name}': '{permission}' is a permission vMenu already declares, so pick a different name.");
                continue;
            }

            if (!segments.Add(segment))
            {
                API.Log.Warn($"[Permissions] Skipping weapon category '{name}': another category already claims '{permission}'.");
                continue;
            }

            if (property.Value.ValueKind != JsonValueKind.Object)
            {
                API.Log.Warn($"[Permissions] Skipping weapon category '{name}': its value has to be a list of weapon spawn names and the text to show for them.");
                continue;
            }

            var weapons = Claim(property.Value, name, claimedWeapons);

            if (weapons.Count == 0)
            {
                API.Log.Warn($"[Permissions] Skipping weapon category '{name}': it has no weapons in it, so it would show up empty.");
                continue;
            }

            Categories.Add(new WeaponCategory { Name = name, Weapons = weapons });

            PermissionRegistry.RegisterDynamic(permission, ConfigFile);

            API.Log.Info($"[Permissions] Weapon category '{name}' holds {weapons.Count} weapon(s) and is granted by '{permission}'.");
        }
    }

    private static List<WeaponEntry> Claim(JsonElement weapons, string category, HashSet<string> claimedWeapons)
    {
        var kept = new List<WeaponEntry>();

        foreach (var weapon in weapons.EnumerateObject())
        {
            if (weapon.Name.Trim().ToLowerInvariant() is not { Length: > 0 } spawnName)
            {
                continue;
            }

            if (spawnName == Unarmed)
            {
                API.Log.Warn($"[Permissions] Skipping '{Unarmed}' in weapon category '{category}': every player already has it, so there would be nothing to hand out.");
                continue;
            }

            if (!PermissionPath.IsValidSegment(spawnName))
            {
                API.Log.Warn($"[Permissions] Skipping '{spawnName}' in weapon category '{category}': only letters, digits and underscores are usable in a permission, so this one could never be whitelisted.");
                continue;
            }

            if (weapon.Value.ValueKind != JsonValueKind.String)
            {
                API.Log.Warn($"[Permissions] Skipping '{spawnName}' in weapon category '{category}': the text to show for it has to be written in quotes.");
                continue;
            }

            if (!claimedWeapons.Add(spawnName))
            {
                API.Log.Warn($"[Permissions] '{spawnName}' is listed in more than one weapon category, so it stays in the first one.");
                continue;
            }

            var label = weapon.Value.GetString()?.Trim();

            kept.Add(new WeaponEntry
            {
                SpawnName = spawnName,
                Label = string.IsNullOrEmpty(label) ? spawnName : label,
            });
        }

        return kept;
    }
}

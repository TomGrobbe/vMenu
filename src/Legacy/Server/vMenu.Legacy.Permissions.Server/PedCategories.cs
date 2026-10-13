using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using vMenu.Enhanced.Data.PedModels;
using vMenu.Enhanced.Data.Permissions.Menus;
using vMenu.Enhanced.Logging;

using vMenu.Enhanced.Serialization.Server;

namespace vMenu.Enhanced.Permissions.Server;

// Unlike vehicles there is no native that lists ped models, so the whole list comes out of the config
// file. Each category gets a permission of its own, registered the same way the vehicle spawner's
// custom categories are.
public static partial class PedCategories
{
    internal const string ConfigFile = "config/ped-models.json";

    // Tolerant on purpose: server owners hand-edit this file.
    private static readonly List<PedModelCategory> Categories = [];

    // A missing or unreadable file just means the menu has nothing in it.
    internal static void Load(string? contents)
    {
        Categories.Clear();

        if (string.IsNullOrWhiteSpace(contents))
        {
            Log.Warning($"[Permissions] No {ConfigFile} found. The ped models menu starts empty.");
            return;
        }

        JToken document;

        try
        {
            document = ServerJson.Parse(contents);
        }
        catch (JsonException exception)
        {
            Log.Error($"[Permissions] {ConfigFile} could not be parsed, so the ped models menu starts empty: {exception.Message}");
            return;
        }

        if (document is not JObject root)
        {
            Log.Error($"[Permissions] {ConfigFile} has to hold a single object of categories, so the ped models menu starts empty.");
            return;
        }

        Register(root);
    }

    private static void Register(JObject root)
    {
        var segments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var claimedModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in root.Properties())
        {
            var name = property.Name.Trim();
            var segment = CategoryName.ToPermissionSegment(name);

            if (segment.Length == 0)
            {
                Log.Warning($"[Permissions] Skipping ped category '{property.Name}': its name has no letters or digits in it, so it could never be granted.");
                continue;
            }

            var permission = PedModelCategories.ForCategory(segment);

            // A name matching one vMenu declares itself would quietly hijack that permission.
            if (PermissionRegistry.TryGet(permission, out _))
            {
                Log.Warning($"[Permissions] Skipping ped category '{name}': '{permission}' is a permission vMenu already declares, so pick a different name.");
                continue;
            }

            if (!segments.Add(segment))
            {
                Log.Warning($"[Permissions] Skipping ped category '{name}': another category already claims '{permission}'.");
                continue;
            }

            if (property.Value is not JObject pedList)
            {
                Log.Warning($"[Permissions] Skipping ped category '{name}': its value has to be a list of ped model names and the text to show for them.");
                continue;
            }

            var peds = Claim(pedList, name, claimedModels);

            if (peds.Count == 0)
            {
                Log.Warning($"[Permissions] Skipping ped category '{name}': it has no peds in it, so it would show up empty.");
                continue;
            }

            Categories.Add(new PedModelCategory { Name = name, Peds = peds });

            PermissionRegistry.RegisterDynamic(permission, ConfigFile);

            Log.Debug($"[Permissions] Ped category '{name}' holds {peds.Count} ped(s) and is granted by '{permission}'.");
        }
    }

    private static List<PedModelEntry> Claim(JObject peds, string category, HashSet<string> claimedModels)
    {
        var kept = new List<PedModelEntry>();

        foreach (var ped in peds.Properties())
        {
            if (ped.Name.Trim().ToLowerInvariant() is not { Length: > 0 } model)
            {
                continue;
            }

            if (ped.Value.Type != JTokenType.String)
            {
                Log.Warning($"[Permissions] Skipping '{model}' in ped category '{category}': the text to show for it has to be written in quotes.");
                continue;
            }

            if (!claimedModels.Add(model))
            {
                Log.Warning($"[Permissions] '{model}' is listed in more than one ped category, so it stays in the first one.");
                continue;
            }

            // An empty label leaves the row with nothing beside the model name, which reads as a bug.
            var label = ((string?)ped.Value)?.Trim();

            kept.Add(new PedModelEntry
            {
                Model = model,
                Label = string.IsNullOrEmpty(label) ? model : label,
            });
        }

        return kept;
    }
}

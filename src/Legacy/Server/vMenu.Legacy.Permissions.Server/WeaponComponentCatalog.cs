using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Weapons;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization.Server;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Permissions.Server;

public static class WeaponComponentCatalog
{
    private const string ConfigFile = "config/weapon-components.json";

    private static readonly List<WeaponComponentEntry> Components = [];

    public static string Payload { get; private set; } = "[]";

    public static void Load()
    {
        Components.Clear();
        Payload = "[]";

        var contents = API.LoadResourceFile(API.GetCurrentResourceName(), ConfigFile);

        if (string.IsNullOrWhiteSpace(contents))
        {
            Log.Warning($"[Permissions] No {ConfigFile} found. No weapon offers any components.");
            return;
        }

        JToken document;

        try
        {
            document = ServerJson.Parse(contents);
        }
        catch (JsonException exception)
        {
            Log.Error($"[Permissions] {ConfigFile} could not be parsed, so no weapon offers any components: {exception.Message}");
            return;
        }

        if (document is not JObject root)
        {
            Log.Error($"[Permissions] {ConfigFile} has to hold a single object of component names, so no weapon offers any components.");
            return;
        }

        Read(root);

        Payload = ServerJson.Serialize(Components);

        if (Components.Count > 0)
        {
            Log.Debug($"[Permissions] Loaded {Components.Count} weapon component(s) from '{ConfigFile}'.");
        }
    }

    private static void Read(JObject root)
    {
        var claimed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var component in root.Properties())
        {
            if (component.Name.Trim().ToUpperInvariant() is not { Length: > 0 } spawnName)
            {
                continue;
            }

            if (component.Value.Type != JTokenType.String)
            {
                Log.Warning($"[Permissions] Skipping component '{spawnName}': the text to show for it has to be written in quotes.");
                continue;
            }

            if (!claimed.Add(spawnName))
            {
                Log.Warning($"[Permissions] '{spawnName}' is listed more than once in {ConfigFile}, so only the first one is used.");
                continue;
            }

            var label = ((string?)component.Value)?.Trim();

            Components.Add(new WeaponComponentEntry
            {
                SpawnName = spawnName,
                Label = string.IsNullOrEmpty(label) ? spawnName : label,
            });
        }
    }
}

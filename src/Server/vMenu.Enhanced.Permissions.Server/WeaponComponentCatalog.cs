using System.Text.Json;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Data.Weapons;
using vMenu.Enhanced.Serialization.Server;

namespace vMenu.Enhanced.Permissions.Server;

public static class WeaponComponentCatalog
{
    private const string ConfigFile = "config/weapon-components.json";

    private static readonly JsonDocumentOptions ParseOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private static readonly List<WeaponComponentEntry> Components = [];

    public static string Payload { get; private set; } = "[]";

    public static void Load()
    {
        Components.Clear();
        Payload = "[]";

        var contents = Native.LoadResourceFile(Native.GetCurrentResourceName(), ConfigFile);

        if (string.IsNullOrWhiteSpace(contents))
        {
            API.Log.Info($"[Permissions] No {ConfigFile} found. No weapon offers any components.");
            return;
        }

        JsonDocument document;

        try
        {
            document = JsonDocument.Parse(contents, ParseOptions);
        }
        catch (JsonException exception)
        {
            API.Log.Error($"[Permissions] {ConfigFile} could not be parsed, so no weapon offers any components: {exception.Message}");
            return;
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                API.Log.Error($"[Permissions] {ConfigFile} has to hold a single object of component names, so no weapon offers any components.");
                return;
            }

            Read(document.RootElement);
        }

        Payload = ServerJson.Serialize(Components);

        if (Components.Count > 0)
        {
            API.Log.Info($"[Permissions] Loaded {Components.Count} weapon component(s) from '{ConfigFile}'.");
        }
    }

    private static void Read(JsonElement root)
    {
        var claimed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var component in root.EnumerateObject())
        {
            if (component.Name.Trim().ToUpperInvariant() is not { Length: > 0 } spawnName)
            {
                continue;
            }

            if (component.Value.ValueKind != JsonValueKind.String)
            {
                API.Log.Warn($"[Permissions] Skipping component '{spawnName}': the text to show for it has to be written in quotes.");
                continue;
            }

            if (!claimed.Add(spawnName))
            {
                API.Log.Warn($"[Permissions] '{spawnName}' is listed more than once in {ConfigFile}, so only the first one is used.");
                continue;
            }

            var label = component.Value.GetString()?.Trim();

            Components.Add(new WeaponComponentEntry
            {
                SpawnName = spawnName,
                Label = string.IsNullOrEmpty(label) ? spawnName : label,
            });
        }
    }
}

using System.Globalization;
using System.Text.Json;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization.Server;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class VehicleExtraActions
{
    private const string ConfigFile = "config/extras.json";

    private static readonly JsonDocumentOptions ParseOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private static readonly Dictionary<string, Dictionary<string, string>> Labels = [];

    private static string _payload = "{}";

    public static void Register()
    {
        Load();

        API.OnNetEvent(VehicleEvents.ExtraLabelsRequest, new Action<Player>(OnRequested), false);
    }

    private static void OnRequested([FromSource] Player source) =>
        API.EmitClient(source.Handle, VehicleEvents.ExtraLabelsSet, _payload);

    private static void Load()
    {
        Labels.Clear();
        _payload = "{}";

        var contents = Native.LoadResourceFile(Native.GetCurrentResourceName(), ConfigFile);

        if (string.IsNullOrWhiteSpace(contents))
        {
            Log.Info($"[Config] No {ConfigFile} found, so vehicle extras keep their numbered names.");
            return;
        }

        JsonDocument document;

        try
        {
            document = JsonDocument.Parse(contents, ParseOptions);
        }
        catch (JsonException exception)
        {
            Log.Error($"[Config] {ConfigFile} could not be parsed, so vehicle extras keep their numbered names: {exception.Message}");
            return;
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                Log.Error($"[Config] {ConfigFile} has to hold a single object of vehicle model names, so vehicle extras keep their numbered names.");
                return;
            }

            Read(document.RootElement);
        }

        _payload = ServerJson.Serialize(Labels);

        Log.Debug($"[Config] Extra labels loaded for {Labels.Count} vehicle model(s) from {ConfigFile}.");
    }

    // Walked, not deserialized: a dictionary keeps the last of two duplicate keys silently.
    private static void Read(JsonElement root)
    {
        foreach (var vehicle in root.EnumerateObject())
        {
            if (vehicle.Name.Trim() is not { Length: > 0 } model)
            {
                continue;
            }

            if (vehicle.Value.ValueKind != JsonValueKind.Object)
            {
                Log.Warning($"[Config] Skipping '{model}' in {ConfigFile}: its extras have to be written as an object of id to name.");
                continue;
            }

            var key = model.ToLowerInvariant();

            if (Labels.ContainsKey(key))
            {
                Log.Warning($"[Config] '{model}' is listed more than once in {ConfigFile}, so only the first block of names is used.");
                continue;
            }

            if (ReadOne(vehicle.Value, model) is { Count: > 0 } named)
            {
                Labels[key] = named;
            }
        }
    }

    private static Dictionary<string, string> ReadOne(JsonElement extras, string model)
    {
        var named = new Dictionary<string, string>();

        foreach (var extra in extras.EnumerateObject())
        {
            if (!int.TryParse(extra.Name.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
            {
                Log.Warning($"[Config] Skipping extra '{extra.Name}' on '{model}' in {ConfigFile}: the id has to be a whole number in quotes.");
                continue;
            }

            if (id < 0 || id >= VehicleExtras.Count)
            {
                Log.Warning($"[Config] Skipping extra {id} on '{model}' in {ConfigFile}: only 0 to {VehicleExtras.Count - 1} are ever shown.");
                continue;
            }

            if (extra.Value.ValueKind != JsonValueKind.String)
            {
                Log.Warning($"[Config] Skipping extra {id} on '{model}' in {ConfigFile}: the name to show has to be written in quotes.");
                continue;
            }

            var label = extra.Value.GetString()?.Trim();

            if (string.IsNullOrEmpty(label))
            {
                Log.Warning($"[Config] Skipping extra {id} on '{model}' in {ConfigFile}: it has no name, so the numbered one is kept.");
                continue;
            }

            var slot = id.ToString(CultureInfo.InvariantCulture);

            if (!named.TryAdd(slot, label))
            {
                Log.Warning($"[Config] Extra {id} is listed more than once on '{model}' in {ConfigFile}, so only the first name is used.");
            }
        }

        return named;
    }
}

using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization.Server;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class VehicleExtraActions
{
    private const string ConfigFile = "config/extras.json";

    private static readonly JsonLoadSettings LoadSettings = new()
    {
        CommentHandling = CommentHandling.Ignore,
    };

    private static readonly Dictionary<string, Dictionary<string, string>> Labels = [];

    private static string _payload = "{}";

    public static void Register()
    {
        Load();

        Script.EventHandlers[VehicleEvents.ExtraLabelsRequest] += new Action<Player>(OnRequested);
    }

    private static void OnRequested([FromSource] Player source) =>
        Script.TriggerClientEvent(source.ServerId(), VehicleEvents.ExtraLabelsSet, _payload);

    private static void Load()
    {
        Labels.Clear();
        _payload = "{}";

        if (ResourceFile.Read(ConfigFile, "Config", "vehicle extras keep their numbered names") is not { } contents)
        {
            return;
        }

        List<KeyValuePair<string, List<KeyValuePair<string, JToken>>?>>? vehicles;

        try
        {
            vehicles = Parse(contents);
        }
        catch (JsonException exception)
        {
            Log.Error($"[Config] {ConfigFile} could not be parsed, so vehicle extras keep their numbered names: {exception.Message}");
            return;
        }

        if (vehicles is null)
        {
            Log.Error($"[Config] {ConfigFile} has to hold a single object of vehicle model names, so vehicle extras keep their numbered names.");
            return;
        }

        Read(vehicles);

        _payload = ServerJson.Serialize(Labels);

        Log.Debug($"[Config] Extra labels loaded for {Labels.Count} vehicle model(s) from {ConfigFile}.");
    }

    private static List<KeyValuePair<string, List<KeyValuePair<string, JToken>>?>>? Parse(string contents)
    {
        using var reader = new JsonTextReader(new StringReader(contents))
        {
            DateParseHandling = DateParseHandling.None,
        };

        if (!Next(reader))
        {
            throw new JsonReaderException("The document holds no value.");
        }

        List<KeyValuePair<string, List<KeyValuePair<string, JToken>>?>>? vehicles = null;

        if (reader.TokenType == JsonToken.StartObject)
        {
            vehicles = new List<KeyValuePair<string, List<KeyValuePair<string, JToken>>?>>();

            while (NextMember(reader, out var name))
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    vehicles.Add(new(name, ReadMembers(reader)));
                    continue;
                }

                reader.Skip();
                vehicles.Add(new(name, null));
            }
        }
        else
        {
            reader.Skip();
        }

        if (Next(reader))
        {
            throw new JsonReaderException("Additional text was found after the root value.");
        }

        return vehicles;
    }

    private static List<KeyValuePair<string, JToken>> ReadMembers(JsonReader reader)
    {
        var members = new List<KeyValuePair<string, JToken>>();

        while (NextMember(reader, out var name))
        {
            members.Add(new(name, JToken.ReadFrom(reader, LoadSettings)));
        }

        return members;
    }

    private static bool NextMember(JsonReader reader, out string name)
    {
        name = string.Empty;

        if (!Next(reader))
        {
            throw new JsonReaderException("The document ended inside an object.");
        }

        if (reader.TokenType == JsonToken.EndObject)
        {
            return false;
        }

        name = (string)reader.Value!;

        if (!Next(reader))
        {
            throw new JsonReaderException("The document ended inside an object.");
        }

        return true;
    }

    private static bool Next(JsonReader reader)
    {
        do
        {
            if (!reader.Read())
            {
                return false;
            }
        }
        while (reader.TokenType == JsonToken.Comment);

        return true;
    }

    // Walked, not deserialized: a dictionary keeps the last of two duplicate keys silently.
    private static void Read(List<KeyValuePair<string, List<KeyValuePair<string, JToken>>?>> vehicles)
    {
        foreach (var vehicle in vehicles)
        {
            if (vehicle.Key.Trim() is not { Length: > 0 } model)
            {
                continue;
            }

            if (vehicle.Value is not { } extras)
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

            if (ReadOne(extras, model) is { Count: > 0 } named)
            {
                Labels[key] = named;
            }
        }
    }

    private static Dictionary<string, string> ReadOne(List<KeyValuePair<string, JToken>> extras, string model)
    {
        var named = new Dictionary<string, string>();

        foreach (var extra in extras)
        {
            if (!ActionArgs.TryInt(extra.Key.Trim(), out var id))
            {
                Log.Warning($"[Config] Skipping extra '{extra.Key}' on '{model}' in {ConfigFile}: the id has to be a whole number in quotes.");
                continue;
            }

            if (id < 0 || id >= VehicleExtras.Count)
            {
                Log.Warning($"[Config] Skipping extra {id} on '{model}' in {ConfigFile}: only 0 to {VehicleExtras.Count - 1} are ever shown.");
                continue;
            }

            if (extra.Value.Type != JTokenType.String)
            {
                Log.Warning($"[Config] Skipping extra {id} on '{model}' in {ConfigFile}: the name to show has to be written in quotes.");
                continue;
            }

            var label = ((string?)extra.Value)?.Trim();

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

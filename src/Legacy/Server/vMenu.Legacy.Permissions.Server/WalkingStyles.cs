using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.PedModels;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization.Server;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Permissions.Server;

// A config file rather than a list baked into vMenu, because a server streaming its own clip sets
// can offer those too. Legacy had nine hardcoded styles, each one a branch in an if chain, and two
// of them silently did nothing depending on which freemode ped you happened to be wearing.
public static class WalkingStyles
{
    private const string ConfigFile = "config/walking-styles.json";

    // Tolerant on purpose: server owners hand-edit this file.
    private static readonly List<WalkingStyle> Styles = [];

    // The list as the clients receive it, built once when the file is read.
    private static string _payload = "[]";

    // A missing or unreadable file just means players are left with the walk their ped came with, which
    // is a working state rather than a broken one.
    public static void Load()
    {
        Styles.Clear();
        _payload = "[]";

        var contents = API.LoadResourceFile(API.GetCurrentResourceName(), ConfigFile);

        if (string.IsNullOrWhiteSpace(contents))
        {
            Log.Warning($"[Config] No {ConfigFile} found, so players are offered no walking styles.");
            return;
        }

        JToken document;

        try
        {
            document = ServerJson.Parse(contents);
        }
        catch (JsonException exception)
        {
            Log.Error($"[Config] {ConfigFile} could not be parsed, so players are offered no walking styles: {exception.Message}");
            return;
        }

        if (document is not JObject root)
        {
            Log.Error($"[Config] {ConfigFile} has to hold a single object of clip sets, so players are offered no walking styles.");
            return;
        }

        Read(root);

        _payload = ServerJson.Serialize(Styles);

        Log.Debug($"[Config] {Styles.Count} walking style(s) loaded from {ConfigFile}.");
    }

    // Call once the config has been read.
    public static void RegisterEventHandlers() =>
        Script.EventHandlers[WalkingStyleEvents.Request] += new Action<Player>(OnRequested);

    // A named method, not a lambda: the binder reads FromSourceAttribute off the delegate's MethodInfo.
    private static void OnRequested([FromSource] Player source) =>
        Script.TriggerClientEvent(source.ServerId(), WalkingStyleEvents.Set, _payload);

    private static void Read(JObject root)
    {
        var claimed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in root.Properties())
        {
            if (property.Name.Trim() is not { Length: > 0 } clipset)
            {
                continue;
            }

            if (property.Value.Type != JTokenType.String)
            {
                Log.Warning($"[Config] Skipping walking style '{clipset}': the text to show for it has to be written in quotes.");
                continue;
            }

            if (!claimed.Add(clipset))
            {
                Log.Warning($"[Config] '{clipset}' is listed more than once, so only the first one is offered.");
                continue;
            }

            // An empty label leaves the row with nothing on it, which reads as a bug.
            var label = ((string?)property.Value)?.Trim();

            Styles.Add(new WalkingStyle
            {
                Clipset = clipset,
                Label = string.IsNullOrEmpty(label) ? clipset : label,
            });
        }
    }
}

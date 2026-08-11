using System.Text.Json;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.PedModels;
using vMenu.Enhanced.Serialization.Server;

namespace vMenu.Enhanced.Permissions.Server;

/// <summary>
/// The ways of walking players can pick from, owned here and mirrored to every client.
/// </summary>
/// <remarks>
/// A config file rather than a list baked into vMenu, because a server streaming its own clip sets
/// can offer those too. Legacy had nine hardcoded styles, each one a branch in an if chain, and two
/// of them silently did nothing depending on which freemode ped you happened to be wearing.
/// </remarks>
public static class WalkingStyles
{
    private const string ConfigFile = "config/walking-styles.json";

    // Tolerant on purpose: server owners hand-edit this file.
    private static readonly JsonDocumentOptions ParseOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private static readonly List<WalkingStyle> Styles = [];

    /// <summary>The list as the clients receive it, built once when the file is read.</summary>
    private static string _payload = "[]";

    /// <summary>
    /// Reads the config file. A missing or unreadable one just means players are left with the walk
    /// their ped came with, which is a working state rather than a broken one.
    /// </summary>
    public static void Load()
    {
        Styles.Clear();
        _payload = "[]";

        var contents = Native.LoadResourceFile(Native.GetCurrentResourceName(), ConfigFile);

        if (string.IsNullOrWhiteSpace(contents))
        {
            API.Log.Info($"[Config] No {ConfigFile} found, so players are offered no walking styles.");
            return;
        }

        JsonDocument document;

        try
        {
            document = JsonDocument.Parse(contents, ParseOptions);
        }
        catch (JsonException exception)
        {
            API.Log.Error($"[Config] {ConfigFile} could not be parsed, so players are offered no walking styles: {exception.Message}");
            return;
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                API.Log.Error($"[Config] {ConfigFile} has to hold a single object of clip sets, so players are offered no walking styles.");
                return;
            }

            Read(document.RootElement);
        }

        _payload = ServerJson.Serialize(Styles);

        API.Log.Info($"[Config] {Styles.Count} walking style(s) loaded from {ConfigFile}.");
    }

    /// <summary>Call once the config has been read.</summary>
    public static void RegisterEventHandlers() =>
        API.OnNetEvent(WalkingStyleEvents.Request, new Action<Player>(OnRequested), false);

    /// <summary>
    /// A named method, not a lambda: the binder reads <see cref="FromSourceAttribute"/> off the
    /// delegate's <c>MethodInfo</c>.
    /// </summary>
    private static void OnRequested([FromSource] Player source) =>
        API.EmitClient(source.Handle, WalkingStyleEvents.Set, _payload);

    private static void Read(JsonElement root)
    {
        var claimed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in root.EnumerateObject())
        {
            if (property.Name.Trim() is not { Length: > 0 } clipset)
            {
                continue;
            }

            if (property.Value.ValueKind != JsonValueKind.String)
            {
                API.Log.Warn($"[Config] Skipping walking style '{clipset}': the text to show for it has to be written in quotes.");
                continue;
            }

            if (!claimed.Add(clipset))
            {
                API.Log.Warn($"[Config] '{clipset}' is listed more than once, so only the first one is offered.");
                continue;
            }

            // An empty label leaves the row with nothing on it, which reads as a bug.
            var label = property.Value.GetString()?.Trim();

            Styles.Add(new WalkingStyle
            {
                Clipset = clipset,
                Label = string.IsNullOrEmpty(label) ? clipset : label,
            });
        }
    }
}

using System.Globalization;
using System.Text;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Misc;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization.Server;

using DisplaySettingsPermissions = vMenu.Enhanced.Data.Permissions.Menus.DisplaySettings;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class LocationBlipActions
{
    private const string ConfigFile = "config/blips.json";

    private const string AlwaysOnList = "alwayson";

    private const string ToggleableList = "toggleable";

    private const int MaxSprite = 900;

    private const int MaxColour = 85;

    private const float MinScaleOffset = -0.6f;

    private const float MaxScaleOffset = 1.2f;

    private static readonly BlipFile Blips = new();

    private static string _payload = "{}";

    public static void Register()
    {
        Load();

        ActionRegistry.Register(
            ActionIds.DisplaySettings.AddBlip,
            DisplaySettingsPermissions.ManageBlips,
            AddBlip);

        ActionRegistry.Register(
            ActionIds.DisplaySettings.RemoveBlip,
            DisplaySettingsPermissions.ManageBlips,
            RemoveBlip);

        API.OnNetEvent(LocationBlipEvents.Request, new Action<Player>(OnRequested), false);
    }

    private static void OnRequested([FromSource] Player source) => SendTo(source);

    private static ActionResponse AddBlip(Player source, string[] args)
    {
        if (args.Length < 7 || Trimmed(args[1]) is not { } name)
        {
            return ActionResponse.InvalidRequest();
        }

        if (ListNamed(args[0]) is not { } list)
        {
            return ActionResponse.NotFound();
        }

        if (!TryParseInt(args[2], out var sprite) || !TryParseInt(args[3], out var colour)
            || !TryParse(args[4], out var scale)
            || !TryParse(args[5], out var x) || !TryParse(args[6], out var y)
            || args.Length < 8 || !TryParse(args[7], out var z))
        {
            return ActionResponse.InvalidRequest();
        }

        foreach (var existing in list)
        {
            if (string.Equals(existing.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return ActionResponse.Refused();
            }
        }

        var added = new BlipEntry
        {
            Name = name,
            Sprite = Math.Clamp(sprite, 0, MaxSprite),
            Colour = Math.Clamp(colour, 0, MaxColour),
            ScaleOffset = Math.Clamp(scale, MinScaleOffset, MaxScaleOffset),
            ShortRange = true,
            X = x,
            Y = y,
            Z = z,
        };

        list.Add(added);

        return Commit(source, $"added blip '{name}'", () => list.Remove(added));
    }

    private static ActionResponse RemoveBlip(Player source, string[] args)
    {
        if (args.Length < 2 || Trimmed(args[1]) is not { } name)
        {
            return ActionResponse.InvalidRequest();
        }

        if (ListNamed(args[0]) is not { } list)
        {
            return ActionResponse.NotFound();
        }

        var at = IndexOf(list, name);

        if (at < 0)
        {
            return ActionResponse.NotFound();
        }

        var removed = list[at];

        list.RemoveAt(at);

        return Commit(source, $"removed blip '{removed.Name}'", () => list.Insert(at, removed));
    }

    private static ActionResponse Commit(Player source, string what, Action undo)
    {
        if (!Write(ServerJson.SerializeIndented(Blips)))
        {
            undo();

            return ActionResponse.Failed();
        }

        _payload = ServerJson.Serialize(Blips);

        foreach (var player in API.Players.All)
        {
            SendTo(player);
        }

        Log.Debug($"[Blips] {source} {what}.");

        return ActionResponse.Ok();
    }

    private static bool Write(string json)
    {
        try
        {
            if (Native.SaveResourceFile(Native.GetCurrentResourceName(), ConfigFile, Encoding.UTF8.GetBytes(json)))
            {
                return true;
            }

            Log.Error($"[Blips] {ConfigFile} could not be written, so nothing was changed.");
        }
        catch (Exception exception)
        {
            Log.Error($"[Blips] Writing {ConfigFile} threw, so nothing was changed: {exception}");
        }

        return false;
    }

    // Ungated on purpose: the always-on list is not optional, so everybody needs it.
    private static void SendTo(Player player) =>
        API.EmitClient(player.Handle, LocationBlipEvents.Set, _payload);

    private static List<BlipEntry>? ListNamed(string? which) => Trimmed(which)?.ToLowerInvariant() switch
    {
        AlwaysOnList => Blips.AlwaysOn,
        ToggleableList => Blips.Toggleable,
        _ => null,
    };

    private static int IndexOf(List<BlipEntry> list, string name)
    {
        for (var index = 0; index < list.Count; index++)
        {
            if (string.Equals(list[index].Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return -1;
    }

    private static string? Trimmed(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool TryParse(string value, out float parsed) =>
        float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed);

    private static bool TryParseInt(string value, out int parsed) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed);

    private static void Load()
    {
        var contents = Native.LoadResourceFile(Native.GetCurrentResourceName(), ConfigFile);

        if (string.IsNullOrWhiteSpace(contents))
        {
            Log.Info($"[Blips] No {ConfigFile} found, so no map blips are added.");
            return;
        }

        if (!ServerJson.TryDeserialize<BlipFile>(contents, out var read, out var error))
        {
            Log.Error($"[Blips] {ConfigFile} could not be parsed, so no map blips are added: {error}");
            return;
        }

        if (read is null)
        {
            Log.Error($"[Blips] {ConfigFile} has to hold an object with an alwaysOn and a toggleable list, so no map blips are added.");
            return;
        }

        Blips.AlwaysOn.AddRange(Keep(read.AlwaysOn, AlwaysOnList));
        Blips.Toggleable.AddRange(Keep(read.Toggleable, ToggleableList));

        _payload = ServerJson.Serialize(Blips);

        Log.Debug($"[Blips] {Blips.AlwaysOn.Count} always on and {Blips.Toggleable.Count} toggleable blip(s) loaded from {ConfigFile}.");
    }

    private static List<BlipEntry> Keep(List<BlipEntry>? entries, string list)
    {
        var kept = new List<BlipEntry>();
        var claimed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var blip in entries ?? [])
        {
            if (string.IsNullOrWhiteSpace(blip.Name))
            {
                Log.Warning($"[Blips] Skipping a blip in '{list}': it has no name.");
                continue;
            }

            blip.Name = blip.Name.Trim();

            if (!claimed.Add(blip.Name))
            {
                Log.Warning($"[Blips] '{blip.Name}' is listed more than once in '{list}', so only the first one is used.");
                continue;
            }

            blip.Sprite = Math.Clamp(blip.Sprite, 0, MaxSprite);
            blip.Colour = Math.Clamp(blip.Colour, 0, MaxColour);
            blip.ScaleOffset = Math.Clamp(blip.ScaleOffset, MinScaleOffset, MaxScaleOffset);

            kept.Add(blip);
        }

        return kept;
    }

    private sealed class BlipFile
    {
        public List<BlipEntry> AlwaysOn { get; set; } = [];

        public List<BlipEntry> Toggleable { get; set; } = [];
    }

    private sealed class BlipEntry
    {
        public string? Name { get; set; }

        public int Sprite { get; set; }

        public int Colour { get; set; }

        public float ScaleOffset { get; set; }

        public bool ShortRange { get; set; } = true;

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }
    }
}

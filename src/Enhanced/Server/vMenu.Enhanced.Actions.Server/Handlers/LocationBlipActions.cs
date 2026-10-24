using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Misc;
using vMenu.Enhanced.Logging;

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

    private const float SamePlaceRange = 5f;

    private const float RemoveRange = 2f;

    // Ungated on purpose: the always-on list is not optional, so everybody needs it.
    private static readonly SyncedConfigFile<BlipFile> Store = new(
        ConfigFile,
        "Blips",
        LocationBlipEvents.Set,
        null,
        "nothing was changed");

    private static BlipFile Blips => Store.Value;

    public static string Payload => Store.Payload;

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

    private static void OnRequested([FromSource] Player source) => Store.SendTo(source);

    private static ActionResponse AddBlip(Player source, string[] args)
    {
        if (args.Length < 7 || ActionArgs.Trimmed(args[1]) is not { } name)
        {
            return ActionResponse.InvalidRequest();
        }

        if (ListNamed(args[0]) is not { } list)
        {
            return ActionResponse.NotFound();
        }

        if (!ActionArgs.TryInt(args[2], out var sprite) || !ActionArgs.TryInt(args[3], out var colour)
            || !ActionArgs.TryFloat(args[4], out var scale)
            || !ActionArgs.TryFloat(args[5], out var x) || !ActionArgs.TryFloat(args[6], out var y)
            || args.Length < 8 || !ActionArgs.TryFloat(args[7], out var z))
        {
            return ActionResponse.InvalidRequest();
        }

        foreach (var existing in list)
        {
            if (string.Equals(existing.Name, name, StringComparison.OrdinalIgnoreCase)
                && DistanceSquared(existing, x, y, z) <= SamePlaceRange * SamePlaceRange)
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

        return Store.Commit(source, $"added blip '{name}'", () => list.Remove(added));
    }

    private static ActionResponse RemoveBlip(Player source, string[] args)
    {
        if (args.Length < 5 || ActionArgs.Trimmed(args[1]) is not { } name)
        {
            return ActionResponse.InvalidRequest();
        }

        if (ListNamed(args[0]) is not { } list)
        {
            return ActionResponse.NotFound();
        }

        if (!ActionArgs.TryFloat(args[2], out var x)
            || !ActionArgs.TryFloat(args[3], out var y)
            || !ActionArgs.TryFloat(args[4], out var z))
        {
            return ActionResponse.InvalidRequest();
        }

        var at = IndexOfNearest(list, name, x, y, z);

        if (at < 0)
        {
            return ActionResponse.NotFound();
        }

        var removed = list[at];

        list.RemoveAt(at);

        return Store.Commit(source, $"removed blip '{removed.Name}'", () => list.Insert(at, removed));
    }

    private static List<BlipEntry>? ListNamed(string? which) => ActionArgs.Trimmed(which)?.ToLowerInvariant() switch
    {
        AlwaysOnList => Blips.AlwaysOn,
        ToggleableList => Blips.Toggleable,
        _ => null,
    };

    // By name and place together, since several blips are allowed to carry the same name.
    private static int IndexOfNearest(List<BlipEntry> list, string name, float x, float y, float z)
    {
        var found = -1;
        var nearest = RemoveRange * RemoveRange;

        for (var index = 0; index < list.Count; index++)
        {
            if (!string.Equals(list[index].Name, name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var apart = DistanceSquared(list[index], x, y, z);

            if (apart > nearest)
            {
                continue;
            }

            nearest = apart;
            found = index;
        }

        return found;
    }

    private static float DistanceSquared(BlipEntry blip, float x, float y, float z)
    {
        var dx = blip.X - x;
        var dy = blip.Y - y;
        var dz = blip.Z - z;

        return (dx * dx) + (dy * dy) + (dz * dz);
    }

    private static void Load()
    {
        if (!ResourceFile.TryLoad<BlipFile>(
            ConfigFile,
            "Blips",
            "no map blips are added",
            "an object with an alwaysOn and a toggleable list",
            out var read))
        {
            return;
        }

        Blips.AlwaysOn.AddRange(Keep(read.AlwaysOn, AlwaysOnList));
        Blips.Toggleable.AddRange(Keep(read.Toggleable, ToggleableList));

        Store.Rebuild();

        Log.Debug($"[Blips] {Blips.AlwaysOn.Count} always on and {Blips.Toggleable.Count} toggleable blip(s) loaded from {ConfigFile}.");
    }

    private static List<BlipEntry> Keep(List<BlipEntry>? entries, string list)
    {
        var kept = new List<BlipEntry>();

        foreach (var blip in entries ?? [])
        {
            if (string.IsNullOrWhiteSpace(blip.Name))
            {
                Log.Warning($"[Blips] Skipping a blip in '{list}': it has no name.");
                continue;
            }

            blip.Name = blip.Name.Trim();

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

using Newtonsoft.Json;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Teleport;
using vMenu.Enhanced.Logging;

using TeleportMenuPermissions = vMenu.Enhanced.Data.Permissions.Menus.TeleportMenu;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives.Server;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class TeleportActions
{
    private const string ConfigFile = "config/teleport-categories.json";

    // Gated on the way out as well, so a player who may not use the categories never holds the list of them.
    private static readonly SyncedConfigFile<List<Category>> Store = new(
        ConfigFile,
        "Teleport",
        TeleportEvents.Set,
        TeleportMenuPermissions.Category,
        "nothing was added");

    private static List<Category> Categories => Store.Value;

    public static string Payload => Store.Payload;

    public static void Register()
    {
        Load();

        ActionRegistry.Register(
            ActionIds.TeleportMenu.AddCategory,
            TeleportMenuPermissions.Manage,
            AddCategory);

        ActionRegistry.Register(
            ActionIds.TeleportMenu.AddLocation,
            TeleportMenuPermissions.Manage,
            AddLocation);

        ActionRegistry.Register(
            ActionIds.TeleportMenu.RemoveCategory,
            TeleportMenuPermissions.Manage,
            RemoveCategory);

        ActionRegistry.Register(
            ActionIds.TeleportMenu.RemoveLocation,
            TeleportMenuPermissions.Manage,
            RemoveLocation);

        Script.EventHandlers[TeleportEvents.Request] += new Action<Player>(OnRequested);
    }

    // A named method, not a lambda: the binder reads FromSourceAttribute off the delegate's MethodInfo.
    private static void OnRequested([FromSource] Player source) => Store.SendTo(source);

    private static ActionResponse AddCategory(Player source, string[] args)
    {
        if (args.Length < 2 || ActionArgs.Trimmed(args[0]) is not { } name)
        {
            return ActionResponse.InvalidRequest();
        }

        if (Find(name) is not null)
        {
            return ActionResponse.Refused();
        }

        var added = new Category
        {
            Name = name,
            Description = args[1].Trim(),
            Locations = [],
        };

        Categories.Add(added);

        return Store.Commit(source, $"added category '{name}'", () => Categories.Remove(added));
    }

    private static ActionResponse AddLocation(Player source, string[] args)
    {
        if (args.Length < 6 || ActionArgs.Trimmed(args[1]) is not { } name)
        {
            return ActionResponse.InvalidRequest();
        }

        if (Find(args[0]) is not { } category)
        {
            return ActionResponse.NotFound();
        }

        if (!ActionArgs.TryFloat(args[3], out var x)
            || !ActionArgs.TryFloat(args[4], out var y)
            || !ActionArgs.TryFloat(args[5], out var z))
        {
            return ActionResponse.InvalidRequest();
        }

        category.Locations ??= [];

        if (category.Locations.IndexOfName(name, location => location.Name) >= 0)
        {
            return ActionResponse.Refused();
        }

        var added = new Location
        {
            Name = name,
            Description = args[2].Trim(),
            Position = new Position { X = x, Y = y, Z = z },

            // Optional, so anything unreadable is left unset rather than refused: without one the player simply
            // keeps whichever way they are already facing.
            Heading = args.Length > 6 && ActionArgs.TryFloat(args[6], out var heading) ? heading : null,
        };

        category.Locations.Add(added);

        return Store.Commit(
            source,
            $"added '{name}' to '{category.Name}'",
            () => category.Locations.Remove(added));
    }

    private static ActionResponse RemoveCategory(Player source, string[] args)
    {
        if (args.Length < 1 || ActionArgs.Trimmed(args[0]) is not { } name)
        {
            return ActionResponse.InvalidRequest();
        }

        var at = Categories.IndexOfName(name, category => category.Name);

        if (at < 0)
        {
            return ActionResponse.NotFound();
        }

        var removed = Categories[at];

        Categories.RemoveAt(at);

        // Put back where it was rather than appended, so a failed write leaves the order alone too.
        return Store.Commit(source, $"removed category '{removed.Name}'", () => Categories.Insert(at, removed));
    }

    private static ActionResponse RemoveLocation(Player source, string[] args)
    {
        if (args.Length < 2 || ActionArgs.Trimmed(args[1]) is not { } name)
        {
            return ActionResponse.InvalidRequest();
        }

        if (Find(args[0]) is not { } category || category.Locations is not { } locations)
        {
            return ActionResponse.NotFound();
        }

        var at = locations.IndexOfName(name, location => location.Name);

        if (at < 0)
        {
            return ActionResponse.NotFound();
        }

        var removed = locations[at];

        locations.RemoveAt(at);

        return Store.Commit(
            source,
            $"removed '{removed.Name}' from '{category.Name}'",
            () => locations.Insert(at, removed));
    }

    private static Category? Find(string? name)
    {
        var at = Categories.IndexOfName(name, category => category.Name);

        return at >= 0 ? Categories[at] : null;
    }

    // Read once, dropping anything malformed. A missing or unreadable file just means the category menu
    // starts empty.
    private static void Load()
    {
        if (!ResourceFile.TryLoad<List<Category>>(
            ConfigFile,
            "Teleport",
            "the category menu starts empty",
            "a list of categories",
            out var read))
        {
            return;
        }

        foreach (var category in read)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                Log.Warning($"[Teleport] Skipping a category in {ConfigFile}: it has no name.");
                continue;
            }

            category.Description ??= string.Empty;
            category.Locations = Keep(category.Locations, category.Name);

            Categories.Add(category);

            Log.Debug($"[Teleport] Category '{category.Name}' holds {category.Locations.Count} location(s).");
        }

        Store.Rebuild();
    }

    private static List<Location> Keep(List<Location>? locations, string category)
    {
        var kept = new List<Location>();

        foreach (var location in locations ?? [])
        {
            if (string.IsNullOrWhiteSpace(location.Name))
            {
                Log.Warning($"[Teleport] Skipping a location in '{category}': it has no name.");
                continue;
            }

            if (location.Position is null)
            {
                Log.Warning($"[Teleport] Skipping '{location.Name}' in '{category}': it has no position.");
                continue;
            }

            location.Description ??= string.Empty;

            kept.Add(location);
        }

        return kept;
    }

    private sealed class Category
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public List<Location>? Locations { get; set; }
    }

    private sealed class Location
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public Position? Position { get; set; }

        // Optional. Left out of the file entirely when nobody set one.
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public float? Heading { get; set; }
    }

    private sealed class Position
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }
    }
}

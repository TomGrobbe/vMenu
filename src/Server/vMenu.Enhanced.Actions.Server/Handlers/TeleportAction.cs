using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using Newtonsoft.Json;

using vMenu.Enhanced.BrokenNatives.Server;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Teleport;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Serialization.Server;

using TeleportMenuPermissions = vMenu.Enhanced.Data.Permissions.Menus.TeleportMenu;

namespace vMenu.Enhanced.Actions.Server.Handlers;

/// <summary>
/// The teleport locations, owned here and mirrored to every client that may see them.
/// </summary>
// Clients are told rather than asked: they hold the list from the moment they have their permissions
// and never fetch it again, so opening the menu costs nothing. Anything that changes the list here
// goes back out to everybody, which is what keeps their copies from drifting.
public static class TeleportActions
{
    private const string ConfigFile = "config/teleport-categories.json";

    private static readonly List<Category> Categories = [];

    /// <summary>The list as the clients receive it, rebuilt only when it changes.</summary>
    private static string _payload = "[]";

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

        API.OnNetEvent(TeleportEvents.Request, new Action<Player>(OnRequested), false);
    }

    /// <summary>
    /// A named method, not a lambda: the binder reads <see cref="FromSourceAttribute"/> off the
    /// delegate's <c>MethodInfo</c>.
    /// </summary>
    private static void OnRequested([FromSource] Player source) => SendTo(source);

    private static ActionResponse AddCategory(Player source, string[] args)
    {
        if (args.Length < 2 || Trimmed(args[0]) is not { } name)
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

        return Commit(source, $"added category '{name}'", () => Categories.Remove(added));
    }

    private static ActionResponse AddLocation(Player source, string[] args)
    {
        if (args.Length < 6 || Trimmed(args[1]) is not { } name)
        {
            return ActionResponse.InvalidRequest();
        }

        if (Find(args[0]) is not { } category)
        {
            return ActionResponse.NotFound();
        }

        if (!TryParse(args[3], out var x) || !TryParse(args[4], out var y) || !TryParse(args[5], out var z))
        {
            return ActionResponse.InvalidRequest();
        }

        category.Locations ??= [];

        foreach (var existing in category.Locations)
        {
            if (string.Equals(existing.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return ActionResponse.Refused();
            }
        }

        var added = new Location
        {
            Name = name,
            Description = args[2].Trim(),
            Position = new Position { X = x, Y = y, Z = z },

            // Optional, so anything unreadable is left unset rather than refused: without one the
            // player simply keeps whichever way they are already facing.
            Heading = args.Length > 6 && TryParse(args[6], out var heading) ? heading : null,
        };

        category.Locations.Add(added);

        return Commit(
            source,
            $"added '{name}' to '{category.Name}'",
            () => category.Locations.Remove(added));
    }

    private static ActionResponse RemoveCategory(Player source, string[] args)
    {
        if (args.Length < 1 || Trimmed(args[0]) is not { } name)
        {
            return ActionResponse.InvalidRequest();
        }

        var at = IndexOfCategory(name);

        if (at < 0)
        {
            return ActionResponse.NotFound();
        }

        var removed = Categories[at];

        Categories.RemoveAt(at);

        // Put back where it was rather than appended, so a failed write leaves the order alone too.
        return Commit(source, $"removed category '{removed.Name}'", () => Categories.Insert(at, removed));
    }

    private static ActionResponse RemoveLocation(Player source, string[] args)
    {
        if (args.Length < 2 || Trimmed(args[1]) is not { } name)
        {
            return ActionResponse.InvalidRequest();
        }

        if (Find(args[0]) is not { } category || category.Locations is not { } locations)
        {
            return ActionResponse.NotFound();
        }

        var at = IndexOfLocation(locations, name);

        if (at < 0)
        {
            return ActionResponse.NotFound();
        }

        var removed = locations[at];

        locations.RemoveAt(at);

        return Commit(
            source,
            $"removed '{removed.Name}' from '{category.Name}'",
            () => locations.Insert(at, removed));
    }

    /// <summary>
    /// Writes the file, then republishes the list. Puts the change back if the write failed, so what
    /// the clients hold always matches what is on disk.
    /// </summary>
    private static ActionResponse Commit(Player source, string what, Action undo)
    {
        if (!Write(ServerJson.SerializeIndented(Categories)))
        {
            undo();

            return ActionResponse.Failed();
        }

        _payload = ServerJson.Serialize(Categories);

        Broadcast();

        Log.Debug($"[Teleport] {source} {what}.");

        return ActionResponse.Ok();
    }

    // Through NativeFixer, because the generated SaveResourceFile only takes a byte[] and the sandbox
    // refuses Encoding.UTF8.GetBytes. The native takes a string, so pushing one straight to it works.
    private static bool Write(string json)
    {
        try
        {
            if (NativeFixer.SaveResourceFile(Native.GetCurrentResourceName(), ConfigFile, json))
            {
                return true;
            }

            Log.Error($"[Teleport] {ConfigFile} could not be written, so nothing was added.");
        }
        catch (Exception exception)
        {
            // Answered as a failed write rather than left to the dispatcher, so the caller still
            // undoes what it added. A throw that escaped here would leave the list holding something
            // that is not on disk and that nobody was told about.
            Log.Error($"[Teleport] Writing {ConfigFile} threw, so nothing was added: {exception}");
        }

        return false;
    }

    private static void Broadcast()
    {
        foreach (var player in API.Players.All)
        {
            SendTo(player);
        }
    }

    // Gated on the way out as well as on the way in: a player who may not use the categories has no
    // business holding the list of them.
    private static void SendTo(Player player)
    {
        if (ServerPermissions.IsPlayerAllowed(player, TeleportMenuPermissions.Category))
        {
            API.EmitClient(player.Handle, TeleportEvents.Set, _payload);
        }
    }

    private static Category? Find(string? name)
    {
        var at = IndexOfCategory(name);

        return at >= 0 ? Categories[at] : null;
    }

    private static int IndexOfCategory(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return -1;
        }

        for (var index = 0; index < Categories.Count; index++)
        {
            if (string.Equals(Categories[index].Name, name.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return -1;
    }

    private static int IndexOfLocation(List<Location> locations, string name)
    {
        for (var index = 0; index < locations.Count; index++)
        {
            if (string.Equals(locations[index].Name, name, StringComparison.OrdinalIgnoreCase))
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

    /// <summary>
    /// Reads the file once, drops anything malformed in it and keeps the result. A missing or
    /// unreadable file just means the category menu starts empty.
    /// </summary>
    private static void Load()
    {
        var contents = Native.LoadResourceFile(Native.GetCurrentResourceName(), ConfigFile);

        if (string.IsNullOrWhiteSpace(contents))
        {
            Log.Info($"[Teleport] No {ConfigFile} found, so the category menu starts empty.");
            return;
        }

        if (!ServerJson.TryDeserialize<List<Category>>(contents, out var read, out var error))
        {
            Log.Error($"[Teleport] {ConfigFile} could not be parsed, so the category menu starts empty: {error}");
            return;
        }

        if (read is null)
        {
            Log.Error($"[Teleport] {ConfigFile} has to hold a list of categories, so the category menu starts empty.");
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

            Log.Trace($"[Teleport] Category '{category.Name}' holds {category.Locations.Count} location(s).");
        }

        _payload = ServerJson.Serialize(Categories);
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

        /// <summary>Optional. Left out of the file entirely when nobody set one.</summary>
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

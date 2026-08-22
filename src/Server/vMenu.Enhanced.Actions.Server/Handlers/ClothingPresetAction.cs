using System.Text;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Clothing;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Serialization.Server;

using CharacterCreatorPermissions = vMenu.Enhanced.Data.Permissions.Menus.CharacterCreator;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class ClothingPresetActions
{
    private const string ConfigFile = "config/clothing-presets.json";

    private static readonly List<ClothingPresetCategory> Categories = [];

    private static string _payload = "[]";

    public static void Register()
    {
        Load();

        ActionRegistry.Register(
            ActionIds.CharacterCreator.AddPresetCategory,
            CharacterCreatorPermissions.PresetsManage,
            AddCategory);

        ActionRegistry.Register(
            ActionIds.CharacterCreator.AddPreset,
            CharacterCreatorPermissions.PresetsManage,
            AddPreset);

        ActionRegistry.Register(
            ActionIds.CharacterCreator.RemovePresetCategory,
            CharacterCreatorPermissions.PresetsManage,
            RemoveCategory);

        ActionRegistry.Register(
            ActionIds.CharacterCreator.RemovePreset,
            CharacterCreatorPermissions.PresetsManage,
            RemovePreset);

        API.OnNetEvent(ClothingPresetEvents.Request, new Action<Player>(OnRequested), false);
    }

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

        var added = new ClothingPresetCategory
        {
            Name = name,
            Description = args[1].Trim(),
        };

        Categories.Add(added);

        return Commit(source, $"added preset category '{name}'", () => Categories.Remove(added));
    }

    private static ActionResponse AddPreset(Player source, string[] args)
    {
        if (args.Length < 5 || Trimmed(args[1]) is not { } name)
        {
            return ActionResponse.InvalidRequest();
        }

        if (Find(args[0]) is not { } category)
        {
            return ActionResponse.NotFound();
        }

        if (IndexOfPreset(category.Presets, name) >= 0)
        {
            return ActionResponse.Refused();
        }

        if (!ServerJson.TryDeserialize<ClothingPreset>(args[4], out var read, out var error) || read is null)
        {
            Log.Warning($"[Presets] {source} sent an outfit that could not be read: {error}");

            return ActionResponse.InvalidRequest();
        }

        read.Name = name;
        read.Description = args[2].Trim();
        read.Gender = args[3].Trim();

        category.Presets.Add(read);

        return Commit(
            source,
            $"published '{name}' to '{category.Name}'",
            () => category.Presets.Remove(read));
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

        return Commit(source, $"removed preset category '{removed.Name}'", () => Categories.Insert(at, removed));
    }

    private static ActionResponse RemovePreset(Player source, string[] args)
    {
        if (args.Length < 2 || Trimmed(args[1]) is not { } name)
        {
            return ActionResponse.InvalidRequest();
        }

        if (Find(args[0]) is not { } category)
        {
            return ActionResponse.NotFound();
        }

        var at = IndexOfPreset(category.Presets, name);

        if (at < 0)
        {
            return ActionResponse.NotFound();
        }

        var removed = category.Presets[at];

        category.Presets.RemoveAt(at);

        return Commit(
            source,
            $"removed '{removed.Name}' from '{category.Name}'",
            () => category.Presets.Insert(at, removed));
    }

    private static ActionResponse Commit(Player source, string what, Action undo)
    {
        if (!Write(ServerJson.SerializeIndented(Categories)))
        {
            undo();

            return ActionResponse.Failed();
        }

        _payload = ServerJson.Serialize(Categories);

        Broadcast();

        Log.Debug($"[Presets] {source} {what}.");

        return ActionResponse.Ok();
    }

    private static bool Write(string json)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(json);

            if (Native.SaveResourceFile(Native.GetCurrentResourceName(), ConfigFile, bytes))
            {
                return true;
            }

            Log.Error($"[Presets] {ConfigFile} could not be written, so nothing was added.");
        }
        catch (Exception exception)
        {
            Log.Error($"[Presets] Writing {ConfigFile} threw, so nothing was added: {exception}");
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

    private static void SendTo(Player player)
    {
        if (ServerPermissions.IsPlayerAllowed(player, CharacterCreatorPermissions.Presets))
        {
            API.EmitClient(player.Handle, ClothingPresetEvents.Set, _payload);
        }
    }

    private static ClothingPresetCategory? Find(string? name)
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

    private static int IndexOfPreset(List<ClothingPreset> presets, string name)
    {
        for (var index = 0; index < presets.Count; index++)
        {
            if (string.Equals(presets[index].Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return -1;
    }

    private static string? Trimmed(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void Load()
    {
        var contents = Native.LoadResourceFile(Native.GetCurrentResourceName(), ConfigFile);

        if (string.IsNullOrWhiteSpace(contents))
        {
            Log.Info($"[Presets] No {ConfigFile} found, so the preset menu starts empty.");

            return;
        }

        if (!ServerJson.TryDeserialize<List<ClothingPresetCategory>>(contents, out var read, out var error))
        {
            Log.Error($"[Presets] {ConfigFile} could not be parsed, so the preset menu starts empty: {error}");

            return;
        }

        if (read is null)
        {
            Log.Error($"[Presets] {ConfigFile} has to hold a list of categories, so the preset menu starts empty.");

            return;
        }

        foreach (var category in read)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                Log.Warning($"[Presets] Skipping a category in {ConfigFile}: it has no name.");

                continue;
            }

            category.Presets = Keep(category.Presets, category.Name);

            Categories.Add(category);

            Log.Trace($"[Presets] Category '{category.Name}' holds {category.Presets.Count} outfit(s).");
        }

        _payload = ServerJson.Serialize(Categories);
    }

    private static List<ClothingPreset> Keep(List<ClothingPreset>? presets, string category)
    {
        var kept = new List<ClothingPreset>();

        foreach (var preset in presets ?? [])
        {
            if (string.IsNullOrWhiteSpace(preset.Name))
            {
                Log.Warning($"[Presets] Skipping an outfit in '{category}': it has no name.");

                continue;
            }

            if (preset.Components.Count == 0 && preset.Props.Count == 0)
            {
                Log.Warning($"[Presets] Skipping '{preset.Name}' in '{category}': it has no clothes in it.");

                continue;
            }

            kept.Add(preset);
        }

        return kept;
    }
}

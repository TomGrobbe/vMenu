using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Clothing;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization.Server;

using CharacterCreatorPermissions = vMenu.Enhanced.Data.Permissions.Menus.CharacterCreator;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class ClothingPresetActions
{
    private const string ConfigFile = "config/clothing-presets.json";

    private static readonly SyncedConfigFile<List<ClothingPresetCategory>> Store = new(
        ConfigFile,
        "Presets",
        ClothingPresetEvents.Set,
        CharacterCreatorPermissions.Presets,
        "nothing was added");

    private static List<ClothingPresetCategory> Categories => Store.Value;

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

        var added = new ClothingPresetCategory
        {
            Name = name,
            Description = args[1].Trim(),
        };

        Categories.Add(added);

        return Store.Commit(source, $"added preset category '{name}'", () => Categories.Remove(added));
    }

    private static ActionResponse AddPreset(Player source, string[] args)
    {
        if (args.Length < 5 || ActionArgs.Trimmed(args[1]) is not { } name)
        {
            return ActionResponse.InvalidRequest();
        }

        if (Find(args[0]) is not { } category)
        {
            return ActionResponse.NotFound();
        }

        if (category.Presets.IndexOfName(name, preset => preset.Name) >= 0)
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

        return Store.Commit(
            source,
            $"published '{name}' to '{category.Name}'",
            () => category.Presets.Remove(read));
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

        return Store.Commit(source, $"removed preset category '{removed.Name}'", () => Categories.Insert(at, removed));
    }

    private static ActionResponse RemovePreset(Player source, string[] args)
    {
        if (args.Length < 2 || ActionArgs.Trimmed(args[1]) is not { } name)
        {
            return ActionResponse.InvalidRequest();
        }

        if (Find(args[0]) is not { } category)
        {
            return ActionResponse.NotFound();
        }

        var at = category.Presets.IndexOfName(name, preset => preset.Name);

        if (at < 0)
        {
            return ActionResponse.NotFound();
        }

        var removed = category.Presets[at];

        category.Presets.RemoveAt(at);

        return Store.Commit(
            source,
            $"removed '{removed.Name}' from '{category.Name}'",
            () => category.Presets.Insert(at, removed));
    }

    private static ClothingPresetCategory? Find(string? name)
    {
        var at = Categories.IndexOfName(name, category => category.Name);

        return at >= 0 ? Categories[at] : null;
    }

    private static void Load()
    {
        if (!ResourceFile.TryLoad<List<ClothingPresetCategory>>(
            ConfigFile,
            "Presets",
            "the preset menu starts empty",
            "a list of categories",
            out var read))
        {
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

            Log.Debug($"[Presets] Category '{category.Name}' holds {category.Presets.Count} outfit(s).");
        }

        Store.Rebuild();
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

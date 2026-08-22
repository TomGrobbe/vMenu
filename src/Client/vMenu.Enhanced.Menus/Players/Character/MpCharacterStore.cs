using vMenu.Enhanced.Data.Appearance;
using vMenu.Enhanced.Menus.Saved;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Players.Character;

public static class MpCharacterStore
{
    public const string CharacterPrefix = "vmenu_mpchar_";

    public const string CategoryPrefix = "vmenu_mpcharcategory_";

    #region Characters

    public static List<MpCharacterEntry> All()
    {
        var characters = new List<MpCharacterEntry>();

        foreach (var key in KvpStore.Keys(CharacterPrefix))
        {
            if (Read(key) is { } entry)
            {
                characters.Add(entry);
            }
        }

        characters.Sort(static (left, right) =>
            string.Compare(left.Character.Name, right.Character.Name, StringComparison.OrdinalIgnoreCase));

        return characters;
    }

    public static MpCharacterEntry? Load(string name) => Read(CharacterKey(name));

    public static bool Exists(string name) => Load(name) is not null;

    public static SaveOutcome Save(MpCharacter character, bool replacing)
    {
        var key = CharacterKey(character.Name);

        if (Read(key) is not null && !replacing)
        {
            return SaveOutcome.NameTaken;
        }

        return KvpStore.TryWrite(key, KvpValueType.Json, MpCharacter.SchemaVersion, character)
            ? SaveOutcome.Saved
            : SaveOutcome.Refused;
    }

    public static void Delete(string name) => KvpStore.Delete(CharacterKey(name));

    public static bool Edit(MpCharacterEntry entry, string newName, string description)
    {
        if (entry.IsFromNewerBuild)
        {
            return false;
        }

        var oldName = entry.Character.Name;
        var renaming = !string.Equals(oldName, newName, StringComparison.Ordinal);

        if (renaming && Exists(newName))
        {
            return false;
        }

        var oldDescription = entry.Character.Description;

        entry.Character.Name = newName;
        entry.Character.Description = description;

        if (Save(entry.Character, replacing: !renaming) is not SaveOutcome.Saved)
        {
            entry.Character.Name = oldName;
            entry.Character.Description = oldDescription;

            return false;
        }

        if (renaming)
        {
            Delete(oldName);
        }

        return true;
    }

    public static SaveOutcome Duplicate(MpCharacterEntry entry, string newName) =>
        Save(
            new MpCharacter
            {
                Name = newName,
                Description = entry.Character.Description,
                Category = entry.Character.Category,
                Core = entry.Character.Core,
                Styles = entry.Character.Styles,
                Outfits = entry.Character.Outfits,
                LastStyle = entry.Character.LastStyle,
                LastOutfit = entry.Character.LastOutfit,
                FacialExpression = entry.Character.FacialExpression,
                MovementClipset = entry.Character.MovementClipset,
            },
            replacing: false);

    public static bool MoveToCategory(MpCharacter character, string category)
    {
        character.Category = category;

        return Save(character, replacing: true) is SaveOutcome.Saved;
    }

    #endregion

    #region Outfits and styles

    public static SaveOutcome SaveOutfit(
        MpCharacterEntry entry,
        string name,
        string description,
        PedOutfit outfit,
        bool replacing)
    {
        if (entry.IsFromNewerBuild)
        {
            return SaveOutcome.Refused;
        }

        var existing = entry.Character.OutfitNamed(name);

        if (existing is not null && !replacing)
        {
            return SaveOutcome.NameTaken;
        }

        if (existing is null)
        {
            entry.Character.Outfits.Add(new MpCharacterOutfit
            {
                Name = name,
                Description = description,
                Outfit = outfit,
            });
        }
        else
        {
            existing.Description = description;
            existing.Outfit = outfit;
        }

        return Save(entry.Character, replacing: true);
    }

    public static bool RenameOutfit(
        MpCharacterEntry entry,
        MpCharacterOutfit outfit,
        string newName,
        string description)
    {
        if (entry.IsFromNewerBuild)
        {
            return false;
        }

        if (entry.Character.OutfitNamed(newName) is { } taken && !ReferenceEquals(taken, outfit))
        {
            return false;
        }

        outfit.Name = newName;
        outfit.Description = description;

        return Save(entry.Character, replacing: true) is SaveOutcome.Saved;
    }

    public static bool DeleteOutfit(MpCharacterEntry entry, MpCharacterOutfit outfit)
    {
        if (entry.IsFromNewerBuild)
        {
            return false;
        }

        entry.Character.Outfits.Remove(outfit);

        return Save(entry.Character, replacing: true) is SaveOutcome.Saved;
    }

    public static SaveOutcome SaveStyle(
        MpCharacterEntry entry,
        string name,
        string description,
        MpCharacterStyle style,
        bool replacing)
    {
        if (entry.IsFromNewerBuild)
        {
            return SaveOutcome.Refused;
        }

        var existing = entry.Character.StyleNamed(name);

        if (existing is not null && !replacing)
        {
            return SaveOutcome.NameTaken;
        }

        style.Name = name;
        style.Description = description;

        if (existing is not null)
        {
            entry.Character.Styles.Remove(existing);
        }

        entry.Character.Styles.Add(style);

        return Save(entry.Character, replacing: true);
    }

    public static bool RenameStyle(
        MpCharacterEntry entry,
        MpCharacterStyle style,
        string newName,
        string description)
    {
        if (entry.IsFromNewerBuild)
        {
            return false;
        }

        if (entry.Character.StyleNamed(newName) is { } taken && !ReferenceEquals(taken, style))
        {
            return false;
        }

        style.Name = newName;
        style.Description = description;

        return Save(entry.Character, replacing: true) is SaveOutcome.Saved;
    }

    public static bool DeleteStyle(MpCharacterEntry entry, MpCharacterStyle style)
    {
        if (entry.IsFromNewerBuild)
        {
            return false;
        }

        entry.Character.Styles.Remove(style);

        return Save(entry.Character, replacing: true) is SaveOutcome.Saved;
    }

    #endregion

    #region Categories

    public static List<MpCharacterCategory> Categories()
    {
        var categories = new List<MpCharacterCategory>();

        foreach (var key in KvpStore.Keys(CategoryPrefix))
        {
            if (KvpStore.TryRead<MpCharacterCategory>(
                    key, KvpValueType.Json, MpCharacter.SchemaVersion, out var category, out _)
                && category is not null)
            {
                categories.Add(category);
            }
        }

        categories.Sort(static (left, right) =>
            string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));

        return categories;
    }

    public static bool AddCategory(string name, string description)
    {
        if (HasCategory(name))
        {
            return false;
        }

        return KvpStore.TryWrite(
            CategoryPrefix + name,
            KvpValueType.Json,
            MpCharacter.SchemaVersion,
            new MpCharacterCategory { Name = name, Description = description });
    }

    public static bool HasCategory(string name) =>
        KvpStore.TryRead<MpCharacterCategory>(
            CategoryPrefix + name,
            KvpValueType.Json,
            MpCharacter.SchemaVersion,
            out _,
            out _);

    public static bool EditCategory(string oldName, string newName, string description)
    {
        var renaming = !string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase);

        if (renaming && HasCategory(newName))
        {
            return false;
        }

        KvpStore.Delete(CategoryPrefix + oldName);

        if (!KvpStore.TryWrite(
            CategoryPrefix + newName,
            KvpValueType.Json,
            MpCharacter.SchemaVersion,
            new MpCharacterCategory { Name = newName, Description = description }))
        {
            return false;
        }

        if (!renaming)
        {
            return true;
        }

        foreach (var entry in All())
        {
            if (!string.Equals(entry.Character.Category, oldName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (entry.IsFromNewerBuild)
            {
                continue;
            }

            MoveToCategory(entry.Character, newName);
        }

        return true;
    }

    public static void DeleteCategory(string name)
    {
        KvpStore.Delete(CategoryPrefix + name);

        foreach (var entry in All())
        {
            if (!string.Equals(entry.Character.Category, name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (entry.IsFromNewerBuild)
            {
                continue;
            }

            MoveToCategory(entry.Character, string.Empty);
        }
    }

    #endregion

    public static IEnumerable<string> Describe() => KvpStore.Describe(CharacterPrefix);

    private static MpCharacterEntry? Read(string key)
    {
        if (!KvpStore.TryRead<MpCharacter>(
                key, KvpValueType.Json, MpCharacter.SchemaVersion, out var character, out var version)
            || character is null)
        {
            return null;
        }

        return new MpCharacterEntry(character, version);
    }

    private static string CharacterKey(string name) => CharacterPrefix + name;
}

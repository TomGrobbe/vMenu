using vMenu.Enhanced.Menus.Saved;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Players.Saved;

// The player's own machine, not the server. Keyed on the resource name, so the same collection shows
// up on every server running vMenu Enhanced, which is also why the version check matters: one of
// those servers may be older than the build that wrote a save. Legacy's bare "ped_" keys belonged to
// a different resource name, so those saves are invisible from here and no migration is possible.
public static class SavedPedStore
{
    public const string PedPrefix = "vmenu_ped_";

    // The underscore placement matters both ways round. Listing "vmenu_ped_" never reaches a category,
    // and a ped somebody named "category_x" stores under "vmenu_ped_category_x", which does not start
    // with "vmenu_pedcategory_".
    public const string CategoryPrefix = "vmenu_pedcategory_";

    #region Peds

    public static List<SavedPedEntry> All()
    {
        var peds = new List<SavedPedEntry>();

        // KvpStore.Keys closes its find handle. Legacy's equivalent leaked one every time the saved peds
        // menu was opened.
        foreach (var key in KvpStore.Keys(PedPrefix))
        {
            if (Read(key) is { } entry)
            {
                peds.Add(entry);
            }
        }

        peds.Sort(static (left, right) =>
            string.Compare(left.Ped.Name, right.Ped.Name, StringComparison.OrdinalIgnoreCase));

        return peds;
    }

    public static SavedPedEntry? Load(string name) => Read(PedKey(name));

    public static bool Exists(string name) => Load(name) is not null;

    // replacing is the difference between "replace this one" and "save a new one".
    public static SaveOutcome Save(SavedPed ped, bool replacing)
    {
        var key = PedKey(ped.Name);

        if (Read(key) is not null && !replacing)
        {
            return SaveOutcome.NameTaken;
        }

        return KvpStore.TryWrite(key, KvpValueType.Json, SavedPed.SchemaVersion, ped)
            ? SaveOutcome.Saved
            : SaveOutcome.Refused;
    }

    public static void Delete(string name) => KvpStore.Delete(PedKey(name));

    // A newer build's save is refused rather than moved. Renaming writes a key that holds nothing yet,
    // which no version check can guard, and what would be written is only the fields this build knows
    // about. That is the silent downgrade the whole refusal mechanism exists to prevent.
    public static bool Edit(SavedPedEntry entry, string newName, string description)
    {
        if (entry.IsFromNewerBuild)
        {
            return false;
        }

        var oldName = entry.Ped.Name;
        var renaming = !string.Equals(oldName, newName, StringComparison.Ordinal);

        if (renaming && Exists(newName))
        {
            return false;
        }

        var oldDescription = entry.Ped.Description;

        entry.Ped.Name = newName;
        entry.Ped.Description = description;

        if (Save(entry.Ped, replacing: !renaming) is not SaveOutcome.Saved)
        {
            entry.Ped.Name = oldName;
            entry.Ped.Description = oldDescription;

            return false;
        }

        if (renaming)
        {
            Delete(oldName);
        }

        return true;
    }

    // The copy is written at this build's schema version even when the original came from a newer one.
    // That is honest rather than lossy: it is a new save holding what this build could read.
    public static SaveOutcome Duplicate(SavedPedEntry entry, string newName) =>
        Save(
            new SavedPed
            {
                Name = newName,
                Description = entry.Ped.Description,
                Category = entry.Ped.Category,
                Appearance = entry.Ped.Appearance,
                MovementClipset = entry.Ped.MovementClipset,
            },
            replacing: false);

    public static bool MoveToCategory(SavedPed ped, string category)
    {
        ped.Category = category;

        return Save(ped, replacing: true) is SaveOutcome.Saved;
    }

    #endregion

    #region Categories

    public static List<SavedPedCategory> Categories()
    {
        var categories = new List<SavedPedCategory>();

        foreach (var key in KvpStore.Keys(CategoryPrefix))
        {
            if (KvpStore.TryRead<SavedPedCategory>(key, KvpValueType.Json, SavedPed.SchemaVersion, out var category, out _)
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
            SavedPed.SchemaVersion,
            new SavedPedCategory { Name = name, Description = description });
    }

    public static bool HasCategory(string name) =>
        KvpStore.TryRead<SavedPedCategory>(
            CategoryPrefix + name,
            KvpValueType.Json,
            SavedPed.SchemaVersion,
            out _,
            out _);

    // False when the new name is already a category.
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
            SavedPed.SchemaVersion,
            new SavedPedCategory { Name = newName, Description = description }))
        {
            return false;
        }

        if (!renaming)
        {
            return true;
        }

        foreach (var entry in All())
        {
            if (!string.Equals(entry.Ped.Category, oldName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // A save from a newer build cannot be rewritten, so it keeps naming the old category. The menu
            // already treats an unknown category as a group of its own.
            if (entry.IsFromNewerBuild)
            {
                continue;
            }

            MoveToCategory(entry.Ped, newName);
        }

        return true;
    }

    // The peds in it are kept and become uncategorised, since losing a group is not a reason to lose
    // what was in it.
    public static void DeleteCategory(string name)
    {
        KvpStore.Delete(CategoryPrefix + name);

        foreach (var entry in All())
        {
            if (!string.Equals(entry.Ped.Category, name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (entry.IsFromNewerBuild)
            {
                continue;
            }

            MoveToCategory(entry.Ped, string.Empty);
        }
    }

    #endregion

    public static IEnumerable<string> Describe() => KvpStore.Describe(PedPrefix);

    private static SavedPedEntry? Read(string key)
    {
        if (!KvpStore.TryRead<SavedPed>(key, KvpValueType.Json, SavedPed.SchemaVersion, out var ped, out var version)
            || ped is null)
        {
            return null;
        }

        return new SavedPedEntry(ped, version);
    }

    private static string PedKey(string name) => PedPrefix + name;
}

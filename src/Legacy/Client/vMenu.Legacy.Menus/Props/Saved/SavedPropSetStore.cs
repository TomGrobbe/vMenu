using vMenu.Enhanced.Menus.Saved;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Props.Saved;

public static class SavedPropSetStore
{
    public const string Prefix = "vmenu_propset_";

    public static List<SavedPropSetEntry> All()
    {
        var sets = new List<SavedPropSetEntry>();

        foreach (var key in KvpStore.Keys(Prefix))
        {
            if (Read(key) is { } entry)
            {
                sets.Add(entry);
            }
        }

        sets.Sort(static (left, right) =>
            string.Compare(left.Set.Name, right.Set.Name, StringComparison.OrdinalIgnoreCase));

        return sets;
    }

    public static SavedPropSetEntry? Load(string name) => Read(Key(name));

    public static bool Exists(string name) => Load(name) is not null;

    public static SaveOutcome Save(SavedPropSet set, bool replacing)
    {
        var key = Key(set.Name);

        if (Read(key) is not null && !replacing)
        {
            return SaveOutcome.NameTaken;
        }

        return KvpStore.TryWrite(key, KvpValueType.Json, SavedPropSet.SchemaVersion, set)
            ? SaveOutcome.Saved
            : SaveOutcome.Refused;
    }

    public static void Delete(string name) => KvpStore.Delete(Key(name));

    public static bool Edit(SavedPropSetEntry entry, string newName, string description)
    {
        if (entry.IsFromNewerBuild || string.IsNullOrWhiteSpace(newName))
        {
            return false;
        }

        var oldName = entry.Set.Name;
        var renaming = !string.Equals(oldName, newName, StringComparison.Ordinal);

        if (renaming && Exists(newName))
        {
            return false;
        }

        entry.Set.Name = newName;
        entry.Set.Description = description;

        if (Save(entry.Set, replacing: true) != SaveOutcome.Saved)
        {
            entry.Set.Name = oldName;

            return false;
        }

        if (renaming)
        {
            KvpStore.Delete(Key(oldName));
        }

        return true;
    }

    public static bool AddProp(SavedPropSetEntry entry, SavedProp prop)
    {
        if (entry.IsFromNewerBuild)
        {
            return false;
        }

        entry.Set.Props.Add(prop);

        if (Save(entry.Set, replacing: true) == SaveOutcome.Saved)
        {
            return true;
        }

        entry.Set.Props.Remove(prop);

        return false;
    }

    public static bool RemoveProp(SavedPropSetEntry entry, int index)
    {
        if (entry.IsFromNewerBuild || (uint)index >= (uint)entry.Set.Props.Count)
        {
            return false;
        }

        var removed = entry.Set.Props[index];

        entry.Set.Props.RemoveAt(index);

        if (Save(entry.Set, replacing: true) == SaveOutcome.Saved)
        {
            return true;
        }

        entry.Set.Props.Insert(index, removed);

        return false;
    }

    private static SavedPropSetEntry? Read(string key) =>
        KvpStore.TryRead<SavedPropSet>(key, KvpValueType.Json, SavedPropSet.SchemaVersion, out var set, out var version)
        && set is not null
            ? new SavedPropSetEntry(set, version)
            : null;

    private static string Key(string name) => Prefix + name;
}

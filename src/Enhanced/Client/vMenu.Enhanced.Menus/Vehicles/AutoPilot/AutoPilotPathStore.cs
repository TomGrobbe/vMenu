using vMenu.Enhanced.Menus.Saved;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Vehicles.AutoPilot;

public static class AutoPilotPathStore
{
    public const string Prefix = "vmenu_autopilotpath_";

    public static List<SavedAutoPilotPathEntry> All()
    {
        var paths = new List<SavedAutoPilotPathEntry>();

        foreach (var key in KvpStore.Keys(Prefix))
        {
            if (Read(key) is { } entry)
            {
                paths.Add(entry);
            }
        }

        paths.Sort(static (left, right) =>
            string.Compare(left.Path.Name, right.Path.Name, StringComparison.OrdinalIgnoreCase));

        return paths;
    }

    public static bool Exists(string name) => Read(Key(name)) is not null;

    public static SaveOutcome Save(SavedAutoPilotPath path, bool replacing)
    {
        var key = Key(path.Name);

        if (Read(key) is not null && !replacing)
        {
            return SaveOutcome.NameTaken;
        }

        return KvpStore.TryWrite(key, KvpValueType.Json, SavedAutoPilotPath.SchemaVersion, path)
            ? SaveOutcome.Saved
            : SaveOutcome.Refused;
    }

    public static void Delete(string name) => KvpStore.Delete(Key(name));

    public static bool Edit(SavedAutoPilotPathEntry entry, string newName, string description)
    {
        if (entry.IsFromNewerBuild || string.IsNullOrWhiteSpace(newName))
        {
            return false;
        }

        var oldName = entry.Path.Name;
        var renaming = !string.Equals(oldName, newName, StringComparison.Ordinal);

        if (renaming && Exists(newName))
        {
            return false;
        }

        var oldDescription = entry.Path.Description;

        entry.Path.Name = newName;
        entry.Path.Description = description;

        if (Save(entry.Path, replacing: true) != SaveOutcome.Saved)
        {
            entry.Path.Name = oldName;
            entry.Path.Description = oldDescription;

            return false;
        }

        if (renaming)
        {
            KvpStore.Delete(Key(oldName));
        }

        return true;
    }

    public static bool RemovePoint(SavedAutoPilotPathEntry entry, int index)
    {
        if (entry.IsFromNewerBuild || (uint)index >= (uint)entry.Path.Points.Count)
        {
            return false;
        }

        var removed = entry.Path.Points[index];

        entry.Path.Points.RemoveAt(index);

        if (Save(entry.Path, replacing: true) == SaveOutcome.Saved)
        {
            return true;
        }

        entry.Path.Points.Insert(index, removed);

        return false;
    }

    private static SavedAutoPilotPathEntry? Read(string key) =>
        KvpStore.TryRead<SavedAutoPilotPath>(key, KvpValueType.Json, SavedAutoPilotPath.SchemaVersion, out var path, out var version)
        && path is not null
            ? new SavedAutoPilotPathEntry(path, version)
            : null;

    private static string Key(string name) => Prefix + name;
}

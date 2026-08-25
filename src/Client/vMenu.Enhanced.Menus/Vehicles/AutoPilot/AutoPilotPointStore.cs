using vMenu.Enhanced.Menus.Saved;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Vehicles.AutoPilot;

public static class AutoPilotPointStore
{
    public const string Prefix = "vmenu_autopilotpoint_";

    public static List<SavedAutoPilotPointEntry> All()
    {
        var points = new List<SavedAutoPilotPointEntry>();

        foreach (var key in KvpStore.Keys(Prefix))
        {
            if (Read(key) is { } entry)
            {
                points.Add(entry);
            }
        }

        points.Sort(static (left, right) =>
            string.Compare(left.Point.Name, right.Point.Name, StringComparison.OrdinalIgnoreCase));

        return points;
    }

    public static bool Exists(string name) => Read(Key(name)) is not null;

    public static SaveOutcome Save(SavedAutoPilotPoint point, bool replacing)
    {
        var key = Key(point.Name);

        if (Read(key) is not null && !replacing)
        {
            return SaveOutcome.NameTaken;
        }

        return KvpStore.TryWrite(key, KvpValueType.Json, SavedAutoPilotPoint.SchemaVersion, point)
            ? SaveOutcome.Saved
            : SaveOutcome.Refused;
    }

    public static void Delete(string name) => KvpStore.Delete(Key(name));

    public static bool Edit(SavedAutoPilotPointEntry entry, string newName, string description)
    {
        if (entry.IsFromNewerBuild || string.IsNullOrWhiteSpace(newName))
        {
            return false;
        }

        var oldName = entry.Point.Name;
        var renaming = !string.Equals(oldName, newName, StringComparison.Ordinal);

        if (renaming && Exists(newName))
        {
            return false;
        }

        var oldDescription = entry.Point.Description;

        entry.Point.Name = newName;
        entry.Point.Description = description;

        if (Save(entry.Point, replacing: true) != SaveOutcome.Saved)
        {
            entry.Point.Name = oldName;
            entry.Point.Description = oldDescription;

            return false;
        }

        if (renaming)
        {
            KvpStore.Delete(Key(oldName));
        }

        return true;
    }

    private static SavedAutoPilotPointEntry? Read(string key) =>
        KvpStore.TryRead<SavedAutoPilotPoint>(key, KvpValueType.Json, SavedAutoPilotPoint.SchemaVersion, out var point, out var version)
        && point is not null
            ? new SavedAutoPilotPointEntry(point, version)
            : null;

    private static string Key(string name) => Prefix + name;
}

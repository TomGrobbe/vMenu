using vMenu.Enhanced.Menus.Saved;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Vehicles.AutoPilot;

public static class DrivingProfileStore
{
    public const string Prefix = "vmenu_autopilotprofile_";

    public static List<SavedDrivingProfileEntry> All()
    {
        var profiles = new List<SavedDrivingProfileEntry>();

        foreach (var key in KvpStore.Keys(Prefix))
        {
            if (Read(key) is { } entry)
            {
                profiles.Add(entry);
            }
        }

        profiles.Sort(static (left, right) =>
            string.Compare(left.Profile.Name, right.Profile.Name, StringComparison.OrdinalIgnoreCase));

        return profiles;
    }

    public static List<SavedDrivingProfileEntry> InCategory(AutoPilotCategory category)
    {
        var matching = new List<SavedDrivingProfileEntry>();

        foreach (var entry in All())
        {
            if (entry.Profile.Category == category)
            {
                matching.Add(entry);
            }
        }

        return matching;
    }

    public static SavedDrivingProfileEntry? Load(string name) => Read(Key(name));

    public static bool Exists(string name) => Load(name) is not null;

    public static bool IsReserved(AutoPilotCategory category, string name) =>
        AutoPilotPresets.IsPreset(category, name);

    public static SaveOutcome Save(SavedDrivingProfile profile, bool replacing)
    {
        var key = Key(profile.Name);

        if (IsReserved(profile.Category, profile.Name))
        {
            return SaveOutcome.NameTaken;
        }

        if (Read(key) is not null && !replacing)
        {
            return SaveOutcome.NameTaken;
        }

        return KvpStore.TryWrite(key, KvpValueType.Json, SavedDrivingProfile.SchemaVersion, profile)
            ? SaveOutcome.Saved
            : SaveOutcome.Refused;
    }

    public static void Delete(string name) => KvpStore.Delete(Key(name));

    public static bool Edit(SavedDrivingProfileEntry entry, string newName, string description)
    {
        if (entry.IsFromNewerBuild || string.IsNullOrWhiteSpace(newName))
        {
            return false;
        }

        var oldName = entry.Profile.Name;
        var renaming = !string.Equals(oldName, newName, StringComparison.Ordinal);

        if (renaming && (Exists(newName) || IsReserved(entry.Profile.Category, newName)))
        {
            return false;
        }

        var oldDescription = entry.Profile.Description;

        entry.Profile.Name = newName;
        entry.Profile.Description = description;

        if (Save(entry.Profile, replacing: true) != SaveOutcome.Saved)
        {
            entry.Profile.Name = oldName;
            entry.Profile.Description = oldDescription;

            return false;
        }

        if (renaming)
        {
            KvpStore.Delete(Key(oldName));

            AutoPilotDefaults.Rename(entry.Profile.Category, oldName, newName);
        }

        return true;
    }

    public static SaveOutcome Duplicate(SavedDrivingProfileEntry entry, string newName)
    {
        var source = entry.Profile;

        return Save(
            new SavedDrivingProfile
            {
                Name = newName,
                Description = source.Description,
                Category = source.Category,
                Flags = source.Flags,
                CruiseSpeed = source.CruiseSpeed,
                FlightHeight = source.FlightHeight,
                MinHeightAboveTerrain = source.MinHeightAboveTerrain,
                Precise = source.Precise,
            },
            replacing: false);
    }

    public static bool Write(SavedDrivingProfileEntry entry, Action<SavedDrivingProfile> change, Action<SavedDrivingProfile> undo)
    {
        if (entry.IsFromNewerBuild)
        {
            return false;
        }

        change(entry.Profile);

        if (Save(entry.Profile, replacing: true) == SaveOutcome.Saved)
        {
            return true;
        }

        undo(entry.Profile);

        return false;
    }

    private static SavedDrivingProfileEntry? Read(string key) =>
        KvpStore.TryRead<SavedDrivingProfile>(key, KvpValueType.Json, SavedDrivingProfile.SchemaVersion, out var profile, out var version)
        && profile is not null
            ? new SavedDrivingProfileEntry(profile, version)
            : null;

    private static string Key(string name) => Prefix + name;
}

using vMenu.Enhanced.Menus.Saved;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Vehicles.Saved;

// The player's own machine, not the server. Keyed on the resource name, so the same collection shows
// up on every server running vMenu Enhanced, which is also why the version check matters: one of
// those servers may be older than the build that wrote a save.
public static class SavedVehicleStore
{
    public const string VehiclePrefix = "vmenu_vehicle_";

    // Deliberately not a suffix of the vehicle prefix, so listing one never finds the other.
    public const string CategoryPrefix = "vmenu_vehcategory_";

    #region Vehicles

    public static List<SavedVehicleEntry> All()
    {
        var vehicles = new List<SavedVehicleEntry>();

        foreach (var key in KvpStore.Keys(VehiclePrefix))
        {
            if (Read(key) is { } entry)
            {
                vehicles.Add(entry);
            }
        }

        vehicles.Sort(static (left, right) =>
            string.Compare(left.Vehicle.Name, right.Vehicle.Name, StringComparison.OrdinalIgnoreCase));

        return vehicles;
    }

    public static SavedVehicleEntry? Load(string name) => Read(VehicleKey(name));

    public static bool Exists(string name) => Load(name) is not null;

    // replacing is the difference between "replace this one" and "save a new one".
    public static SaveOutcome Save(SavedVehicle vehicle, bool replacing)
    {
        var key = VehicleKey(vehicle.Name);

        if (Read(key) is not null && !replacing)
        {
            return SaveOutcome.NameTaken;
        }

        return KvpStore.TryWrite(key, KvpValueType.Json, SavedVehicle.SchemaVersion, vehicle)
            ? SaveOutcome.Saved
            : SaveOutcome.Refused;
    }

    public static void Delete(string name) => KvpStore.Delete(VehicleKey(name));

    // A newer build's save is refused rather than moved. Renaming writes a key that holds nothing yet,
    // which no version check can guard, and what would be written is only the fields this build knows
    // about. That is the silent downgrade the whole refusal mechanism exists to prevent.
    public static bool Edit(SavedVehicleEntry entry, string newName, string description)
    {
        if (entry.IsFromNewerBuild)
        {
            return false;
        }

        var oldName = entry.Vehicle.Name;
        var renaming = !string.Equals(oldName, newName, StringComparison.Ordinal);

        if (renaming && Exists(newName))
        {
            return false;
        }

        var oldDescription = entry.Vehicle.Description;

        entry.Vehicle.Name = newName;
        entry.Vehicle.Description = description;

        if (Save(entry.Vehicle, replacing: !renaming) is not SaveOutcome.Saved)
        {
            entry.Vehicle.Name = oldName;
            entry.Vehicle.Description = oldDescription;

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
    public static SaveOutcome Duplicate(SavedVehicleEntry entry, string newName) =>
        Save(
            new SavedVehicle
            {
                Name = newName,
                Description = entry.Vehicle.Description,
                Category = entry.Vehicle.Category,
                Appearance = entry.Vehicle.Appearance,
            },
            replacing: false);

    public static bool MoveToCategory(SavedVehicle vehicle, string category)
    {
        vehicle.Category = category;

        return Save(vehicle, replacing: true) is SaveOutcome.Saved;
    }

    #endregion

    #region Categories

    public static List<SavedVehicleCategory> Categories()
    {
        var categories = new List<SavedVehicleCategory>();

        foreach (var key in KvpStore.Keys(CategoryPrefix))
        {
            if (KvpStore.TryRead<SavedVehicleCategory>(key, KvpValueType.Json, SavedVehicle.SchemaVersion, out var category, out _)
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
            SavedVehicle.SchemaVersion,
            new SavedVehicleCategory { Name = name, Description = description });
    }

    public static bool HasCategory(string name) =>
        KvpStore.TryRead<SavedVehicleCategory>(
            CategoryPrefix + name,
            KvpValueType.Json,
            SavedVehicle.SchemaVersion,
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
            SavedVehicle.SchemaVersion,
            new SavedVehicleCategory { Name = newName, Description = description }))
        {
            return false;
        }

        if (!renaming)
        {
            return true;
        }

        foreach (var entry in All())
        {
            if (!string.Equals(entry.Vehicle.Category, oldName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // A save from a newer build cannot be rewritten, so it keeps naming the old category. The menu
            // already treats an unknown category as a group of its own.
            if (entry.IsFromNewerBuild)
            {
                continue;
            }

            MoveToCategory(entry.Vehicle, newName);
        }

        return true;
    }

    // The vehicles in it are kept and become uncategorised, since losing a group is not a reason to lose
    // what was in it.
    public static void DeleteCategory(string name)
    {
        KvpStore.Delete(CategoryPrefix + name);

        foreach (var entry in All())
        {
            if (!string.Equals(entry.Vehicle.Category, name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // A save from a newer build cannot be rewritten, so it keeps naming a category that is gone. The
            // menu already treats an unknown category as uncategorised.
            if (entry.IsFromNewerBuild)
            {
                continue;
            }

            MoveToCategory(entry.Vehicle, string.Empty);
        }
    }

    #endregion

    public static IEnumerable<string> Describe() => KvpStore.Describe(VehiclePrefix);

    private static SavedVehicleEntry? Read(string key)
    {
        if (!KvpStore.TryRead<SavedVehicle>(key, KvpValueType.Json, SavedVehicle.SchemaVersion, out var vehicle, out var version)
            || vehicle is null)
        {
            return null;
        }

        return new SavedVehicleEntry(vehicle, version);
    }

    private static string VehicleKey(string name) => VehiclePrefix + name;
}

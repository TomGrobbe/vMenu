using vMenu.Enhanced.Menus.Vehicles.Appearance;

namespace vMenu.Enhanced.Menus.Vehicles.Saved;

// Anything added to or taken out of this, or out of VehicleAppearance underneath it, changes the
// stored format. Raise SchemaVersion when that happens: it is what stops a server running an older
// build of vMenu from overwriting a save it cannot fully read.
//
// A class rather than a record: generated equality reaches for EqualityComparer<T>.Default, which
// the client sandbox refuses to load.
public sealed class SavedVehicle
{
    // What this build understands the shape below to be. Version 2 added Description; a version 1 save
    // still reads fine, it simply comes back without one.
    public const int SchemaVersion = 2;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // Empty means uncategorised, which is a real place rather than a missing one.
    public string Category { get; set; } = string.Empty;

    public VehicleAppearance Appearance { get; set; } = new();
}

// Kept apart from the payload so nothing here can be mistaken for something that was written to
// disk, and so the model needs no serializer attributes to hide it.
public sealed class SavedVehicleEntry(SavedVehicle vehicle, int storedVersion)
{
    public SavedVehicle Vehicle { get; } = vehicle;

    public int StoredVersion { get; } = storedVersion;

    // Written by a newer vMenu than this one. It can still be spawned, duplicated and deleted, but not
    // edited or overwritten, since either would drop whatever that version added.
    public bool IsFromNewerBuild => StoredVersion > SavedVehicle.SchemaVersion;
}

public sealed class SavedVehicleCategory
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

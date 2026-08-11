using vMenu.Enhanced.Menus.Vehicles.Appearance;

namespace vMenu.Enhanced.Menus.Vehicles.Saved;

/// <summary>
/// A vehicle a player put away, stored on their own machine.
/// </summary>
/// <remarks>
/// Anything added to or taken out of this, or out of <see cref="VehicleAppearance"/> underneath it,
/// changes the stored format. Raise <see cref="SchemaVersion"/> when that happens: it is what stops
/// a server running an older build of vMenu from overwriting a save it cannot fully read.
/// </remarks>
// A plain class rather than a record, because the generated equality reaches for
// EqualityComparer<T>.Default and the client sandbox refuses to load it.
public sealed class SavedVehicle
{
    /// <summary>What this build understands the shape below to be.</summary>
    // Version 2 added Description. A version 1 save still reads fine, it simply comes back without
    // one, which is the default value tolerance this codebase uses in place of migration code.
    public const int SchemaVersion = 2;

    public string Name { get; set; } = string.Empty;

    /// <summary>Whatever the player wants to remember about this one. Empty is normal.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Empty means uncategorised, which is a real place rather than a missing one.</summary>
    public string Category { get; set; } = string.Empty;

    public VehicleAppearance Appearance { get; set; } = new();
}

/// <summary>A saved vehicle, together with what the store had to say about it.</summary>
// Kept apart from the payload so nothing here can be mistaken for something that was written to
// disk, and so the model needs no serializer attributes to hide it.
public sealed class SavedVehicleEntry(SavedVehicle vehicle, int storedVersion)
{
    public SavedVehicle Vehicle { get; } = vehicle;

    public int StoredVersion { get; } = storedVersion;

    /// <summary>
    /// Written by a newer vMenu than this one. It can still be spawned, duplicated and deleted, but
    /// not edited or overwritten, since either would drop whatever that version added.
    /// </summary>
    public bool IsFromNewerBuild => StoredVersion > SavedVehicle.SchemaVersion;
}

/// <summary>A group saved vehicles are sorted into.</summary>
public sealed class SavedVehicleCategory
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

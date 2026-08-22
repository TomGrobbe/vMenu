using vMenu.Enhanced.Menus.Players.Appearance;

namespace vMenu.Enhanced.Menus.Players.Saved;

/// <summary>
/// A ped a player put away, stored on their own machine.
/// </summary>
/// <remarks>
/// Anything added to or taken out of this, or out of <see cref="PedAppearance"/> underneath it,
/// changes the stored format. Raise <see cref="SchemaVersion"/> when that happens: it is what stops
/// a server running an older build of vMenu from overwriting a save it cannot fully read.
/// </remarks>
// A plain class rather than a record, because the generated equality reaches for
// EqualityComparer<T>.Default and the client sandbox refuses to load it.
public sealed class SavedPed
{
    /// <summary>What this build understands the shape below to be.</summary>
    public const int SchemaVersion = 2;

    public string Name { get; set; } = string.Empty;

    /// <summary>Whatever the player wants to remember about this one. Empty is normal.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Empty means uncategorised, which is a real place rather than a missing one.</summary>
    public string Category { get; set; } = string.Empty;

    public PedAppearance Appearance { get; set; } = new();

    /// <summary>How the player was walking when they saved it. Empty is the ped's own walk.</summary>
    // Here rather than on the appearance, because the appearance is defined as what can be read back
    // off the ped, and the game offers no way to ask which clip set a ped is using.
    public string MovementClipset { get; set; } = string.Empty;
}

/// <summary>A saved ped, together with what the store had to say about it.</summary>
// Kept apart from the payload so nothing here can be mistaken for something that was written to
// disk, and so the model needs no serializer attributes to hide it.
public sealed class SavedPedEntry(SavedPed ped, int storedVersion)
{
    public SavedPed Ped { get; } = ped;

    public int StoredVersion { get; } = storedVersion;

    /// <summary>
    /// Written by a newer vMenu than this one. It can still be spawned, duplicated and deleted, but
    /// not edited or overwritten, since either would drop whatever that version added.
    /// </summary>
    public bool IsFromNewerBuild => StoredVersion > SavedPed.SchemaVersion;
}

/// <summary>A group saved peds are sorted into.</summary>
public sealed class SavedPedCategory
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

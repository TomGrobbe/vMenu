using vMenu.Enhanced.Menus.Players.Appearance;

namespace vMenu.Enhanced.Menus.Players.Saved;

// Anything added to or taken out of this, or out of PedAppearance underneath it, changes the stored
// format. Raise SchemaVersion when that happens: it is what stops a server running an older build of
// vMenu from overwriting a save it cannot fully read.
//
// A class rather than a record: generated equality reaches for EqualityComparer<T>.Default, which
// the client sandbox refuses to load.
public sealed class SavedPed
{
    // What this build understands the shape below to be.
    public const int SchemaVersion = 2;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // Empty means uncategorised, which is a real place rather than a missing one.
    public string Category { get; set; } = string.Empty;

    public PedAppearance Appearance { get; set; } = new();

    // Here rather than on the appearance, because the appearance is defined as what can be read back off
    // the ped, and the game offers no way to ask which clip set a ped is using.
    public string MovementClipset { get; set; } = string.Empty;
}

// Kept apart from the payload so nothing here can be mistaken for something that was written to
// disk, and so the model needs no serializer attributes to hide it.
public sealed class SavedPedEntry(SavedPed ped, int storedVersion)
{
    public SavedPed Ped { get; } = ped;

    public int StoredVersion { get; } = storedVersion;

    // Written by a newer vMenu than this one. It can still be spawned, duplicated and deleted, but not
    // edited or overwritten, since either would drop whatever that version added.
    public bool IsFromNewerBuild => StoredVersion > SavedPed.SchemaVersion;
}

public sealed class SavedPedCategory
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

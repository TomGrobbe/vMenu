namespace vMenu.Enhanced.Menus.Saved;

/// <summary>What happened when a save was attempted.</summary>
// Shared between the saved vehicles and saved peds stores rather than declared twice, so a caller
// that handles one handles both and the two menus cannot drift apart on what an outcome means.
public enum SaveOutcome
{
    Saved,

    /// <summary>A save under that name already exists and the caller did not mean to replace it.</summary>
    NameTaken,

    /// <summary>
    /// The stored save was written by a newer vMenu, so overwriting it would throw away whatever
    /// that version added. Nothing was changed.
    /// </summary>
    Refused,
}

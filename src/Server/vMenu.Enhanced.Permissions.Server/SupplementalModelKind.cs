namespace vMenu.Enhanced.Permissions.Server;

/// <summary>
/// Kinds of model a server owner can whitelist. Only <see cref="Vehicle"/> is wired to permissions
/// today; the others are read and kept so the config format does not change later.
/// </summary>
public enum SupplementalModelKind
{
    Vehicle,

    Ped,

    Weapon,
}

namespace vMenu.Enhanced.PluginContracts;

/// <summary>The values an <see cref="OpenMenuResult"/> reason can carry.</summary>
public static class OpenMenuReasons
{
    public const string Opened = "opened";
    public const string BlockedByPriority = "blockedByPriority";
    public const string BlockedByLock = "blockedByLock";
    public const string UnknownMenu = "unknownMenu";
    public const string NotAllowed = "notAllowed";
    public const string Superseded = "superseded";
}

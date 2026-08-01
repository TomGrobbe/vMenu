namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// Framework-wide defaults, overridable per menu and then per entry.
/// </summary>
public static class MenuFrameworkOptions
{
    /// <summary>
    /// <see cref="GateBehaviour.Lock"/>, because a player who can see why something is unavailable
    /// asks a better question than one looking at a menu that seems broken.
    /// </summary>
    public static GateBehaviour DefaultGateBehaviour { get; set; } = GateBehaviour.Lock;
}

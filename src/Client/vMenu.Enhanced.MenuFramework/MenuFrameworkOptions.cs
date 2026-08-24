namespace vMenu.Enhanced.MenuFramework;

// Framework-wide defaults, overridable per menu and then per entry.
public static class MenuFrameworkOptions
{
    // Lock, because a player who can see why something is unavailable asks a better question than one
    // looking at a menu that seems broken.
    public static GateBehaviour DefaultGateBehaviour { get; set; } = GateBehaviour.Lock;
}

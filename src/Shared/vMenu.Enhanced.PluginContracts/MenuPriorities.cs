namespace vMenu.Enhanced.PluginContracts;

/// <summary>How hard an open request may push. A request replaces an open menu of equal or lower priority and is
/// refused by a higher one. vMenu's own menus always sit at <see cref="None"/>.</summary>
public static class MenuPriorities
{
    public const string None = "none";
    public const string Low = "low";
    public const string Medium = "medium";
    public const string High = "high";
}

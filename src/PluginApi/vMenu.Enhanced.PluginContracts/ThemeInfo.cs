namespace vMenu.Enhanced.PluginContracts;

/// <summary>One look a player can put vMenu's menus in. The id is what a plugin sends back to pick
/// it, the name is what a player is meant to read.</summary>
public class ThemeInfo
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}

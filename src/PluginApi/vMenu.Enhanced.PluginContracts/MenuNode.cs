namespace vMenu.Enhanced.PluginContracts;

/// <summary>
/// One menu in a plugin's tree, either the root or a submenu's target. Ids follow the
/// same rules as item ids: plugin chosen, unique within the plugin.
/// </summary>
public class MenuNode
{
    public string Id { get; set; } = string.Empty;

    public TextRef? Title { get; set; }

    public TextRef? Subtitle { get; set; }

    /// <summary>Opt in <see cref="NodeEvents"/> subscriptions for this menu.</summary>
    public List<string>? Events { get; set; }

    public List<ItemNode> Items { get; set; } = new();
}

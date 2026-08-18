namespace vMenu.Enhanced.PluginContracts;

/// <summary>
/// Visibility and lock condition for an item. Permission and setting hold SHORT names,
/// vMenu composes the full ACE and convar names from the plugin's identity, so a plugin
/// can never gate on another plugin's names. Exactly one of the four fields should be
/// set. All requires every child to pass, Any requires at least one.
/// </summary>
public class GateNode
{
    public string? Permission { get; set; }

    /// <summary>Short name of one of the plugin's own bool settings.</summary>
    public string? Setting { get; set; }

    public List<GateNode>? All { get; set; }

    public List<GateNode>? Any { get; set; }
}

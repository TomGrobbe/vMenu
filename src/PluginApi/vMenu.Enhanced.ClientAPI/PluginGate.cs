using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.ClientAPI;

/// <summary>
/// Decides whether a menu item is available to the player, evaluated live by vMenu. Combine
/// gates with <c>&amp;</c> and <c>|</c>. Names are short: vMenu scopes them to your plugin, so
/// you can never gate on another plugin's permissions or settings.
/// </summary>
public sealed class PluginGate
{
    private readonly GateNode _node;

    private PluginGate(GateNode node) => _node = node;

    /// <summary>One of the permissions your server side declared, by its short name.</summary>
    public static PluginGate Permission(string shortName) => new(new GateNode { Permission = shortName });

    /// <summary>One of your bool settings: the item is available while the convar reads true.</summary>
    public static PluginGate Setting(PluginBoolSetting setting) => new(new GateNode { Setting = setting.Name });

    public static implicit operator PluginGate(string permissionShortName) => Permission(permissionShortName);

    public static PluginGate operator &(PluginGate left, PluginGate right) =>
        new(new GateNode { All = new List<GateNode> { left._node, right._node } });

    public static PluginGate operator |(PluginGate left, PluginGate right) =>
        new(new GateNode { Any = new List<GateNode> { left._node, right._node } });

    internal GateNode ToNode() => _node;
}

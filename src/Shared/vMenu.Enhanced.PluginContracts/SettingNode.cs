namespace vMenu.Enhanced.PluginContracts;

public static class SettingTypes
{
    public const string Bool = "bool";
    public const string Int = "int";
    public const string Float = "float";
    public const string String = "string";
}

/// <summary>A convar setting the plugin declares. vMenu materialises it as
/// <c>vMenu.Enhanced.Plugins.&lt;Id&gt;.&lt;Name&gt;</c> and tracks it for live menu refresh. The
/// default travels as a string in the convar's own text form.</summary>
public class SettingNode
{
    public string Name { get; set; } = string.Empty;

    /// <summary>One of the <see cref="SettingTypes"/> values.</summary>
    public string Type { get; set; } = SettingTypes.Bool;

    public string Default { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

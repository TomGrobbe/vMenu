namespace vMenu.Enhanced.PluginContracts;

/// <summary>A theme a resource offers to vMenu. The stylesheet is loaded by vMenu's own NUI page
/// straight out of the resource that registered it, so nothing has to be copied into vMenu.</summary>
public class ThemeSource
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>The stylesheet, either a path inside the registering resource or a full
    /// <c>https://cfx-nui-&lt;resource&gt;/</c> url naming another one.</summary>
    public string Css { get; set; } = string.Empty;

    public string? Banner { get; set; }
}

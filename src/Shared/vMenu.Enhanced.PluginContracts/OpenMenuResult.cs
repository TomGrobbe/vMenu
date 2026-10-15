namespace vMenu.Enhanced.PluginContracts;

/// <summary>vMenu's answer to an openMenu op that carried a request id.</summary>
public class OpenMenuResult
{
    public int RequestId { get; set; }

    public bool Opened { get; set; }

    /// <summary>One of the <see cref="OpenMenuReasons"/> values.</summary>
    public string Reason { get; set; } = string.Empty;
}

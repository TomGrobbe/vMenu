namespace vMenu.Enhanced.PluginContracts;

/// <summary>Protocol wide constants. The version rides in every registration so either side can
/// refuse a payload it does not understand. Unknown JSON fields are ignored and unknown update
/// operations are skipped, so the version only moves on breaking changes.</summary>
public static class PluginProtocol
{
    public const int Version = 1;

    /// <summary>The resource name vMenu Enhanced runs under, enforced by vMenu itself.</summary>
    public const string VMenuResource = "vMenu.Enhanced";
}

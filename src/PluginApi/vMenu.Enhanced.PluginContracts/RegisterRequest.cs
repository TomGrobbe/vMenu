namespace vMenu.Enhanced.PluginContracts;

/// <summary>
/// The client side registration payload: the plugin's whole menu tree plus everything
/// needed to present it. Registration is idempotent, a re-register replaces the plugin's
/// previous tree entirely.
/// </summary>
public class RegisterRequest
{
    public int ProtocolVersion { get; set; } = PluginProtocol.Version;

    /// <summary>Shown as the plugin's row in the Plugins menu. The resource name becomes the row description.</summary>
    public TextRef? DisplayName { get; set; }

    /// <summary>Optional extra line under the resource name in the row description.</summary>
    public string? DescriptionKey { get; set; }

    /// <summary>Language code to key to text. An "en" table is required and is the fallback.</summary>
    public Dictionary<string, Dictionary<string, string>>? Translations { get; set; }

    /// <summary>Settings mirrored from the server declaration so gates can reference them client side.</summary>
    public List<SettingNode>? Settings { get; set; }

    public MenuNode? Menu { get; set; }

    /// <summary>Items injected into every player's Plugin Actions submenu in Online Players.</summary>
    public List<ItemNode>? PlayerActions { get; set; }
}

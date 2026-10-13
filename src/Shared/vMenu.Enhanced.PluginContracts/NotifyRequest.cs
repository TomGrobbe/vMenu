namespace vMenu.Enhanced.PluginContracts;

/// <summary>
/// A notification shown through vMenu's notification area. Style is "info", "success",
/// "warning" or "error", anything else falls back to info. The duration is clamped by
/// vMenu the same way as the public vmenu:notify event.
/// </summary>
public class NotifyRequest
{
    public string Style { get; set; } = "info";

    public TextRef? Text { get; set; }

    public int? DurationMs { get; set; }
}

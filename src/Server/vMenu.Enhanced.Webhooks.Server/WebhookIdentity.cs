using CitizenFX.FiveM.Server;

namespace vMenu.Enhanced.Webhooks.Server;

public static class WebhookIdentity
{
    public const string Resource = "vMenu.Enhanced";

    private const string VersionKey = "version";

    private const string Unstamped = "versiongoeshere";

    private static string? _version;

    public static string Version => _version ??= Read();

    public static string UserAgent() => Resource + "/" + Version;

    public static string Footer() => "vMenu Enhanced " + Version;

    private static string Read()
    {
        var resource = Native.GetCurrentResourceName();

        var text = Native.GetNumResourceMetadata(resource, VersionKey) == 0
            ? null
            : Native.GetResourceMetadata(resource, VersionKey, 0)?.Trim();

        return string.IsNullOrEmpty(text) || string.Equals(text, Unstamped, StringComparison.Ordinal)
            ? "dev"
            : text;
    }
}

using System.Text;

using CitizenFX.FiveM.Server;

namespace vMenu.Enhanced.Webhooks.Server;

public static class WebhookIdentity
{
    public const string Resource = "vMenu.Enhanced";

    private const string VersionKey = "version";

    private const string Unstamped = "versiongoeshere";

    private const string Hostname = "sv_hostname";

    private const int MaxHostnameLength = 100;

    private const string Separator = " · ";

    private static string? _version;

    public static string Version => _version ??= Read();

    public static string UserAgent() => Resource + "/" + Version;

    public static string Footer()
    {
        var host = ServerName();
        var self = "vMenu Enhanced " + Version;

        return host.Length == 0 ? self : host + Separator + self;
    }

    // sv_hostname carries FiveM colour codes and can hold control characters. Strip both so the
    // footer reads as plain text.
    private static string ServerName()
    {
        var raw = Native.GetConvar(Hostname, string.Empty);

        if (string.IsNullOrEmpty(raw))
        {
            return string.Empty;
        }

        var text = new StringBuilder(raw.Length);

        for (var index = 0; index < raw.Length; index++)
        {
            var character = raw[index];

            if (character == '^' && index + 1 < raw.Length && char.IsDigit(raw[index + 1]))
            {
                index++;

                continue;
            }

            if (!char.IsControl(character))
            {
                text.Append(character);
            }
        }

        var name = text.ToString().Trim();

        if (name.Length <= MaxHostnameLength)
        {
            return name;
        }

        // Never cut through a surrogate pair, which would leave a lone half in the footer.
        var cut = char.IsHighSurrogate(name[MaxHostnameLength - 1]) ? MaxHostnameLength - 1 : MaxHostnameLength;

        return name[..cut].TrimEnd();
    }

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

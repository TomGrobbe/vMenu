using vMenu.Enhanced.Data.Configuration;

namespace vMenu.Enhanced.Plugins.Server;

// What the server remembers about one registered plugin.
public sealed class RegisteredServerPlugin
{
    public required string Resource { get; init; }

    // Sanitized identity used inside permission and convar names.
    public required string Id { get; init; }

    public required string DisplayName { get; init; }

    public required IReadOnlyList<Setting> Settings { get; init; }

    public required IReadOnlyList<string> Permissions { get; init; }
}

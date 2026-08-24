using vMenu.Enhanced.Data.Updates;

namespace vMenu.Enhanced.Updates.Server;

// A class rather than a record: generated equality routes through
// EqualityComparer<string>.Default, which the sandbox refuses to load.
internal sealed class KnownUpdate(SemanticVersion version, string url, string source)
{
    public SemanticVersion Version { get; } = version;

    public string Url { get; } = url;

    // github or nuget. Only ever shown in the debug log.
    public string Source { get; } = source;
}

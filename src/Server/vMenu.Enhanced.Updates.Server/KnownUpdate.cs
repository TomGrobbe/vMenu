using vMenu.Enhanced.Data.Updates;

namespace vMenu.Enhanced.Updates.Server;

/// <summary>A release that is newer than the one running.</summary>
// A plain class rather than a record, matching the rest of this codebase: the generated equality
// routes through EqualityComparer<string>.Default, which the sandbox refuses to load.
internal sealed class KnownUpdate(SemanticVersion version, string url, string source)
{
    public SemanticVersion Version { get; } = version;

    public string Url { get; } = url;

    /// <summary>github or nuget. Only ever shown in the debug log.</summary>
    public string Source { get; } = source;
}

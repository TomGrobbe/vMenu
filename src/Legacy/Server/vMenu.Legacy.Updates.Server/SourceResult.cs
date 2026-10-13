namespace vMenu.Enhanced.Updates.Server;

// Whether a source could be read, kept apart from whether it had anything. The whole reason this is
// not just a nullable KnownUpdate: on the stable channel there is nothing to find today, every
// enhanced release so far being a prerelease. Conflating that with an unreachable github.com would
// warn four times a day on a correctly configured server.
internal sealed class SourceResult(bool reached, KnownUpdate? update)
{
    public static readonly SourceResult Unreachable = new(false, null);

    public bool Reached { get; } = reached;

    public KnownUpdate? Update { get; } = update;

    public static SourceResult Nothing() => new(true, null);

    public static SourceResult Found(KnownUpdate update) => new(true, update);
}

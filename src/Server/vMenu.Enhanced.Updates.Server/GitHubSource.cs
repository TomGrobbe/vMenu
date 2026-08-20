using System.Globalization;

using vMenu.Enhanced.Data.Updates;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization.Server;
using vMenu.Enhanced.Updates.Server.Dto;
using vMenu.Enhanced.Updates.Server.Http;

namespace vMenu.Enhanced.Updates.Server;

internal static class GitHubSource
{
    public const string Accept = "application/vnd.github+json";

    /// <summary>The release list, which is the only endpoint that can see an enhanced release.</summary>
    // /releases/latest is useless here: it excludes prereleases and drafts, and every enhanced
    // release so far is at least one of those. per_page keeps the body down, the release notes being
    // most of it.
    public const string ReleasesUrl = "https://api.github.com/repos/TomGrobbe/vMenu/releases?per_page=50";

    public const string ReleasesPage = "https://github.com/TomGrobbe/vMenu/releases";

    private const string EnhancedPrefix = "enhanced-v";

    private const string CombinedMarker = "-and-enhanced-v";

    // Unauthenticated GitHub allows 60 requests an hour from one address, and this makes four a day
    // on the schedule. A shared host that does hit the limit gets a 403, which is a debug line and a
    // fall through to nuget.org rather than a warning.
    public static async Task<SourceResult> LatestAsync(UpdateChannel channel, string userAgent, int timeoutMs)
    {
        var reply = await HttpGet.GetAsync(new HttpRequest(ReleasesUrl, Accept, userAgent, timeoutMs));

        if (!reply.IsOk)
        {
            Log.Debug(
                "[Updates] github.com did not answer usefully: " +
                (reply.Status > 0 ? reply.Status.ToString(CultureInfo.InvariantCulture) : reply.Reason));

            return SourceResult.Unreachable;
        }

        if (!ServerJson.TryDeserialize<GitHubRelease[]>(reply.Body, out var releases, out var error) || releases is null)
        {
            Log.Debug($"[Updates] The release list from github.com did not read: {error}");

            return SourceResult.Unreachable;
        }

        Log.Debug($"[Updates] github.com listed {releases.Length} release(s).");

        KnownUpdate? best = null;

        foreach (var release in releases)
        {
            // Anonymous callers never see drafts anyway. Checked because a server that ever runs this
            // with a token in the environment would start seeing them.
            if (release.Draft || release.TagName is not { } tag)
            {
                continue;
            }

            if (channel == UpdateChannel.Stable && release.Prerelease)
            {
                continue;
            }

            if (VersionIn(tag) is not { } text
                || !SemanticVersion.TryParse(text, out var version)
                || version is null)
            {
                continue;
            }

            // Belt and braces over the release flag: the combined workflow does not pass --prerelease,
            // so a combined release carrying an alpha enhanced build reads as stable at the GitHub
            // level while its version says otherwise. The version wins.
            if (channel == UpdateChannel.Stable && version.IsPrerelease)
            {
                continue;
            }

            if (best is null || version.IsNewerThan(best.Version))
            {
                best = new KnownUpdate(version, release.HtmlUrl ?? ReleasesPage, "github");
            }
        }

        return best is null ? SourceResult.Nothing() : SourceResult.Found(best);
    }

    /// <summary>The enhanced version inside a tag, or null when the tag is not one of ours.</summary>
    // StartsWith and not Contains for the first case: combined-v3.9.0-and-enhanced-v0.0.2 contains
    // enhanced-v in the middle, so a Contains check would take the wrong substring. Legacy v3.x.y
    // tags fall through to null and are ignored.
    private static string? VersionIn(string tag)
    {
        if (tag.StartsWith(EnhancedPrefix, StringComparison.Ordinal))
        {
            return tag[EnhancedPrefix.Length..];
        }

        var marker = tag.IndexOf(CombinedMarker, StringComparison.Ordinal);

        return marker < 0 ? null : tag[(marker + CombinedMarker.Length)..];
    }
}

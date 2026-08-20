using System.Globalization;

using vMenu.Enhanced.Data.Updates;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization.Server;
using vMenu.Enhanced.Updates.Server.Dto;
using vMenu.Enhanced.Updates.Server.Http;

namespace vMenu.Enhanced.Updates.Server;

internal static class NugetSource
{
    public const string Accept = "application/json";

    /// <summary>Every published version of the server plugin API package.</summary>
    // The flat container wants the id lowercased. The plugin API packages are pushed on every
    // enhanced build with exactly the resource's semver and none of them is ever drafted, which is
    // what makes this worth having as a fallback. It carries no release link, so anything found here
    // points at the releases page.
    public const string IndexUrl = "https://api.nuget.org/v3-flatcontainer/vmenu.enhanced.serverapi/index.json";

    public static async Task<SourceResult> LatestAsync(UpdateChannel channel, string userAgent, int timeoutMs)
    {
        var reply = await HttpGet.GetAsync(new HttpRequest(IndexUrl, Accept, userAgent, timeoutMs));

        if (!reply.IsOk)
        {
            Log.Debug(
                "[Updates] nuget.org did not answer usefully: " +
                (reply.Status > 0 ? reply.Status.ToString(CultureInfo.InvariantCulture) : reply.Reason));

            return SourceResult.Unreachable;
        }

        if (!ServerJson.TryDeserialize<NugetVersionIndex>(reply.Body, out var index, out var error)
            || index?.Versions is not { } versions)
        {
            Log.Debug($"[Updates] The version list from nuget.org did not read: {error}");

            return SourceResult.Unreachable;
        }

        Log.Debug($"[Updates] nuget.org listed {versions.Length} version(s).");

        SemanticVersion? best = null;

        foreach (var text in versions)
        {
            if (!SemanticVersion.TryParse(text, out var version) || version is null)
            {
                continue;
            }

            if (channel == UpdateChannel.Stable && version.IsPrerelease)
            {
                continue;
            }

            if (best is null || version.IsNewerThan(best))
            {
                best = version;
            }
        }

        return best is null
            ? SourceResult.Nothing()
            : SourceResult.Found(new KnownUpdate(best, GitHubSource.ReleasesPage, "nuget"));
    }
}

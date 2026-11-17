using System.Globalization;

using vMenu.Enhanced.Data.Updates;
using vMenu.Enhanced.Http.Server;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization.Server;
using vMenu.Enhanced.Updates.Server.Dto;

namespace vMenu.Enhanced.Updates.Server;

// Dev builds never get a release, their zips only live as artifacts on the dev-enhanced.yml run that
// built them. That run is found by number, which the version carries as -dev.<run_number>.
internal static class BuildSource
{
    private const string DevLabel = "-dev.";

    private const string RunsUrl =
        "https://api.github.com/repos/TomGrobbe/vMenu/actions/workflows/dev-enhanced.yml/runs" +
        "?branch=develop&event=push&status=success&per_page=20";

    private const string ArtifactsUrl = "https://api.github.com/repos/TomGrobbe/vMenu/actions/runs/{0}/artifacts";

    private const string BuildsPage =
        "https://github.com/TomGrobbe/vMenu/actions/workflows/dev-enhanced.yml?query=branch%3Adevelop+is%3Asuccess";

    public static int? RunNumber(SemanticVersion version)
    {
        var marker = version.Text.IndexOf(DevLabel, StringComparison.Ordinal);

        if (marker < 0)
        {
            return null;
        }

        var rest = version.Text[(marker + DevLabel.Length)..];
        var dot = rest.IndexOf('.');

        if (dot >= 0)
        {
            rest = rest[..dot];
        }

        return int.TryParse(rest, NumberStyles.None, CultureInfo.InvariantCulture, out var run) ? run : null;
    }

    // The artifact itself when it is still there, its run when it expired, and the list of develop builds
    // when the run is not among the latest ones.
    public static async Task<string> DownloadUrlAsync(int runNumber, string artifactName, string userAgent, int timeoutMs)
    {
        var runs = await ReadAsync<GitHubWorkflowRuns>(RunsUrl, "build list", userAgent, timeoutMs);
        GitHubWorkflowRun? found = null;

        foreach (var run in runs?.WorkflowRuns ?? [])
        {
            if (run.RunNumber == runNumber)
            {
                found = run;

                break;
            }
        }

        if (found?.HtmlUrl is not string runUrl)
        {
            Log.Debug($"[Updates] github.com has no finished develop build #{runNumber}, linking the build list instead.");

            return BuildsPage;
        }

        var url = string.Format(CultureInfo.InvariantCulture, ArtifactsUrl, found.Id);
        var artifacts = await ReadAsync<GitHubArtifacts>(url, "artifact list", userAgent, timeoutMs);

        foreach (var artifact in artifacts?.Artifacts ?? [])
        {
            if (!artifact.Expired && string.Equals(artifact.Name, artifactName, StringComparison.Ordinal))
            {
                return $"{runUrl}/artifacts/{artifact.Id.ToString(CultureInfo.InvariantCulture)}";
            }
        }

        Log.Debug($"[Updates] Develop build #{runNumber} has no {artifactName} left, linking the build instead.");

        return runUrl;
    }

    private static async Task<T?> ReadAsync<T>(string url, string what, string userAgent, int timeoutMs)
        where T : class
    {
        var reply = await HttpSend.SendAsync(new HttpRequest(url, GitHubSource.Accept, userAgent, timeoutMs));

        if (!reply.IsOk)
        {
            Log.Debug(
                $"[Updates] github.com did not answer the {what} usefully: " +
                (reply.Status > 0 ? reply.Status.ToString(CultureInfo.InvariantCulture) : reply.Reason));

            return null;
        }

        if (!ServerJson.TryDeserialize<T>(reply.Body, out var value, out var error))
        {
            Log.Debug($"[Updates] The {what} from github.com did not read: {error}");

            return null;
        }

        return value;
    }
}

using Newtonsoft.Json;

namespace vMenu.Enhanced.Updates.Server.Dto;

internal sealed class GitHubArtifacts
{
    [JsonProperty("artifacts")]
    public GitHubArtifact[]? Artifacts { get; set; }
}

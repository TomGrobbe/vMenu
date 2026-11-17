using System.Text.Json.Serialization;

namespace vMenu.Enhanced.Updates.Server.Dto;

internal sealed class GitHubArtifacts
{
    [JsonPropertyName("artifacts")]
    public GitHubArtifact[]? Artifacts { get; set; }
}

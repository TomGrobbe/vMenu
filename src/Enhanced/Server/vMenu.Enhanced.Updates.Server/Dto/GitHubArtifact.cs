using System.Text.Json.Serialization;

namespace vMenu.Enhanced.Updates.Server.Dto;

internal sealed class GitHubArtifact
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("expired")]
    public bool Expired { get; set; }
}

using Newtonsoft.Json;

namespace vMenu.Enhanced.Updates.Server.Dto;

internal sealed class GitHubArtifact
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("expired")]
    public bool Expired { get; set; }
}

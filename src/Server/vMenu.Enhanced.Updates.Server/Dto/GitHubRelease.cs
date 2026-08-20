using System.Text.Json.Serialization;

namespace vMenu.Enhanced.Updates.Server.Dto;

// Every name is explicit. ServerJson resolves properties through a camel case policy, so TagName
// would be looked up as tagName while GitHub sends tag_name. Without these, every field reads back as
// null and the checker silently decides there are no releases.
internal sealed class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public string? TagName { get; set; }

    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }

    [JsonPropertyName("draft")]
    public bool Draft { get; set; }

    [JsonPropertyName("prerelease")]
    public bool Prerelease { get; set; }
}

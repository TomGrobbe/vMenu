using System.Text.Json.Serialization;

namespace vMenu.Enhanced.Updates.Server.Dto;

internal sealed class GitHubWorkflowRun
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("run_number")]
    public int RunNumber { get; set; }

    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }
}

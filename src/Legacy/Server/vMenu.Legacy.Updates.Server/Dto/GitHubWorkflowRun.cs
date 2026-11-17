using Newtonsoft.Json;

namespace vMenu.Enhanced.Updates.Server.Dto;

internal sealed class GitHubWorkflowRun
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("run_number")]
    public int RunNumber { get; set; }

    [JsonProperty("html_url")]
    public string? HtmlUrl { get; set; }
}

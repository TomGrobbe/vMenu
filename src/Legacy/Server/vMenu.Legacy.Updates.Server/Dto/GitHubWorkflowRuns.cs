using Newtonsoft.Json;

namespace vMenu.Enhanced.Updates.Server.Dto;

internal sealed class GitHubWorkflowRuns
{
    [JsonProperty("workflow_runs")]
    public GitHubWorkflowRun[]? WorkflowRuns { get; set; }
}

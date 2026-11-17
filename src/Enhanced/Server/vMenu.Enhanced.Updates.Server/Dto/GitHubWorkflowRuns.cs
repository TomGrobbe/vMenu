using System.Text.Json.Serialization;

namespace vMenu.Enhanced.Updates.Server.Dto;

internal sealed class GitHubWorkflowRuns
{
    [JsonPropertyName("workflow_runs")]
    public GitHubWorkflowRun[]? WorkflowRuns { get; set; }
}

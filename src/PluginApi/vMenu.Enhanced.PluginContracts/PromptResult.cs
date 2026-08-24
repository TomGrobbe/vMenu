namespace vMenu.Enhanced.PluginContracts;

/// <summary>The answer to a <see cref="PromptRequest"/>. Busy means another input was already open
/// and the request was refused, cancelled means the player backed out.</summary>
public class PromptResult
{
    public int RequestId { get; set; }

    public bool Cancelled { get; set; }

    public bool Busy { get; set; }

    public List<string>? Answers { get; set; }
}

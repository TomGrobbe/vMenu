namespace vMenu.Enhanced.PluginContracts;

public class SuggestionNode
{
    public string Value { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class PromptNode
{
    public TextRef? Title { get; set; }

    public int MaxLength { get; set; } = 60;

    public string Initial { get; set; } = string.Empty;

    public List<SuggestionNode>? Suggestions { get; set; }
}

/// <summary>Asks vMenu to open its text input for the player. The request id correlates the answer,
/// chosen by the plugin side and echoed back in the result.</summary>
public class PromptRequest
{
    public int RequestId { get; set; }

    public List<PromptNode> Prompts { get; set; } = new();
}

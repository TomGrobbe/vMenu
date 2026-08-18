namespace vMenu.Enhanced.ClientAPI;

/// <summary>One suggestion row the input box offers while the player types.</summary>
public sealed class PromptSuggestion
{
    public PromptSuggestion(string value, string? description = null)
    {
        Value = value;
        Description = description;
    }

    /// <summary>What lands in the box when the suggestion is picked.</summary>
    public string Value { get; }

    /// <summary>What the player reads in the list. The value is shown when omitted.</summary>
    public string? Description { get; }
}

/// <summary>One question in a multi prompt input session.</summary>
public sealed class PluginPrompt
{
    public PluginPrompt(Text title, int maxLength = 60, string initialValue = "", IReadOnlyList<PromptSuggestion>? suggestions = null)
    {
        Title = title;
        MaxLength = maxLength;
        InitialValue = initialValue;
        Suggestions = suggestions;
    }

    public Text Title { get; }

    public int MaxLength { get; }

    public string InitialValue { get; }

    public IReadOnlyList<PromptSuggestion>? Suggestions { get; }
}

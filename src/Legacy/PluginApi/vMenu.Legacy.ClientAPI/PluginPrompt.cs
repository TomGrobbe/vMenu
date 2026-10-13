namespace vMenu.Enhanced.ClientAPI;

/// <summary>One suggestion row the input box offers while the player types.</summary>
public sealed class PromptSuggestion(string value, string? description = null)
{
    /// <summary>What lands in the box when the suggestion is picked.</summary>
    public string Value { get; } = value;

    /// <summary>What the player reads in the list. The value is shown when omitted.</summary>
    public string? Description { get; } = description;
}

/// <summary>One question in a multi prompt input session.</summary>
public sealed class PluginPrompt(Text title, int maxLength = 60, string initialValue = "", IReadOnlyList<PromptSuggestion>? suggestions = null)
{
    public Text Title { get; } = title;

    public int MaxLength { get; } = maxLength;

    public string InitialValue { get; } = initialValue;

    public IReadOnlyList<PromptSuggestion>? Suggestions { get; } = suggestions;
}

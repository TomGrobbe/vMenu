using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

// One question in a UserInput.GetTextAsync session. A class, not a record, for the same reason as
// InputSuggestion.
public sealed class InputPrompt(
    MenuText title,
    int maxLength,
    string initialValue = "",
    IReadOnlyList<InputSuggestion>? suggestions = null,
    bool suggestWhenEmpty = false,
    MenuText description = default)
{
    public MenuText Title { get; } = title;

    public MenuText Description { get; } = description;

    public int MaxLength { get; } = maxLength;

    public string InitialValue { get; } = initialValue;

    public IReadOnlyList<InputSuggestion>? Suggestions { get; } = suggestions;

    public bool SuggestWhenEmpty { get; } = suggestWhenEmpty;
}

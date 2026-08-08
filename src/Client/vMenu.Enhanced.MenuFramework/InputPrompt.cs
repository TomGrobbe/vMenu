using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>One question in a <see cref="UserInput.GetTextAsync(InputPrompt[])"/> session.</summary>
// A class, not a record, for the same reason as InputSuggestion.
public sealed class InputPrompt(
    MenuText title,
    int maxLength,
    string initialValue = "",
    IReadOnlyList<InputSuggestion>? suggestions = null)
{
    public MenuText Title { get; } = title;

    public int MaxLength { get; } = maxLength;

    public string InitialValue { get; } = initialValue;

    public IReadOnlyList<InputSuggestion>? Suggestions { get; } = suggestions;
}

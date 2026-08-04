namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// One row the input prompt can offer while the player types. <see cref="Value"/> lands in the box
/// when it is picked, <see cref="Label"/> is what the player reads, and both are matched against.
/// </summary>
// A class, not a record: the generated equality routes through EqualityComparer<string>.Default,
// which the sandbox refuses to load.
public sealed class InputSuggestion
{
    public required string Value { get; init; }

    public required string Label { get; init; }

    public string? Icon { get; init; }

    public string? Detail { get; init; }
}

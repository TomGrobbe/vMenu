namespace vMenu.Enhanced.MenuFramework;

// One row the input prompt can offer while the player types. Value lands in the box when it is
// picked, Label is what the player reads, and both are matched against.
//
// A class rather than a record: generated equality routes through
// EqualityComparer<string>.Default, which the sandbox refuses to load.
public sealed class InputSuggestion
{
    public required string Value { get; init; }

    public required string Label { get; init; }

    public string? Icon { get; init; }

    public string? Detail { get; init; }
}

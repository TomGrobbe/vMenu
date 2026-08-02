namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// One row the input prompt can offer while the player types. <see cref="Value"/> lands in the box
/// when it is picked, <see cref="Label"/> is what the player reads, and both are matched against.
/// </summary>
/// <remarks>
/// A class, not a record: the generated equality members route through
/// <c>EqualityComparer&lt;string&gt;.Default</c>, which the FiveM sandbox refuses to load.
/// </remarks>
public sealed class InputSuggestion
{
    public required string Value { get; init; }

    public required string Label { get; init; }

    public string? Icon { get; init; }

    public string? Detail { get; init; }
}

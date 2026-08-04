namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// Static metadata for a <see cref="MenuDefinition"/>.
/// </summary>
// A constant cannot express text computed at runtime, so a subclass may override the matching
// MenuDefinition property instead, and the override wins.
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class VMenuAttribute : Attribute
{
    /// <summary>Translation key for the menu's title, and for the item that opens it.</summary>
    public required string TitleKey { get; init; }

    public string? SubtitleKey { get; init; }

    /// <summary>Translation key for the description of the item that opens this menu.</summary>
    public string? DescriptionKey { get; init; }

    /// <summary>Permission gating the item that opens this menu, and so the menu itself.</summary>
    public string? Permission { get; init; }

    public string LinkLabel { get; init; } = "→";
}

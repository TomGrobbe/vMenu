namespace vMenu.Enhanced.MenuFramework;

// Static metadata for a MenuDefinition. A constant cannot express text computed at runtime, so a
// subclass may override the matching MenuDefinition property instead, and the override wins.
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class VMenuAttribute : Attribute
{
    // Translation key for the menu's title, and for the item that opens it.
    public required string TitleKey { get; init; }

    public string? SubtitleKey { get; init; }

    // Translation key for the description of the item that opens this menu.
    public string? DescriptionKey { get; init; }

    // Permission gating the item that opens this menu, and so the menu itself.
    public string? Permission { get; init; }

    public string LinkLabel { get; init; } = "→";
}

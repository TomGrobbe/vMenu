using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

public sealed class ButtonHint
{
    public required Control Control { get; init; }

    public required MenuText Text { get; init; }

    // If gate is not allowed, button is removed from the instructional buttons.
    public MenuGate Gate { get; init; } = MenuGate.Always;

    public static implicit operator ButtonHint((Control Control, MenuText Text) hint) =>
        new() { Control = hint.Control, Text = hint.Text };
}

using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

public sealed class ButtonHint
{
    public required string Name { get; init; }

    public required MenuText Description { get; init; }

    public required string DefaultKey { get; init; }

    public string? DefaultButton { get; init; }

    public MenuKeyPressType PressType { get; init; } = MenuKeyPressType.JUST_PRESSED;

    public Action<Menu, MenuKeyBinding>? Handler { get; init; }

    public required MenuText Text { get; init; }

    public Control? ShadowedControl { get; init; }

    public MenuGate Gate { get; init; } = MenuGate.Always;
}

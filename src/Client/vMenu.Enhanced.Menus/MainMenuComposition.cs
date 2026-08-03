using vMenu.Enhanced.MenuFramework;

namespace vMenu.Enhanced.Menus;

/// <summary>
/// What the main menu contains, in the order it appears on screen.
/// </summary>
/// <remarks>
/// An explicit list rather than attribute discovery. Order is a product decision that belongs in one
/// readable place, not an integer spread across a dozen files; and scanning assemblies would be the
/// wrong cost to pay in the client runtime, per player, on script start.
/// </remarks>
public static class MainMenuComposition
{
    public static IReadOnlyList<MenuDefinition> Definitions =>
    [
        new VehicleOptionsMenu(),
        new VehicleSpawnerMenu(),
        new MiscSettingsMenu(),
        new DeveloperFeaturesMenu(),
    ];
}

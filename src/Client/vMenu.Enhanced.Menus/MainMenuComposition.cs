using vMenu.Enhanced.MenuFramework;

namespace vMenu.Enhanced.Menus;

/// <summary>What the main menu contains, in the order it appears on screen.</summary>
// An explicit list, not attribute discovery. Order is a product decision that belongs in one
// readable place, and scanning assemblies would be the wrong cost per player on script start.
public static class MainMenuComposition
{
    public static IReadOnlyList<MenuDefinition> Definitions =>
    [
        new OnlinePlayersMenu(),
        new PlayerOptions(),
        new VehicleOptionsMenu(),
        new VehicleSpawnerMenu(),
        new WorldMenu(),
        new TeleportMenu(),
        new RecordingMenu(),
        new MiscSettingsMenu(),
        new DeveloperFeaturesMenu(),
        new AboutMenu(),
    ];
}

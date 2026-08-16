using vMenu.Enhanced.MenuFramework;

namespace vMenu.Enhanced.Menus;


public static class MainMenuComposition
{
    public static IReadOnlyList<MenuDefinition> Definitions =>
    [
        new StaffAlertsMenu(),
        new OnlinePlayersMenu(),
        new PlayerMenu(),
        new VehiclesMenu(),
        new WorldMenu(),
        new TeleportMenu(),
        new RecordingMenu(),
        new MiscSettingsMenu(),
        new DeveloperFeaturesMenu(),
        new AboutMenu(),
    ];
}

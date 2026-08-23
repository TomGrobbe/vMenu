using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Plugins;

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
        new PropSpawnerMenu(),
        new RecordingMenu(),
        new DisplaySettingsMenu(),
        new MiscSettingsMenu(),
        new DeveloperFeaturesMenu(),
        new PluginsMenu(),
        new AboutMenu(),
    ];
}

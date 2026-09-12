namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class KeyBindings
{
    private const string KeyNote =
        "Use a key name from https://docs.fivem.net/docs/game-references/input-mapper-parameter-ids/keyboard/. " +
        "This is only the default key: players can rebind it themselves under Settings, Key Bindings";

    public static readonly StringSetting MenuToggleKey = new("vMenu.Enhanced.KeyBindings.MenuToggleKey")
    {
        Description = "The default key that opens and closes the menu. " + KeyNote,
        Default = "M",
    };

    public static readonly StringSetting NoClipToggleKey = new("vMenu.Enhanced.KeyBindings.NoClipToggleKey")
    {
        Description = "The default key that turns noclip on and off. " + KeyNote,
        Default = "F2",
    };

    public static readonly StringSetting TeleportKey = new("vMenu.Enhanced.KeyBindings.TeleportKey")
    {
        Description =
            "The default key that runs the 'teleport action' (see teleportation menu in-game) " + KeyNote,
        Default = "F10",
    };
}

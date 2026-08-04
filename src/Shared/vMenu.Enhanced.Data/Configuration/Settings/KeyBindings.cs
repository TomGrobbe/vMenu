namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class KeyBindings
{
    private const string KeyNote =
        "Use a key name from https://docs.fivem.net/docs/game-references/input-mapper-parameter-ids/keyboard/. " +
        "This only sets the starting key: players can rebind it themselves under Settings, Key Bindings, and " +
        "once they have, their choice wins and changing this does nothing for them.";

    public static readonly StringSetting MenuToggleKey = new("vMenu.Enhanced.KeyBindings.MenuToggleKey")
    {
        Description = "The key that opens and closes the vMenu menu. " + KeyNote,
        Default = "M",
    };

    public static readonly StringSetting NoClipToggleKey = new("vMenu.Enhanced.KeyBindings.NoClipToggleKey")
    {
        Description = "The key that turns noclip on and off, for players allowed to use it. " + KeyNote,
        Default = "F2",
    };
}

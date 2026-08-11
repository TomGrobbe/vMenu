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

    public static readonly StringSetting TeleportKey = new("vMenu.Enhanced.KeyBindings.TeleportKey")
    {
        Description =
            "The key that runs the teleport each player picked for it under the teleport menu. It does " +
            "nothing until they pick one, and only teleports them if they are allowed to. " + KeyNote,
        Default = "F10",
    };

    public static readonly StringSetting VisorToggleKey = new("vMenu.Enhanced.KeyBindings.VisorToggleKey")
    {
        Description =
            "The key players hold to flip the visor on a motorcycle helmet up or down. It has to be held " +
            "for a moment rather than tapped, because on a controller it shares a button with the " +
            "headlights. It does nothing unless they are wearing a helmet that has a visor. " + KeyNote,
        Default = "F11",
    };

    public static readonly StringSetting VisorToggleButton = new("vMenu.Enhanced.KeyBindings.VisorToggleButton")
    {
        Description =
            "The controller button for the same thing, which by default is D-pad right, the button the " +
            "game itself puts the visor on. Use a button name from " +
            "https://docs.fivem.net/docs/game-references/input-mapper-parameter-ids/pad_digitalbutton/. " +
            "This only sets the starting button: players can rebind it themselves under Settings, Key " +
            "Bindings, and once they have, their choice wins and changing this does nothing for them.",
        Default = "LRIGHT_INDEX",
    };
}

using CitizenFX.FiveM.Shared;

namespace vMenu.Enhanced.Menus.Misc;

public static class MinimapKeyBinding
{
    private const string Command = "vmenu:minimap";
    private const string DefaultKey = "Z";

    private const string DefaultButton = "LDOWN_INDEX";

    private static bool _registered;

    public static void Register(Action onPressed)
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        var padCommand = KeyMapping.Pad(Command);

        SharedAPI.Commands.RegisterCommand(Command, false, onPressed);
        SharedAPI.Commands.RegisterCommand(padCommand, false, onPressed);

        KeyMapping.Register(Command, padCommand, "vMenu: Expand or zoom the minimap", DefaultKey, DefaultButton);
    }
}

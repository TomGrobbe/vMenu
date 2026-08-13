using CitizenFX.FiveM.Client;
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

        SharedAPI.Commands.RegisterCommand(Command, false, onPressed);

        const string Description = "vMenu: Expand or zoom the minimap";

        Native.RegisterKeyMapping(Command, Description, "keyboard", DefaultKey);
        Native.RegisterKeyMapping(Command, Description, "PAD_DIGITALBUTTONANY", DefaultButton);
    }
}

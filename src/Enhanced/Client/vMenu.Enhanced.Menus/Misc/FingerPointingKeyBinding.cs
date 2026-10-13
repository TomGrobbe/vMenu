using CitizenFX.FiveM.Shared;

namespace vMenu.Enhanced.Menus.Misc;

public static class FingerPointingKeyBinding
{
    private const string Command = "vmenu:point";

    private const string Key = "B";

    private const string Button = "R3_INDEX";

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

        KeyMapping.Register(Command, padCommand, "vMenu: Point your finger", Key, Button);
    }
}

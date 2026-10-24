using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Players.Character;

internal static class CharacterCameraKeyBinding
{
    private const string Command = "vmenu:character:autocamera";

    private const string Key = "N";

    private const string Button = "R1_INDEX";

    internal static int KeyboardControl { get; } = BindingControl(Command);

    internal static int ControllerControl { get; } = BindingControl(KeyMapping.Pad(Command));

    internal static int UnboundControl { get; } = BindingControl($"{Command}:unbound");

    internal static Control ControllerDefault => Control.FrontendRb;

    private static bool _registered;

    internal static void Register(Action onPressed)
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        var padCommand = KeyMapping.Pad(Command);

        Commands.Register(Command, false, onPressed);
        Commands.Register(padCommand, false, onPressed);

        KeyMapping.Register(Command, padCommand, "vMenu: Character creator auto camera", Key, Button);
    }

    private static int BindingControl(string command) => API.GetHashKey(command) | int.MinValue;
}

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus;

public static class KeyMapping
{
    public static string Pad(string command) => $"{command}:pad";

    public static void Register(
        string keyboardCommand,
        string? controllerCommand,
        string description,
        string keyboardKey,
        string? controllerButton)
    {
        API.RegisterKeyMapping(keyboardCommand, description, "keyboard", keyboardKey);

        if (string.IsNullOrWhiteSpace(controllerCommand) || string.IsNullOrWhiteSpace(controllerButton))
        {
            return;
        }

        API.RegisterKeyMapping(controllerCommand, $"{description} (controller)", "PAD_ANALOGBUTTON", controllerButton);
    }
}

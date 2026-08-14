using CitizenFX.FiveM.Client;

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
        Native.RegisterKeyMapping(keyboardCommand, description, "keyboard", keyboardKey);

        if (string.IsNullOrWhiteSpace(controllerCommand) || string.IsNullOrWhiteSpace(controllerButton))
        {
            return;
        }

        Native.RegisterKeyMapping(controllerCommand, $"{description} (controller)", "PAD_ANALOGBUTTON", controllerButton);
    }
}

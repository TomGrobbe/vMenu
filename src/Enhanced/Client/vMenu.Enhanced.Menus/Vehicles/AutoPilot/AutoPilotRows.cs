using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

namespace vMenu.Enhanced.Menus.Vehicles.AutoPilot;

internal static class AutoPilotRows
{
    internal static SeparatorEntry Group(string textKey, string descriptionKey) => new()
    {
        Text = MenuText.Key(textKey),
        Description = MenuText.Key(descriptionKey),
    };

    internal static ButtonEntry Placeholder(string textKey, string descriptionKey) => new()
    {
        Text = MenuText.Key(textKey),
        Description = MenuText.Key(descriptionKey),
        ReadEnabled = () => false,
    };
}

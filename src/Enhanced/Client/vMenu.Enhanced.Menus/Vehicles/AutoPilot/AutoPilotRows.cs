using vMenu.Enhanced.MenuFramework;

namespace vMenu.Enhanced.Menus.Vehicles.AutoPilot;

internal static class AutoPilotRows
{
    internal static ButtonEntry Placeholder(string textKey) => new(textKey)
    {
        ReadEnabled = () => false,
    };
}

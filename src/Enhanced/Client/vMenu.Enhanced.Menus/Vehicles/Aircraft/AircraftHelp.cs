using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Vehicles.Aircraft;

internal static class AircraftHelp
{
    private static int _shownFrame = -1;

    public static bool CanShow => _shownFrame != Native.GetFrameCount() && !Native.IsHelpMessageBeingDisplayed();

    public static void Show(string label)
    {
        if (!Native.DoesTextLabelExist(label))
        {
            return;
        }

        _shownFrame = Native.GetFrameCount();

        Native.BeginTextCommandDisplayHelp(label);
        Native.EndTextCommandDisplayHelp(0, false, true, -1);
    }
}

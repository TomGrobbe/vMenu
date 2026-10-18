using CitizenFX.Core.Native;

namespace vMenu.Enhanced.Menus.Vehicles.Aircraft;

internal static class AircraftHelp
{
    private static int _shownFrame = -1;

    public static bool CanShow => _shownFrame != API.GetFrameCount() && !API.IsHelpMessageBeingDisplayed();

    public static void Show(string label)
    {
        if (!API.DoesTextLabelExist(label))
        {
            return;
        }

        _shownFrame = API.GetFrameCount();

        API.BeginTextCommandDisplayHelp(label);
        API.EndTextCommandDisplayHelp(0, false, true, -1);
    }
}

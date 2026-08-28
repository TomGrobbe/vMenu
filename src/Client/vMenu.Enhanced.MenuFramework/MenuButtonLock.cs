using CitizenFX.FiveM.Client;

using MenuAPI;

namespace vMenu.Enhanced.MenuFramework;

// Two of these screens can overlap: the second opens inside the grace delay of the first, while
// MenuController.DisableMenuButtons is still held by us and still true. Reading that flag on the way
// in therefore cannot tell whether we own it or somebody else does, so ownership is counted here
// instead. Without this the second screen hands the buttons back to nobody and the menu locks up.
public static class MenuButtonLock
{
    // The key or click that closed the screen is still held when focus returns to the game, and
    // MenuAPI selects on release: without this grace the row that opened it opens it again.
    private const int GraceMs = 300;

    private static int _holders;

    private static bool _owned;

    public static void Take()
    {
        if (_holders == 0 && !MenuController.DisableMenuButtons)
        {
            _owned = true;
        }

        _holders++;

        MenuController.DisableMenuButtons = true;
    }

    public static void Release() => _ = ReleaseAsync();

    private static async Task ReleaseAsync()
    {
        await API.Delay(GraceMs);

        if (--_holders > 0 || !_owned)
        {
            return;
        }

        _owned = false;

        MenuController.DisableMenuButtons = false;
    }
}

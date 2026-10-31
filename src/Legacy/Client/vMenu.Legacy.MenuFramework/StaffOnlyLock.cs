using MenuAPI;

using vMenu.Enhanced.Permissions;

namespace vMenu.Enhanced.MenuFramework;

// DontOpenAnyMenu rather than a gate on the root menu, because the toggle key opens the main menu without
// asking MenuArbiter or the registry, and plugin menus have no gate of their own.
public static class StaffOnlyLock
{
    public static void Initialize()
    {
        StaffOnlyMode.Changed += Apply;

        Apply();
    }

    private static void Apply()
    {
        var locked = StaffOnlyMode.Active;

        if (locked)
        {
            MenuController.CloseAllMenus();
        }

        MenuController.DontOpenAnyMenu = locked;
    }
}

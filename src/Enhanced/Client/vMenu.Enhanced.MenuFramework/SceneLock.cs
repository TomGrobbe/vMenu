using MenuAPI;

namespace vMenu.Enhanced.MenuFramework;

public static class SceneLock
{
    public static bool IsActive { get; private set; }

    public static void Take()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;

        MenuController.CloseAllMenus();
        MenuButtonLock.Take();
    }

    public static void Release()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;

        MenuButtonLock.Release();
    }
}

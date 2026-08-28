using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Admin;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Admin;

public static class ClearArea
{
    private static bool _registered;

    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnNetEvent(AdminEvents.ClearArea, new Action<float, float, float, float>(OnClearArea), false);
    }

    public static async Task RequestAsync()
    {
        var result = await ServerActions.InvokeAsync(ActionIds.Admin.ClearArea);

        if (result.Status == ActionStatus.Ok)
        {
            Notifications.Success(MenuText.Key(Loc.Admin.ClearAreaDone));

            return;
        }

        if (result.Status == ActionStatus.RateLimited)
        {
            Notifications.Warning(MenuText.Key(Loc.Admin.ClearAreaTooFast));

            return;
        }

        Notifications.Error(MenuText.Key(
            result.Status == ActionStatus.Denied ? Loc.Admin.ClearAreaDenied : Loc.Admin.ClearAreaFailed));
    }

    private static void OnClearArea(float x, float y, float z, float radius) =>
        Native.ClearAreaOfEverything(x, y, z, radius, false, false, false, false);
}

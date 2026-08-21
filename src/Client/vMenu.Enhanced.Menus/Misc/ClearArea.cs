using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Misc;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Misc;

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

        API.OnNetEvent(MiscEvents.ClearArea, new Action<float, float, float, float>(OnClearArea), false);
    }

    public static async Task RequestAsync()
    {
        var result = await ServerActions.InvokeAsync(ActionIds.MiscSettings.ClearArea);

        if (result.Status == ActionStatus.Ok)
        {
            Notifications.Success(MenuText.Key(Loc.MiscSettings.ClearAreaDone));

            return;
        }

        if (result.Status == ActionStatus.RateLimited)
        {
            Notifications.Warning(MenuText.Key(Loc.MiscSettings.ClearAreaTooFast));

            return;
        }

        Notifications.Error(MenuText.Key(
            result.Status == ActionStatus.Denied ? Loc.MiscSettings.ClearAreaDenied : Loc.MiscSettings.ClearAreaFailed));
    }

    private static void OnClearArea(float x, float y, float z, float radius) =>
        Native.ClearAreaOfEverything(x, y, z, radius, false, false, false, false);
}

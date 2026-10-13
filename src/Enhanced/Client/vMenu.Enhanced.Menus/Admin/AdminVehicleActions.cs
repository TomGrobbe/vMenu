using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Admin;

public static class AdminVehicleActions
{
    private static bool _busy;

    public static Task DeleteEmptyAsync() => WipeAsync(ActionIds.Admin.DeleteEmptyVehicles);

    public static Task DeleteEverythingAsync() => WipeAsync(ActionIds.Admin.DeleteAllVehicles);

    private static async Task WipeAsync(string action)
    {
        if (_busy)
        {
            return;
        }

        _busy = true;

        try
        {
            var result = await ServerActions.InvokeAsync(action);

            if (result.Status != ActionStatus.Ok || result.Data.Length < 1)
            {
                AdminReport.Show(result);

                return;
            }

            Notifications.Success(MenuText.Key(
                Loc.Admin.DeleteVehiclesDone,
                ("count", MenuText.Literal(result.Data[0]))));
        }
        finally
        {
            _busy = false;
        }
    }
}

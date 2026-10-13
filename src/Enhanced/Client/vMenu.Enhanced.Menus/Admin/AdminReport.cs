using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Admin;

public static class AdminReport
{
    public static void Show(ActionResult result, MenuText player = default)
    {
        if (result.Status == ActionStatus.RateLimited && result.Data.Length > 0)
        {
            Notifications.Warning(MenuText.Key(
                Loc.Admin.TooFast,
                ("seconds", MenuText.Literal(result.Data[0]))));

            return;
        }

        Show(result.Status, player);
    }

    public static void Show(ActionStatus status, MenuText player = default)
    {
        var key = status switch
        {
            ActionStatus.Denied => Loc.Admin.Denied,
            ActionStatus.NotFound => Loc.Admin.NotFound,
            ActionStatus.NotReady => Loc.Admin.StillJoining,
            ActionStatus.TooFar => Loc.Admin.MovedAway,
            _ => Loc.Admin.Failed,
        };

        Notifications.Error(MenuText.Key(key, ("player", player)));
    }
}

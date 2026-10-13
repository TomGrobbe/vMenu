using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.JoinLeave;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Players;

public static class JoinLeaveNotifications
{
    private static bool _registered;

    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        Script.EventHandlers[JoinLeaveEvents.Joined] += new Action<string>(OnJoined);
        Script.EventHandlers[JoinLeaveEvents.Left] += new Action<string, string>(OnLeft);
    }

    private static void OnJoined(string name)
    {
        if (!UserPreferences.AreJoinLeaveNotificationsEnabled)
        {
            return;
        }

        Notifications.Info(MenuText.Key(Loc.JoinLeaveNotifications.Joined, ("player", MenuText.Literal(name))));
    }

    private static void OnLeft(string name, string reason)
    {
        if (!UserPreferences.AreJoinLeaveNotificationsEnabled)
        {
            return;
        }

        var player = MenuText.Literal(name);

        Notifications.Info(reason.Length > 0
            ? MenuText.Key(Loc.JoinLeaveNotifications.LeftWithReason, ("player", player), ("reason", MenuText.Literal(reason)))
            : MenuText.Key(Loc.JoinLeaveNotifications.Left, ("player", player)));
    }
}

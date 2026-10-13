using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Admin;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Admin;

public static class AdminPushEvents
{
    private const string On = "1";

    private static bool _registered;

    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        AdminFreeze.Initialize();
        AdminHold.Initialize();

        API.OnNetEvent(AdminEvents.Freeze, new Action<string>(OnFreeze), false);
        API.OnNetEvent(AdminEvents.Hold, new Action<string>(OnHold), false);
        API.OnNetEvent(AdminEvents.HoldEnded, new Action(OnHoldEnded), false);
        API.OnNetEvent(AdminEvents.Announce, new Action<string>(OnAnnounce), false);
    }

    private static void OnFreeze(string state)
    {
        var frozen = state == On;

        AdminFreeze.SetFrozen(frozen);

        if (frozen)
        {
            Notifications.Warning(MenuText.Key(Loc.Admin.FrozenByStaff));

            return;
        }

        Notifications.Info(MenuText.Key(Loc.Admin.UnfrozenByStaff));
    }

    private static void OnHold(string holderServerId)
    {
        if (!int.TryParse(holderServerId, out var holder))
        {
            Log.Error($"[Admin] Ignoring a hold for a server id that did not parse: {holderServerId}");

            return;
        }

        AdminHold.SetHolder(holder);

        if (holder != 0)
        {
            Notifications.Warning(MenuText.Key(Loc.Admin.GrabbedByStaff));

            return;
        }

        Notifications.Info(MenuText.Key(Loc.Admin.ReleasedByStaff));
    }

    private static void OnHoldEnded()
    {
        AdminPlayerActions.ForgetCarried();

        Notifications.Info(MenuText.Key(Loc.Admin.CarriedLeft));
    }

    private static void OnAnnounce(string text) => Announcements.Show(text);
}

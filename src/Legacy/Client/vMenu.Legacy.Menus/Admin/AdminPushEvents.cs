using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Admin;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

using vMenu.Legacy.Hosting;

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

        Script.EventHandlers[AdminEvents.Freeze] += new Action<string>(OnFreeze);
        Script.EventHandlers[AdminEvents.Hold] += new Action<string>(OnHold);
        Script.EventHandlers[AdminEvents.HoldEnded] += new Action(OnHoldEnded);
        Script.EventHandlers[AdminEvents.Announce] += new Action<string>(OnAnnounce);
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

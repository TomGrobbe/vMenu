using System.Globalization;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Players;

namespace vMenu.Enhanced.Menus.Admin;

public static class AdminPlayerActions
{
    private const string On = "1";

    private const string DefaultRoutingBucket = "0";

    private static int _carrying;

    private static bool _busy;

    public static bool IsCarrying => _carrying != 0;

    public static async Task ToggleFreezeAsync()
    {
        if (_busy || Closest() is not { } target)
        {
            return;
        }

        _busy = true;

        try
        {
            var result = await ServerActions.InvokeAsync(
                ActionIds.Admin.SetFrozen,
                Id(target.ServerId));

            if (result.Status != ActionStatus.Ok || result.Data.Length < 2)
            {
                AdminReport.Show(result, MenuText.Literal(Id(target.ServerId)));

                return;
            }

            var name = MenuText.Literal(result.Data[1]);

            Notifications.Success(MenuText.Key(
                result.Data[0] == On ? Loc.Admin.FreezeDone : Loc.Admin.UnfreezeDone,
                ("player", name)));
        }
        finally
        {
            _busy = false;
        }
    }

    public static async Task ToggleHoldAsync()
    {
        if (_busy)
        {
            return;
        }

        var target = _carrying != 0 ? _carrying : Closest()?.ServerId ?? 0;

        if (target == 0)
        {
            return;
        }

        _busy = true;

        try
        {
            var result = await ServerActions.InvokeAsync(ActionIds.Admin.SetHeld, Id(target));

            if (result.Status == ActionStatus.Refused)
            {
                Notifications.Warning(MenuText.Key(
                    Loc.Admin.GrabTaken,
                    ("player", MenuText.Literal(Id(target)))));

                return;
            }

            if (result.Status != ActionStatus.Ok || result.Data.Length < 2)
            {
                if (result.Status is ActionStatus.NotFound or ActionStatus.NotReady)
                {
                    _carrying = 0;
                }

                AdminReport.Show(result, MenuText.Literal(Id(target)));

                return;
            }

            var holding = result.Data[0] == On;
            var name = MenuText.Literal(result.Data[1]);

            _carrying = holding ? target : 0;

            Notifications.Success(MenuText.Key(
                holding ? Loc.Admin.GrabDone : Loc.Admin.ReleaseDone,
                ("player", name)));
        }
        finally
        {
            _busy = false;
        }
    }

    public static async Task RefreshEveryonesPermissionsAsync()
    {
        var result = await ServerActions.InvokeAsync(ActionIds.Admin.RefreshPermissions);

        if (result.Status != ActionStatus.Ok || result.Data.Length < 1)
        {
            AdminReport.Show(result);

            return;
        }

        Notifications.Success(MenuText.Key(
            Loc.Admin.RefreshPermissionsDone,
            ("count", MenuText.Literal(result.Data[0]))));
    }

    public static async Task ResetRoutingBucketAsync()
    {
        var result = await ServerActions.InvokeAsync(ActionIds.Admin.ResetRoutingBucket);

        if (result.Status != ActionStatus.Ok || result.Data.Length < 1)
        {
            AdminReport.Show(result);

            return;
        }

        if (result.Data[0] == DefaultRoutingBucket)
        {
            Notifications.Info(MenuText.Key(Loc.Admin.ResetRoutingBucketAlready));

            return;
        }

        Notifications.Success(MenuText.Key(
            Loc.Admin.ResetRoutingBucketDone,
            ("bucket", MenuText.Literal(result.Data[0]))));
    }

    private static RosteredPlayer? Closest()
    {
        var target = AdminTargeting.Closest();

        if (target is null)
        {
            Notifications.Warning(MenuText.Key(Loc.Admin.NobodyNearby));
        }

        return target;
    }

    private static string Id(int serverId) => serverId.ToString(CultureInfo.InvariantCulture);

    internal static void ForgetCarried() => _carrying = 0;
}

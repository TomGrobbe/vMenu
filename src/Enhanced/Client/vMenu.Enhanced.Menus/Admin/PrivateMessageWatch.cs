using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.OnlinePlayers;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using AdminPermissions = vMenu.Enhanced.Data.Permissions.Menus.Admin;

namespace vMenu.Enhanced.Menus.Admin;

public static class PrivateMessageWatch
{
    private static bool _registered;

    private static bool Watching =>
        UserDefaults.AdminSeePrivateMessages.Value && ClientPermissions.IsAllowed(AdminPermissions.SeePrivateMessages);

    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnNetEvent(PlayerEvents.MessageCopy, new Action<string, string, string>(OnCopy), false);

        ClientPermissions.PermissionsChanged += Report;

        Report();
    }

    public static void SetWatching(bool watching)
    {
        UserDefaults.AdminSeePrivateMessages.Value = watching;

        Report();
    }

    private static void Report() => API.EmitServer(PlayerEvents.ReportWatchingMessages, Watching);

    private static void OnCopy(string sender, string target, string message)
    {
        if (!Watching)
        {
            return;
        }

        Notifications.Info(
            MenuText.Key(
                Loc.Admin.PrivateMessageCopy,
                ("sender", MenuText.Literal(sender)),
                ("target", MenuText.Literal(target)),
                ("message", MenuText.Literal(message))),
            PlayerPushEvents.MessageDurationMs);
    }
}

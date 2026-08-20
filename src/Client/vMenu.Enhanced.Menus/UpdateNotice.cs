using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Updates;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus;

/// <summary>
/// Tells this player, if they are staff, that a newer vMenu Enhanced is out.
/// </summary>
/// <remarks>
/// The server decides who hears about it, so there is no permission check here. Everything this
/// receives was already gated behind <c>vMenu.Enhanced.Staff</c> on the way out.
/// </remarks>
public static class UpdateNotice
{
    private static bool _registered;

    private static string? _shown;

    /// <summary>Call before permissions arrive, so an answer is never dropped.</summary>
    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnNetEvent(UpdateEvents.Available, new Action<string, string>(OnAvailable), false);
    }

    /// <summary>Call once this client has its permissions.</summary>
    public static void Request() => API.EmitServer(UpdateEvents.Request);

    // The url is taken and not shown yet. It is here because adding a parameter to a net event after
    // people have wired to it is not a change worth making twice, and the About menu is the obvious
    // next home for it.
    private static void OnAvailable(string version, string url)
    {
        // The server dedupes per session too. This catches a check finding the same version twice
        // for a player who asked in between.
        if (string.Equals(_shown, version, StringComparison.Ordinal))
        {
            return;
        }

        _shown = version;

        Log.Debug($"[Updates] v{version} is available: {url}");

        // Deferred, because on join this arrives while the player is still on the loading screen. At
        // runtime the wait is skipped and it shows straight away.
        _ = Notifications.ShowWhenVisibleAsync(
            NotificationStyle.Info,
            MenuText.Key(Loc.Updates.Available, ("version", MenuText.Literal(version))));
    }
}

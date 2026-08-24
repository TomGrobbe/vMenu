using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration.Settings;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using TeleportMenuPermissions = vMenu.Enhanced.Data.Permissions.Menus.TeleportMenu;

namespace vMenu.Enhanced.Menus.Teleport;

// Registered once and never unregistered, unlike the chat commands, because a FiveM key mapping has
// to exist for the player to be able to rebind it under Settings. Taking it away when a permission is
// missing would take the binding out of their settings with it, so the key always exists and answers
// for itself when pressed.
public static class TeleportKeyBinding
{
    // The key does nothing. What a player who has never chosen has.
    public const int Disabled = 0;

    public const int ToWaypoint = 1;

    public const int ToCoords = 2;

    private const string Command = "vmenu:teleport";

    // Used when a server owner blanks the convar rather than leaving it alone.
    private const string FallbackKey = "F10";

    private static bool _registered;

    // A teleport fades the screen and moves the player, so only one runs at a time.
    private static bool _running;

    // Call after ClientConfig.Initialize, whose convar names the key.
    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        var key = ClientConfig.Value(KeyBindings.TeleportKey);

        if (string.IsNullOrWhiteSpace(key))
        {
            key = FallbackKey;
        }

        SharedAPI.Commands.RegisterCommand(Command, false, new Action(Run));
        Native.RegisterKeyMapping(Command, "vMenu: Teleport", "keyboard", key);
    }

    // A keybind handler does not run on the game thread, and a native asked about the world from one
    // answers as though nothing is there: IsWaypointActive says there is no waypoint even when the
    // player is looking at theirs. Everything below touches the game, so none of it runs here.
    private static void Run() => SharedAPI.RunOnMainThread(Dispatch);

    // A command handler cannot await, so this is the fire and forget boundary.
    private static async void Dispatch()
    {
        if (_running)
        {
            return;
        }

        var action = UserDefaults.TeleportKeyAction.Value;

        // Silent, because this is the player's own choice rather than something the server refused.
        if (action is not (ToWaypoint or ToCoords))
        {
            return;
        }

        // Checked here rather than by refusing to register the key, so the player keeps their choice and
        // their binding when they move between servers that allow different things.
        if (!ClientPermissions.IsAllowed(action == ToWaypoint
            ? TeleportMenuPermissions.Waypoint
            : TeleportMenuPermissions.Coords))
        {
            Notifications.Error(MenuText.Key(Loc.TeleportMenu.KeyActionDenied));

            return;
        }

        _running = true;

        try
        {
            await (action == ToWaypoint
                ? TeleportTargets.ToWaypointAsync()
                : TeleportTargets.ToTypedCoordsAsync());
        }
        catch (Exception exception)
        {
            Log.Error($"[Teleport] The teleport key threw: {exception}");
        }
        finally
        {
            _running = false;
        }
    }
}

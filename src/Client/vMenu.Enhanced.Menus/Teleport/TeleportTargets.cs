using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Client.Entities;
using CitizenFX.FiveM.Client.Extensions;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Players;

namespace vMenu.Enhanced.Menus.Teleport;

// The two teleports a player asks for directly, rather than picking off a list. Out here rather than
// on the menu, because the teleport key runs the same two and must not have to reach into a menu
// definition to do it.
internal static class TeleportTargets
{
    private const int TextLength = 50;

    private const int WaypointBlipType = 4;

    // Long enough for the teleport's own fade, short enough not to hang on a stuck screen.
    private const int FadeWaitMs = 2000;

    private static readonly char[] CoordSeparators = [',', ' '];

    public static async Task ToWaypointAsync()
    {
        if (!Native.IsWaypointActive())
        {
            Notifications.Error(MenuText.Key(Loc.TeleportMenu.NoWaypoint));

            return;
        }

        if (WaypointBlip() is not { } blip)
        {
            Notifications.Error(MenuText.Key(Loc.TeleportMenu.WaypointInvalid));

            return;
        }

        // Read before the trip, because the blip is gone by the time it is over.
        var target = blip.Position;

        // A waypoint says where on the map, never how high up, so the height has to be looked up.
        if (await PlayerTeleport.ToGroundAsync(target.X, target.Y))
        {
            return;
        }

        Notifications.Error(MenuText.Key(Loc.TeleportMenu.GroundNotFound));

        // The game clears a waypoint once the player reaches it, and moving up the column at its coordinates
        // counts as reaching it. Nothing was found, so the waypoint they set is worth having again.
        await WaitForFadeInAsync();

        // Only now: setting it while still standing at those coordinates, whatever the height, has the game
        // clear it straight back off again.
        Native.SetNewWaypoint(target.X, target.Y);
    }

    public static async Task ToTypedCoordsAsync()
    {
        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.TeleportMenu.CoordsPrompt),
            TextLength,
            "0, 0, 0");

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        if (!TryParseCoords(typed, out var position))
        {
            Notifications.Error(MenuText.Key(Loc.TeleportMenu.CoordsInvalid));

            return;
        }

        await PlayerTeleport.ToCoordsAsync(position);
    }

    // Waits out the fade the teleport back runs, so the player is really somewhere else.
    private static async Task WaitForFadeInAsync()
    {
        var started = Native.GetGameTimer();

        while (!Native.IsScreenFadedIn() && Native.GetGameTimer() - started < FadeWaitMs)
        {
            await API.Delay(0);
        }
    }

    private static bool TryParseCoords(string typed, out Vector3 position)
    {
        position = default;

        var parts = typed.Split(CoordSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length != 3
            || !TryParseCoord(parts[0], out var x)
            || !TryParseCoord(parts[1], out var y)
            || !TryParseCoord(parts[2], out var z))
        {
            return false;
        }

        position = new Vector3(x, y, z);

        return true;
    }

    // Invariant, because these are pasted from a map or a script, never typed in the player's locale.
    private static bool TryParseCoord(string part, out float value) =>
        float.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

    private static Blip? WaypointBlip()
    {
        for (int it = Native.GetBlipInfoIdIterator(), blip = Native.GetFirstBlipInfoId(it);
             Native.DoesBlipExist(blip);
             blip = Native.GetNextBlipInfoId(it))
        {
            if (Native.GetBlipInfoIdType(blip) == WaypointBlipType)
            {
                return new Blip(blip);
            }
        }

        return null;
    }
}

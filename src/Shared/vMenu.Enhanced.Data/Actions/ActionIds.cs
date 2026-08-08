namespace vMenu.Enhanced.Data.Actions;

/// <summary>
/// Every action a client may ask the server to run. Nested per area to mirror
/// <c>Permissions.Menus</c>.
/// </summary>
public static class ActionIds
{
    public static class TeleportMenu
    {
        public const string TeleportCategories = "TeleportMenu.TeleportCategories";
    }
    public static class VehicleOptions
    {
        public const string DeleteVehicle = "VehicleOptions.DeleteVehicle";
    }

    public static class WeatherOptions
    {
        /// <summary>Takes a weather type, or <c>dynamic</c> to hand it back to the schedule.</summary>
        public const string SetWeather = "WeatherOptions.SetWeather";
    }

    public static class OnlinePlayers
    {
        /// <summary>
        /// Takes a search query, or an empty string for everybody. Answers with one
        /// <see cref="Data.OnlinePlayers.PlayerRow"/> per matching player.
        /// </summary>
        // Searching happens here rather than on the client because it matches against identifiers,
        // and those are nobody's business but the server's.
        public const string GetList = "OnlinePlayers.GetList";

        /// <summary>Takes a server id, answers with x, y and z.</summary>
        // Two ids for one answer, because the dispatcher checks a permission per action id and being
        // allowed to teleport to somebody is not the same as being allowed to point at them.
        public const string GetCoordsForTeleport = "OnlinePlayers.GetCoordsForTeleport";

        /// <inheritdoc cref="GetCoordsForTeleport"/>
        public const string GetCoordsForWaypoint = "OnlinePlayers.GetCoordsForWaypoint";

        /// <summary>Takes a server id and a reason.</summary>
        public const string Kick = "OnlinePlayers.Kick";

        /// <summary>Takes a server id.</summary>
        public const string Kill = "OnlinePlayers.Kill";

        /// <summary>Takes a server id. Brings that player to whoever asked.</summary>
        public const string Summon = "OnlinePlayers.Summon";

        /// <summary>Takes a server id and the message.</summary>
        public const string SendMessage = "OnlinePlayers.SendMessage";

        /// <summary>
        /// Takes a server id, answers with one identifier per entry.
        /// </summary>
        // The only thing that ever sends identifiers to a client, and it is gated on a permission of
        // its own. The player list itself never carries them.
        public const string GetIdentifiers = "OnlinePlayers.GetIdentifiers";
    }

    public static class TimeOptions
    {
        /// <summary>Takes in-game seconds to offset the clock by, or <see cref="RealTime"/> to reset.</summary>
        public const string SetTime = "TimeOptions.SetTime";

        /// <summary>Asks the server for whatever offset lands the clock back on real time.</summary>
        // A word rather than a number, because only the server knows the clock speed and the exact
        // moment the reset lands, and both of those decide what the offset has to be.
        public const string RealTime = "realtime";
    }
}

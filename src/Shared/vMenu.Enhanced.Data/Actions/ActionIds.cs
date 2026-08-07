namespace vMenu.Enhanced.Data.Actions;

/// <summary>
/// Every action a client may ask the server to run. Nested per area to mirror
/// <c>Permissions.Menus</c>.
/// </summary>
public static class ActionIds
{
    public static class VehicleOptions
    {
        public const string DeleteVehicle = "VehicleOptions.DeleteVehicle";
    }

    public static class WeatherOptions
    {
        /// <summary>Takes a weather type, or <c>dynamic</c> to hand it back to the schedule.</summary>
        public const string SetWeather = "WeatherOptions.SetWeather";
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

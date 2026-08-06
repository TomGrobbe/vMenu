using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.World;

using TimeOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.TimeOptions;
using TimeOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.TimeOptions;
using WeatherOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeatherOptions;
using WeatherOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.WeatherOptions;

namespace vMenu.Enhanced.Actions.Server.Handlers;

/// <summary>Weather and time overrides, which apply to everybody on the server.</summary>
public static class WorldActions
{
    public static void Register()
    {
        ActionRegistry.Register(
            ActionIds.WeatherOptions.SetWeather,
            WeatherOptionsPermissions.SetWeather,
            SetWeather);

        ActionRegistry.Register(
            ActionIds.TimeOptions.SetTime,
            TimeOptionsPermissions.SetTime,
            SetTime);
    }

    private static ActionResponse SetWeather(Player source, string[] args)
    {
        // Refused rather than denied: the player has the permission, the owner has the feature off.
        if (!ServerConfig.Value(WeatherOptionsSettings.Enabled))
        {
            return ActionResponse.Refused();
        }

        if (args.Length < 1)
        {
            return ActionResponse.InvalidRequest();
        }

        if (string.Equals(args[0], WorldStateConvars.Dynamic, StringComparison.OrdinalIgnoreCase))
        {
            ServerState.SetWeather(null);

            API.Log.Info($"[State] {source} handed the weather back to the schedule.");

            return ActionResponse.Ok();
        }

        if (!WeatherTypes.TryParse(args[0], out var type))
        {
            return ActionResponse.InvalidRequest();
        }

        ServerState.SetWeather(type);

        API.Log.Info($"[State] {source} forced the weather to {WeatherTypes.NameOf(type)}.");

        return ActionResponse.Ok();
    }

    private static ActionResponse SetTime(Player source, string[] args)
    {
        if (!ServerConfig.Value(TimeOptionsSettings.Enabled))
        {
            return ActionResponse.Refused();
        }

        if (args.Length < 1
            || !int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
        {
            return ActionResponse.InvalidRequest();
        }

        ServerState.SetTimeOffset(seconds);

        API.Log.Info($"[State] {source} set the clock offset to {ServerState.TimeOffsetSeconds}s.");

        return ActionResponse.Ok();
    }
}

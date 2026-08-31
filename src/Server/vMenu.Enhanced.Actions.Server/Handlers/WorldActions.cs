using System.Globalization;

using CitizenFX.FiveM.Server.Entities;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Webhooks.Server;

using TimeOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.TimeOptions;
using TimeOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.TimeOptions;
using WeatherOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeatherOptions;
using WeatherOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.WeatherOptions;

namespace vMenu.Enhanced.Actions.Server.Handlers;

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

        ActionRegistry.Register(
            ActionIds.WeatherOptions.SetBlackout,
            WeatherOptionsPermissions.Blackout,
            SetBlackout);

        ActionRegistry.Register(
            ActionIds.WeatherOptions.SetSnow,
            WeatherOptionsPermissions.Snow,
            SetSnow);

        ActionRegistry.Register(
            ActionIds.TimeOptions.SetFrozen,
            TimeOptionsPermissions.FreezeTime,
            SetFrozen);
    }

    // No Enabled check, unlike the rest: blackout is street lighting, not weather.
    private static ActionResponse SetBlackout(Player source, string[] args)
    {
        if (args.Length < 1 || !BlackoutModes.TryParse(args[0], out var mode))
        {
            return ActionResponse.InvalidRequest();
        }

        ServerState.SetBlackout(mode);

        Log.Debug($"[State] {source} set the blackout to {BlackoutModes.NameOf(mode)}.");

        Announce(source, "changed the blackout", ("blackout", BlackoutModes.NameOf(mode)));

        return ActionResponse.Ok();
    }

    private static ActionResponse SetSnow(Player source, string[] args)
    {
        if (!ServerConfig.Value(WeatherOptionsSettings.Enabled))
        {
            return ActionResponse.Refused();
        }

        if (args.Length < 1 || !SnowModes.TryParse(args[0], out var mode))
        {
            return ActionResponse.InvalidRequest();
        }

        ServerState.SetSnow(mode);

        Log.Debug($"[State] {source} set the snow effects to {SnowModes.NameOf(mode)}.");

        Announce(source, "changed the snow effects", ("snow", SnowModes.NameOf(mode)));

        return ActionResponse.Ok();
    }

    private static ActionResponse SetFrozen(Player source, string[] args)
    {
        if (!ServerConfig.Value(TimeOptionsSettings.Enabled))
        {
            return ActionResponse.Refused();
        }

        if (args.Length < 1 || !bool.TryParse(args[0], out var frozen))
        {
            return ActionResponse.InvalidRequest();
        }

        ServerState.SetTimeFrozen(frozen);

        Log.Debug($"[State] {source} {(frozen ? "froze" : "unfroze")} the clock.");

        Announce(source, frozen ? "froze the clock" : "unfroze the clock");

        return ActionResponse.Ok();
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

            Log.Debug($"[State] {source} handed the weather back to the schedule.");

            Announce(source, "handed the weather back to the schedule");

            return ActionResponse.Ok();
        }

        if (!WeatherTypes.TryParse(args[0], out var type))
        {
            return ActionResponse.InvalidRequest();
        }

        ServerState.SetWeather(type);

        Log.Debug($"[State] {source} forced the weather to {WeatherTypes.NameOf(type)}.");

        Announce(source, "changed the weather", ("weather", WeatherTypes.NameOf(type)));

        return ActionResponse.Ok();
    }

    private static void Announce(Player source, string what, params (string Key, string Value)[] data) =>
        WebhookLog.Event(what + ".", WebhookActor.For(source), null, data);

    private static ActionResponse SetTime(Player source, string[] args)
    {
        if (!ServerConfig.Value(TimeOptionsSettings.Enabled))
        {
            return ActionResponse.Refused();
        }

        if (args.Length < 1)
        {
            return ActionResponse.InvalidRequest();
        }

        // Worked out on the server, not sent as a number, so the offset matches the moment it lands and the
        // speed the server is actually running at.
        if (string.Equals(args[0], ActionIds.TimeOptions.RealTime, StringComparison.OrdinalIgnoreCase))
        {
            ServerState.SetTimeOffsetRunning(ServerClock.RealTimeOffset());

            Log.Debug(
                $"[State] {source} put the clock back on the server's own time, " +
                $"offset {ServerState.TimeOffsetSeconds}s.");

            Announce(source, "put the clock back on the server's own time");

            return ActionResponse.Ok();
        }

        if (!int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
        {
            return ActionResponse.InvalidRequest();
        }

        ServerState.SetTimeOffset(seconds);

        Log.Debug($"[State] {source} set the clock offset to {ServerState.TimeOffsetSeconds}s.");

        Announce(source, "changed the time", ("offset", ServerState.TimeOffsetSeconds + "s"));

        return ActionResponse.Ok();
    }
}

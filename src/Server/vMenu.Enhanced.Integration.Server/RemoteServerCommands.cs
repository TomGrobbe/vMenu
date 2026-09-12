using System.Globalization;
using System.Text.Json;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Actions.Server.Handlers;
using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.Serialization.Server;

using TimeOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.TimeOptions;
using WeatherOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.WeatherOptions;

namespace vMenu.Enhanced.Integration.Server;

internal static class RemoteServerCommands
{
    private const int ForecastCount = 6;

    public static bool IsServerAction(string action) => action switch
    {
        "announce" or "get-world" or "set-weather" or "set-time"
            or "set-blackout" or "set-snow" or "set-freeze" or "get-config" or "set-convar" => true,
        _ => false,
    };

    private static bool IsRead(string action) => action is "get-world" or "get-config";

    public static IntegrationCommands.CommandReply Run(string action, JsonElement parameters)
    {
        if (!IsRead(action) && !ServerConfig.Value(vMenu.Enhanced.Data.Configuration.Settings.Integration.AllowActions))
        {
            return new IntegrationCommands.CommandReply(403, IntegrationJson.Fail("disabled"));
        }

        return action switch
        {
            "announce" => Announce(parameters),
            "get-world" => GetWorld(),
            "set-weather" => SetWeather(parameters),
            "set-time" => SetTime(parameters),
            "set-blackout" => SetBlackout(parameters),
            "set-snow" => SetSnow(parameters),
            "set-freeze" => SetFreeze(parameters),
            "get-config" => GetConfig(),
            "set-convar" => SetConvar(parameters),
            _ => new IntegrationCommands.CommandReply(400, IntegrationJson.Fail("unknown-action")),
        };
    }

    private static IntegrationCommands.CommandReply Announce(JsonElement parameters)
    {
        var text = IntegrationJson.ReadString(parameters, "text");
        if (string.IsNullOrWhiteSpace(text))
        {
            return new IntegrationCommands.CommandReply(400, IntegrationJson.Fail("bad-request"));
        }

        var reached = AdminActions.Broadcast(text.Trim());

        return new IntegrationCommands.CommandReply(
            200, $"{{\"ok\":true,\"reached\":{reached.ToString(CultureInfo.InvariantCulture)}}}");
    }

    private static IntegrationCommands.CommandReply GetWorld()
    {
        var world = ServerJson.Serialize(WorldSnapshot.Capture(ForecastCount));

        return new IntegrationCommands.CommandReply(200, "{\"ok\":true,\"world\":" + world + "}");
    }

    private static IntegrationCommands.CommandReply SetWeather(JsonElement parameters)
    {
        if (!ServerConfig.Value(WeatherOptionsSettings.Enabled))
        {
            return new IntegrationCommands.CommandReply(403, IntegrationJson.Fail("refused"));
        }

        var weather = IntegrationJson.ReadString(parameters, "weather");
        if (string.IsNullOrWhiteSpace(weather))
        {
            return new IntegrationCommands.CommandReply(400, IntegrationJson.Fail("bad-request"));
        }

        if (string.Equals(weather, WorldStateConvars.Dynamic, StringComparison.OrdinalIgnoreCase))
        {
            ServerState.SetWeather(null);

            return new IntegrationCommands.CommandReply(200, IntegrationJson.Ok);
        }

        if (!WeatherTypes.TryParse(weather, out var type))
        {
            return new IntegrationCommands.CommandReply(400, IntegrationJson.Fail("bad-request"));
        }

        ServerState.SetWeather(type);

        return new IntegrationCommands.CommandReply(200, IntegrationJson.Ok);
    }

    private static IntegrationCommands.CommandReply SetTime(JsonElement parameters)
    {
        if (!ServerConfig.Value(TimeOptionsSettings.Enabled))
        {
            return new IntegrationCommands.CommandReply(403, IntegrationJson.Fail("refused"));
        }

        var mode = IntegrationJson.ReadString(parameters, "mode");
        if (string.Equals(mode, "realtime", StringComparison.OrdinalIgnoreCase))
        {
            ServerState.SetTimeOffsetRunning(ServerClock.RealTimeOffset());

            return new IntegrationCommands.CommandReply(200, IntegrationJson.Ok);
        }

        if (!IntegrationJson.TryReadInt(parameters, "seconds", out var seconds))
        {
            return new IntegrationCommands.CommandReply(400, IntegrationJson.Fail("bad-request"));
        }

        ServerState.SetTimeOffset(seconds);

        return new IntegrationCommands.CommandReply(200, IntegrationJson.Ok);
    }

    private static IntegrationCommands.CommandReply SetBlackout(JsonElement parameters)
    {
        var mode = IntegrationJson.ReadString(parameters, "mode");
        if (!BlackoutModes.TryParse(mode, out var parsed))
        {
            return new IntegrationCommands.CommandReply(400, IntegrationJson.Fail("bad-request"));
        }

        ServerState.SetBlackout(parsed);

        return new IntegrationCommands.CommandReply(200, IntegrationJson.Ok);
    }

    private static IntegrationCommands.CommandReply SetSnow(JsonElement parameters)
    {
        if (!ServerConfig.Value(WeatherOptionsSettings.Enabled))
        {
            return new IntegrationCommands.CommandReply(403, IntegrationJson.Fail("refused"));
        }

        var mode = IntegrationJson.ReadString(parameters, "mode");
        if (!SnowModes.TryParse(mode, out var parsed))
        {
            return new IntegrationCommands.CommandReply(400, IntegrationJson.Fail("bad-request"));
        }

        ServerState.SetSnow(parsed);

        return new IntegrationCommands.CommandReply(200, IntegrationJson.Ok);
    }

    private static IntegrationCommands.CommandReply SetFreeze(JsonElement parameters)
    {
        if (!ServerConfig.Value(TimeOptionsSettings.Enabled))
        {
            return new IntegrationCommands.CommandReply(403, IntegrationJson.Fail("refused"));
        }

        if (!IntegrationJson.TryReadBool(parameters, "frozen", out var frozen))
        {
            return new IntegrationCommands.CommandReply(400, IntegrationJson.Fail("bad-request"));
        }

        ServerState.SetTimeFrozen(frozen);

        return new IntegrationCommands.CommandReply(200, IntegrationJson.Ok);
    }

    private static IntegrationCommands.CommandReply GetConfig()
    {
        var response = new ConfigResponse
        {
            Sections = [.. ConfigCatalog.Sections.Select(section => new ConfigSectionDto
            {
                Title = section.Title,
                Settings = [.. section.Settings
                    .Where(setting => !setting.ServerOnly)
                    .Select(setting => new ConfigSettingDto
                    {
                        Name = setting.Name,
                        Type = setting.TypeName,
                        Description = setting.Description,
                        Default = setting.DefaultValue,
                        Value = ServerConfig.GetString(setting.Name),
                    })],
            })],
        };

        var json = ServerJson.Serialize(response);

        return new IntegrationCommands.CommandReply(200, "{\"ok\":true,\"config\":" + json + "}");
    }

    private static IntegrationCommands.CommandReply SetConvar(JsonElement parameters)
    {
        var name = IntegrationJson.ReadString(parameters, "name");
        var value = IntegrationJson.ReadString(parameters, "value");
        if (string.IsNullOrWhiteSpace(name) || value is null)
        {
            return new IntegrationCommands.CommandReply(400, IntegrationJson.Fail("bad-request"));
        }

        // Allow-list of owner-facing catalog convars only, so integration secrets cannot be set.
        var known = ConfigCatalog.All.FirstOrDefault(
            setting => !setting.ServerOnly && string.Equals(setting.Name, name, StringComparison.Ordinal));
        if (known is null)
        {
            return new IntegrationCommands.CommandReply(400, IntegrationJson.Fail("not-allowed"));
        }

        // The value goes into a command string, so strip quote/backslash/semicolon/newline to stop injection.
        var safe = Sanitise(value);

        Native.ExecuteCommand($"set {known.Name} \"{safe}\"");

        return new IntegrationCommands.CommandReply(200, IntegrationJson.Ok);
    }

    private static string Sanitise(string value)
    {
        var cleaned = value.Replace("\\", string.Empty)
            .Replace("\"", string.Empty)
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty)
            .Replace(";", string.Empty);

        return cleaned.Trim();
    }
}

// Plain DTOs so ServerJson serialises them without building a reflection converter at request time.
internal sealed class ConfigResponse
{
    public required IReadOnlyList<ConfigSectionDto> Sections { get; init; }
}

internal sealed class ConfigSectionDto
{
    public required string Title { get; init; }

    public required IReadOnlyList<ConfigSettingDto> Settings { get; init; }
}

internal sealed class ConfigSettingDto
{
    public required string Name { get; init; }

    public required string Type { get; init; }

    public required string Description { get; init; }

    public required string Default { get; init; }

    public required string? Value { get; init; }
}

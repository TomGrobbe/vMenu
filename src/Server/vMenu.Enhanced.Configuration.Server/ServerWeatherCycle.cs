using System.Globalization;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization.Server;

using WeatherOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.WeatherOptions;

namespace vMenu.Enhanced.Configuration.Server;

// Clients cannot read the config folder, so the server reads the custom cycle and hands it to them.
public static class ServerWeatherCycle
{
    private const string ConfigFile = "config/weather-cycle.json";

    public static void Initialize()
    {
        Load();

        ServerConfig.AddEventListenerFor(
            [WeatherOptionsSettings.Enabled, WeatherOptionsSettings.WeatherCycle],
            Load);
    }

    private static void Load()
    {
        var wanted = Wanted();
        var custom = wanted == WeatherCycleType.Custom && ServerConfig.Value(WeatherOptionsSettings.Enabled)
            ? ReadFile()
            : null;

        WeatherCycle.Use(wanted, custom);

        Native.SetConvarReplicated(
            WorldStateConvars.CustomCycle,
            custom is null ? string.Empty : ServerJson.Serialize(custom.Cleaned));

        if (custom is not null)
        {
            Log.Info(
                $"[Weather] Using the custom weather cycle from {ConfigFile}: {custom.Entries.Length} entries " +
                $"over {custom.LengthGameHours.ToString("0.##", CultureInfo.InvariantCulture)} in-game hours.");
        }
    }

    private static WeatherCycleType Wanted()
    {
        var raw = ServerConfig.Value(WeatherOptionsSettings.WeatherCycle);

        if (WeatherCycles.TryParse(raw, out var cycle) || string.IsNullOrWhiteSpace(raw))
        {
            return cycle;
        }

        Log.Warning(
            $"[Weather] {WeatherOptionsSettings.WeatherCycle.Name} is set to '{raw}', which is not default, " +
            "snowy or custom. Using default.");

        return WeatherCycleType.Default;
    }

    private static CustomCycle? ReadFile()
    {
        var contents = Native.LoadResourceFile(Native.GetCurrentResourceName(), ConfigFile);

        if (string.IsNullOrWhiteSpace(contents))
        {
            Log.Warning($"[Weather] The weather cycle is set to custom, but {ConfigFile} is missing or empty. Using default.");

            return null;
        }

        if (!ServerJson.TryDeserialize<CustomCycleFile>(contents, out var read, out var error))
        {
            Log.Warning($"[Weather] {ConfigFile} could not be read, so the default weather cycle is used: {error}");

            return null;
        }

        var custom = CustomCycle.Build(read, problem => Log.Warning($"[Weather] {ConfigFile}: {problem}"));

        if (custom is null)
        {
            Log.Warning($"[Weather] {ConfigFile} has no usable entries, so the default weather cycle is used.");
        }

        return custom;
    }
}

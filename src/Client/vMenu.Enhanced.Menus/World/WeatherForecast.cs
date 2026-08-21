using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Serialization;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using WeatherOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeatherOptions;
using WeatherOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.WeatherOptions;

namespace vMenu.Enhanced.Menus.World;

public static class WeatherForecast
{
    private const long RefreshIntervalMs = 1000;

    private const int UpcomingCount = 3;

    private const int Unknown = -1;

    private const string HideMessage = """{"type":"forecast","visible":false}""";

    private static TickHandle? _tick;

    private static bool _shown;

    private static string _painted = string.Empty;

    public static bool Enabled =>
        UserDefaults.WorldWeatherForecast.Value
        && ClientConfig.Value(WeatherOptionsSettings.Enabled)
        && ClientPermissions.IsAllowed(WeatherOptionsPermissions.Forecast);

    public static void Initialize()
    {
        _tick = TickRegistry.Register(
            "World.Forecast",
            Flush,
            TickRate.Every(RefreshIntervalMs),
            () => Enabled,
            autoStart: false);

        ClientPermissions.PermissionsChanged += Reevaluate;

        ClientConfig.AddEventListenerFor([WeatherOptionsSettings.Enabled], Reevaluate);
    }

    public static void Restore() => Reevaluate();

    public static void SetEnabled(bool enabled)
    {
        UserDefaults.WorldWeatherForecast.Value = enabled;

        Reevaluate();
    }

    private static void Reevaluate()
    {
        _tick?.Reevaluate();

        if (!Enabled)
        {
            Hide();

            return;
        }

        Flush();
    }

    private static void Flush()
    {
        if (!Hud.CanDraw)
        {
            Hide();

            return;
        }

        var message = ClientJson.Serialize(Build());

        if (message == _painted)
        {
            return;
        }

        _painted = message;
        _shown = true;

        Native.SendNuiMessage(message);
    }

    private static void Hide()
    {
        _painted = string.Empty;

        if (!_shown)
        {
            return;
        }

        _shown = false;

        Native.SendNuiMessage(HideMessage);
    }

    private static ForecastMessage Build()
    {
        var localizer = Localizer.Current;
        var forced = WorldState.WeatherOverride;
        var scheduled = WorldState.HasClock && forced is null;
        var speed = WorldState.TimeSpeed;
        var current = WorldState.Weather;
        var upcoming = new List<ForecastRow>(UpcomingCount);

        if (scheduled)
        {
            var entries = WeatherCycle.Forecast(GameClock.CycleGameHours(WorldState.UnixSeconds, speed), UpcomingCount);

            foreach (var entry in entries)
            {
                upcoming.Add(new ForecastRow
                {
                    Name = localizer.Get(Loc.World.WeatherName(entry.Type)),
                    Icon = IconOf(entry.Type),
                    InSeconds = RealSeconds(entry.GameHoursUntilStart, speed),
                    ForSeconds = RealSeconds(entry.GameHoursLong, speed),
                });
            }
        }

        var moonDays = WorldTime.MoonCycleDays;

        return new ForecastMessage
        {
            Title = localizer.Get(Loc.World.ForecastTitle),
            NowLabel = localizer.Get(Loc.World.ForecastNow),
            NextLabel = localizer.Get(Loc.World.ForecastNext),
            MoonLabel = localizer.Get(Loc.World.ForecastMoon),
            Note = WorldState.HasClock
                ? forced is null ? string.Empty : localizer.Get(Loc.World.ForecastForced)
                : localizer.Get(Loc.World.ForecastNoClock),
            CurrentName = localizer.Get(Loc.World.WeatherName(current)),
            CurrentIcon = IconOf(current),
            CurrentForSeconds = scheduled ? RealSeconds(WorldState.Schedule.GameHoursUntilNext, speed) : Unknown,
            Upcoming = upcoming.ToArray(),
            MoonName = localizer.Get(Loc.World.MoonPhaseName(MoonCycle.PhaseOf(moonDays))),
            MoonLit = (int)Math.Round(MoonCycle.Illumination(moonDays) * 100.0),
            MoonWaxing = MoonCycle.DayOfCycle(moonDays) < MoonCycle.FullMoonDay,
        };
    }

    private static int RealSeconds(double gameHours, double speed) =>
        (int)Math.Round(gameHours * GameClock.RealSecondsPerGameHourAt(speed));

    private static string IconOf(WeatherType type) => type switch
    {
        WeatherType.ExtraSunny => "sunny",
        WeatherType.Clear => "clear",
        WeatherType.Neutral => "clear",
        WeatherType.Clouds => "clouds",
        WeatherType.Overcast => "overcast",
        WeatherType.Clearing => "clearing",
        WeatherType.Rain => "rain",
        WeatherType.RainHalloween => "rain",
        WeatherType.Thunder => "thunder",
        WeatherType.Smog => "smog",
        WeatherType.Foggy => "fog",
        WeatherType.Snow => "snow",
        WeatherType.SnowLight => "snow",
        WeatherType.SnowHalloween => "snow",
        WeatherType.Xmas => "snow",
        WeatherType.Blizzard => "blizzard",
        WeatherType.Halloween => "halloween",
        _ => "clear",
    };

    private sealed class ForecastRow
    {
        public required string Name { get; init; }

        public required string Icon { get; init; }

        public required int InSeconds { get; init; }

        public required int ForSeconds { get; init; }
    }

    private sealed class ForecastMessage
    {
        public string Type { get; } = "forecast";

        public bool Visible { get; } = true;

        public required string Title { get; init; }

        public required string NowLabel { get; init; }

        public required string NextLabel { get; init; }

        public required string MoonLabel { get; init; }

        public required string Note { get; init; }

        public required string CurrentName { get; init; }

        public required string CurrentIcon { get; init; }

        public required int CurrentForSeconds { get; init; }

        public required ForecastRow[] Upcoming { get; init; }

        public required string MoonName { get; init; }

        public required int MoonLit { get; init; }

        public required bool MoonWaxing { get; init; }
    }
}

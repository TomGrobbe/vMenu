using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Misc;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Serialization;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using DisplaySettingsPermissions = vMenu.Enhanced.Data.Permissions.Menus.DisplaySettings;
using WeatherOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.WeatherOptions;

namespace vMenu.Enhanced.Menus.World;

public static class WeatherForecast
{
    private const long RefreshIntervalMs = 1000;

    private const int UpcomingCount = 3;

    public const int Full = 0;

    public const int Compact = 1;

    private const int Unknown = -1;

    private const string HideMessage = """{"type":"forecast","visible":false}""";

    private static TickHandle? _tick;

    private static bool _shown;

    private static string _painted = string.Empty;

    public static MenuGate Allowed { get; } =
        MenuGate.Setting(WeatherOptionsSettings.Enabled)
        & MenuGate.Permission(DisplaySettingsPermissions.Forecast);

    public static bool Enabled =>
        UserDefaults.DisplayWeatherForecast.Value
        && ClientConfig.Value(WeatherOptionsSettings.Enabled)
        && ClientPermissions.IsAllowed(DisplaySettingsPermissions.Forecast);

    public static bool ClockEnabled => UserDefaults.DisplayShowTime.Value;

    public static bool ClockOnlyShown => ClockEnabled && !Enabled;

    private static bool Wanted => Enabled || ClockEnabled;

    public static void Initialize()
    {
        _tick = TickRegistry.Register(
            "World.Forecast",
            Flush,
            TickRate.Every(RefreshIntervalMs),
            () => Wanted,
            autoStart: false);

        ClientPermissions.PermissionsChanged += Reevaluate;

        ClientConfig.AddEventListenerFor([WeatherOptionsSettings.Enabled], Reevaluate);
    }

    public static void Restore() => Reevaluate();

    public static int Style =>
        UserDefaults.DisplayWeatherForecastStyle.Value == Compact ? Compact : Full;

    public static bool CompactShown => Enabled && Style == Compact;

    public static void SetEnabled(bool enabled)
    {
        UserDefaults.DisplayWeatherForecast.Value = enabled;

        Reevaluate();
    }

    public static void SetStyle(int style)
    {
        UserDefaults.DisplayWeatherForecastStyle.Value = style;

        Reevaluate();
    }

    public static void SetClockEnabled(bool enabled)
    {
        UserDefaults.DisplayShowTime.Value = enabled;

        Reevaluate();
    }

    private static void Reevaluate()
    {
        _tick?.Reevaluate();

        LocationDisplay.RefreshAnchor();

        if (!Wanted)
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
        if (ClockOnlyShown)
        {
            return ClockOnly();
        }

        var localizer = Localizer.Current;
        var forced = WorldState.WeatherOverride;
        var scheduled = WorldState.HasClock && forced is null;
        var speed = WorldState.TimeSpeed;
        var current = WorldState.Weather;
        var compact = Style == Compact;
        var wanted = compact ? 1 : UpcomingCount;
        var upcoming = new List<ForecastRow>(wanted);

        if (scheduled)
        {
            var entries = WeatherCycle.Forecast(GameClock.CycleGameHours(WorldState.UnixSeconds, speed), wanted);

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
            ShowForecast = true,
            ShowTime = ClockEnabled,
            Compact = compact,
            Time = ClockText(),
            Title = localizer.Get(Loc.DisplaySettings.ForecastTitle),
            NowLabel = localizer.Get(Loc.DisplaySettings.ForecastNow),
            NextLabel = localizer.Get(Loc.DisplaySettings.ForecastNext),
            MoonLabel = localizer.Get(Loc.DisplaySettings.ForecastMoon),
            Note = WorldState.HasClock
                ? forced is null ? string.Empty : localizer.Get(Loc.DisplaySettings.ForecastForced)
                : localizer.Get(Loc.DisplaySettings.ForecastNoClock),
            CurrentName = localizer.Get(Loc.World.WeatherName(current)),
            CurrentIcon = IconOf(current),
            CurrentForSeconds = scheduled ? RealSeconds(WorldState.Schedule.GameHoursUntilNext, speed) : Unknown,
            Upcoming = upcoming.ToArray(),
            MoonName = localizer.Get(Loc.World.MoonPhaseName(MoonCycle.PhaseOf(moonDays))),
            MoonLit = (int)Math.Round(MoonCycle.Illumination(moonDays) * 100.0),
            MoonWaxing = MoonCycle.DayOfCycle(moonDays) < MoonCycle.FullMoonDay,
        };
    }

    private static ForecastMessage ClockOnly() => new()
    {
        ShowForecast = false,
        ShowTime = true,
        Compact = true,
        Time = ClockText(),
        Title = string.Empty,
        NowLabel = string.Empty,
        NextLabel = string.Empty,
        MoonLabel = string.Empty,
        Note = string.Empty,
        CurrentName = string.Empty,
        CurrentIcon = string.Empty,
        CurrentForSeconds = Unknown,
        Upcoming = [],
        MoonName = string.Empty,
        MoonLit = 0,
        MoonWaxing = false,
    };

    private static string ClockText() =>
        TimeText.Format((Native.GetClockHours() * 3600) + (Native.GetClockMinutes() * 60));

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

        public required bool ShowForecast { get; init; }

        public required bool ShowTime { get; init; }

        public required bool Compact { get; init; }

        public required string Time { get; init; }

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

using vMenu.Enhanced.Data.World;

namespace vMenu.Enhanced.MenuFramework.Localization;

public static partial class Loc
{
    public static class World
    {
        public const string Title = "world.title";

        public const string Subtitle = "world.subtitle";

        public const string LinkDescription = "world.link.desc";

        public const string Status = "world.status";

        public const string StatusWeatherForced = "world.status.weatherforced";

        public const string StatusTimeForced = "world.status.timeforced";

        public const string StatusBothForced = "world.status.bothforced";

        public const string Weather = "world.weather";

        public const string WeatherDescription = "world.weather.desc";

        public const string WeatherDynamic = "world.weather.dynamic";

        public const string SetTime = "world.settime";

        public const string SetTimeDescription = "world.settime.desc";

        public const string SetTimePrompt = "world.settime.prompt";

        public const string TimePreset = "world.timepreset";

        public const string TimePresetDescription = "world.timepreset.desc";

        public const string ResetWeather = "world.resetweather";

        public const string ResetWeatherDescription = "world.resetweather.desc";

        public const string ResetTime = "world.resettime";

        public const string ResetTimeDescription = "world.resettime.desc";

        public const string WeatherSet = "world.weather.set";

        public const string WeatherReset = "world.weather.reset";

        public const string TimeSet = "world.time.set";

        public const string TimeReset = "world.time.reset";

        public const string TimeNotUnderstood = "world.time.notunderstood";

        /// <summary>Tail of every change notification, left out when the blend is set to zero.</summary>
        public const string Transition = "world.transition";

        public const string Denied = "world.denied";

        public const string Disabled = "world.disabled";

        public const string Failed = "world.failed";

        public const string Forecast = "world.forecast";

        public const string ForecastDescription = "world.forecast.desc";

        public const string ForecastTitle = "world.forecast.title";

        public const string ForecastNow = "world.forecast.now";

        public const string ForecastNext = "world.forecast.next";

        public const string ForecastForced = "world.forecast.forced";

        public const string ForecastMoon = "world.forecast.moon";

        public const string ForecastNoClock = "world.forecast.noclock";

        public const string MoonPhaseNew = "world.moonphase.new";

        public const string MoonPhaseWaxingCrescent = "world.moonphase.waxingcrescent";

        public const string MoonPhaseFirstQuarter = "world.moonphase.firstquarter";

        public const string MoonPhaseWaxingGibbous = "world.moonphase.waxinggibbous";

        public const string MoonPhaseFull = "world.moonphase.full";

        public const string MoonPhaseWaningGibbous = "world.moonphase.waninggibbous";

        public const string MoonPhaseLastQuarter = "world.moonphase.lastquarter";

        public const string MoonPhaseWaningCrescent = "world.moonphase.waningcrescent";

        public static string MoonPhaseName(MoonPhase phase) => phase switch
        {
            MoonPhase.New => MoonPhaseNew,
            MoonPhase.WaxingCrescent => MoonPhaseWaxingCrescent,
            MoonPhase.FirstQuarter => MoonPhaseFirstQuarter,
            MoonPhase.WaxingGibbous => MoonPhaseWaxingGibbous,
            MoonPhase.Full => MoonPhaseFull,
            MoonPhase.WaningGibbous => MoonPhaseWaningGibbous,
            MoonPhase.LastQuarter => MoonPhaseLastQuarter,
            _ => MoonPhaseWaningCrescent,
        };

        public const string WeatherClear = "world.weathername.clear";

        public const string WeatherExtraSunny = "world.weathername.extrasunny";

        public const string WeatherClouds = "world.weathername.clouds";

        public const string WeatherOvercast = "world.weathername.overcast";

        public const string WeatherRain = "world.weathername.rain";

        public const string WeatherClearing = "world.weathername.clearing";

        public const string WeatherThunder = "world.weathername.thunder";

        public const string WeatherSmog = "world.weathername.smog";

        public const string WeatherFoggy = "world.weathername.foggy";

        public const string WeatherXmas = "world.weathername.xmas";

        public const string WeatherSnow = "world.weathername.snow";

        public const string WeatherSnowLight = "world.weathername.snowlight";

        public const string WeatherBlizzard = "world.weathername.blizzard";

        public const string WeatherHalloween = "world.weathername.halloween";

        public const string WeatherNeutral = "world.weathername.neutral";

        public const string WeatherRainHalloween = "world.weathername.rainhalloween";

        public const string WeatherSnowHalloween = "world.weathername.snowhalloween";

        public static string WeatherName(WeatherType type) => type switch
        {
            WeatherType.Clear => WeatherClear,
            WeatherType.ExtraSunny => WeatherExtraSunny,
            WeatherType.Clouds => WeatherClouds,
            WeatherType.Overcast => WeatherOvercast,
            WeatherType.Rain => WeatherRain,
            WeatherType.Clearing => WeatherClearing,
            WeatherType.Thunder => WeatherThunder,
            WeatherType.Smog => WeatherSmog,
            WeatherType.Foggy => WeatherFoggy,
            WeatherType.Xmas => WeatherXmas,
            WeatherType.Snow => WeatherSnow,
            WeatherType.SnowLight => WeatherSnowLight,
            WeatherType.Blizzard => WeatherBlizzard,
            WeatherType.Halloween => WeatherHalloween,
            WeatherType.Neutral => WeatherNeutral,
            WeatherType.RainHalloween => WeatherRainHalloween,
            WeatherType.SnowHalloween => WeatherSnowHalloween,
            _ => WeatherClear,
        };
    }
}

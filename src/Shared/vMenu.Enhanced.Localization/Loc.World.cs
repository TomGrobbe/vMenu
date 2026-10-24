using vMenu.Enhanced.Data.World;

namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class World
    {
        [English("Weather & Time")]
        public const string Title = "world.title";

        [English("Weather & Time")]
        public const string Subtitle = "world.subtitle";

        [English("Weather and time of day. Anything you change here applies to everyone on the server.")]
        public const string LinkDescription = "world.link.desc";

        [English("~b~{weather}~s~ · ~b~{time}~s~")]
        public const string Status = "world.status";

        [English("~y~{weather}~s~ (altered) · ~b~{time}~s~")]
        public const string StatusWeatherForced = "world.status.weatherforced";

        [English("~b~{weather}~s~ · ~y~{time}~s~ (altered)")]
        public const string StatusTimeForced = "world.status.timeforced";

        [English("~y~{weather}~s~ (altered) · ~y~{time}~s~ (altered)")]
        public const string StatusBothForced = "world.status.bothforced";

        [English(
            "Weather",
            "Pick a weather type to hold, or the first option to let it follow the server cycle. Press Enter to apply.")]
        public const string Weather = "world.weather";

        [English("Dynamic (Server Cycle)")]
        public const string WeatherDynamic = "world.weather.dynamic";

        [English(
            "Set Time",
            "Type a time of day. The clock carries on from there, unless you have frozen it. Most formats work: 15:25, 3.25pm, 325pm, 1525.")]
        public const string SetTime = "world.settime";

        [English("Time of day")]
        public const string SetTimePrompt = "world.settime.prompt";

        [English(
            "Preset Time",
            "Pick one of the times the server owner set up, then press Enter to move the clock to it. The clock carries on from there, unless you have frozen it.")]
        public const string TimePreset = "world.timepreset";

        [English(
            "Blackout",
            "Kills the lights across the map. Follow Weather Schedule uses the blackouts planned in a custom weather schedule, if the server has one. Press Enter to apply. Everybody on the server sees this.")]
        public const string Blackout = "world.blackout";

        [English("Follow Weather Schedule")]
        public const string BlackoutDynamic = "world.blackout.dynamic";

        [English("Off")]
        public const string BlackoutOff = "world.blackout.off";

        [English("City Lights Out")]
        public const string BlackoutCity = "world.blackout.city";

        [English("City And Vehicle Lights Out")]
        public const string BlackoutCityAndVehicles = "world.blackout.all";

        [English("Blackout is now ~y~{value}~s~.")]
        public const string BlackoutSet = "world.blackout.set";

        [English(
            "Snow Effects",
            "Snow on the ground, footprints, tyre tracks and the snow particles. Automatic follows the weather. Press Enter to apply.")]
        public const string Snow = "world.snow";

        [English("Weather Cycle")]
        public const string Cycle = "world.cycle";

        [English(
            "Which weather schedule the server follows. Custom uses the server's own schedule. Lasts until the server restarts. Press Enter to apply. Current weather cycle: ~b~{cycle}~s~.")]
        public const string CycleDescription = "world.cycle.desc";

        [English("Default")]
        public const string CycleDefault = "world.cycle.default";

        [English("Snowy")]
        public const string CycleSnowy = "world.cycle.snowy";

        [English("Custom")]
        public const string CycleCustom = "world.cycle.custom";

        [English("The weather cycle is now ~y~{value}~s~.")]
        public const string CycleSet = "world.cycle.set";

        [English("Automatic (Follows Weather)")]
        public const string SnowAutomatic = "world.snow.automatic";

        [English("Always On")]
        public const string SnowOn = "world.snow.on";

        [English("Always Off")]
        public const string SnowOff = "world.snow.off";

        [English("Snow effects are now ~y~{value}~s~.")]
        public const string SnowSet = "world.snow.set";

        [English(
            "Freeze Time",
            "Holds the clock where it is for everybody. The weather carries on changing. Letting it go winds the clock forward to the time it should have been.")]
        public const string FreezeTime = "world.freezetime";

        [English("The clock is ~y~frozen~s~.")]
        public const string TimeFrozen = "world.time.frozen";

        [English("The clock is winding forward to catch up.")]
        public const string TimeUnfrozen = "world.time.unfrozen";

        [English("~b~{weather}~s~ · ~y~{time}~s~ (frozen)")]
        public const string StatusTimeFrozen = "world.status.timefrozen";

        [English("~y~{weather}~s~ (altered) · ~y~{time}~s~ (frozen)")]
        public const string StatusWeatherForcedTimeFrozen = "world.status.weatherforcedtimefrozen";

        [English(
            "Reset To Dynamic Server Weather",
            "Hands the weather back to the cycle that would currently be active in GTA Online.")]
        public const string ResetWeather = "world.resetweather";

        [English("Reset To Dynamic Server Time", "Syncs the clock to GTA Online again.")]
        public const string ResetTime = "world.resettime";

        [English("The weather is changing to ~y~{value}~s~. {transition}")]
        public const string WeatherSet = "world.weather.set";

        [English("The weather is following the server cycle again. {transition}")]
        public const string WeatherReset = "world.weather.reset";

        [English("The time is moving to ~y~{value}~s~. {transition}")]
        public const string TimeSet = "world.time.set";

        [English("The time is following the server again. {transition}")]
        public const string TimeReset = "world.time.reset";

        [English(
            "~r~{input}~s~ is not a time I understand. Try something like ~y~15:25~s~, ~y~3.25pm~s~ or ~y~1525~s~.")]
        public const string TimeNotUnderstood = "world.time.notunderstood";

        /// <summary>Tail of every change notification, left out when the blend is set to zero.</summary>
        [English("Give it about ~y~{duration}~s~ to get there.")]
        public const string Transition = "world.transition";

        [English("You are not allowed to change that.")]
        public const string Denied = "world.denied";

        [English("The server owner has turned that off.")]
        public const string Disabled = "world.disabled";

        [English("That did not work. Try again in a moment.")]
        public const string Failed = "world.failed";

        [English("New moon")]
        public const string MoonPhaseNew = "world.moonphase.new";

        [English("Waxing crescent")]
        public const string MoonPhaseWaxingCrescent = "world.moonphase.waxingcrescent";

        [English("First quarter")]
        public const string MoonPhaseFirstQuarter = "world.moonphase.firstquarter";

        [English("Waxing gibbous")]
        public const string MoonPhaseWaxingGibbous = "world.moonphase.waxinggibbous";

        [English("Full moon")]
        public const string MoonPhaseFull = "world.moonphase.full";

        [English("Waning gibbous")]
        public const string MoonPhaseWaningGibbous = "world.moonphase.waninggibbous";

        [English("Last quarter")]
        public const string MoonPhaseLastQuarter = "world.moonphase.lastquarter";

        [English("Waning crescent")]
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

        [English("Clear")]
        public const string WeatherClear = "world.weathername.clear";

        [English("Extra Sunny")]
        public const string WeatherExtraSunny = "world.weathername.extrasunny";

        [English("Cloudy")]
        public const string WeatherClouds = "world.weathername.clouds";

        [English("Overcast")]
        public const string WeatherOvercast = "world.weathername.overcast";

        [English("Rain")]
        public const string WeatherRain = "world.weathername.rain";

        [English("Clearing")]
        public const string WeatherClearing = "world.weathername.clearing";

        [English("Thunderstorm")]
        public const string WeatherThunder = "world.weathername.thunder";

        [English("Smog")]
        public const string WeatherSmog = "world.weathername.smog";

        [English("Fog")]
        public const string WeatherFoggy = "world.weathername.foggy";

        [English("Christmas")]
        public const string WeatherXmas = "world.weathername.xmas";

        [English("Snow")]
        public const string WeatherSnow = "world.weathername.snow";

        [English("Light Snow")]
        public const string WeatherSnowLight = "world.weathername.snowlight";

        [English("Blizzard")]
        public const string WeatherBlizzard = "world.weathername.blizzard";

        [English("Halloween")]
        public const string WeatherHalloween = "world.weathername.halloween";

        [English("Neutral")]
        public const string WeatherNeutral = "world.weathername.neutral";

        [English("Halloween Rain")]
        public const string WeatherRainHalloween = "world.weathername.rainhalloween";

        [English("Halloween Snow")]
        public const string WeatherSnowHalloween = "world.weathername.snowhalloween";

        public static string BlackoutName(BlackoutMode mode) => mode switch
        {
            BlackoutMode.City => BlackoutCity,
            BlackoutMode.CityAndVehicles => BlackoutCityAndVehicles,
            BlackoutMode.Dynamic => BlackoutDynamic,
            _ => BlackoutOff,
        };

        public static string CycleName(WeatherCycleType cycle) => cycle switch
        {
            WeatherCycleType.Snowy => CycleSnowy,
            WeatherCycleType.Custom => CycleCustom,
            _ => CycleDefault,
        };

        public static string SnowName(SnowMode mode) => mode switch
        {
            SnowMode.On => SnowOn,
            SnowMode.Off => SnowOff,
            _ => SnowAutomatic,
        };

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

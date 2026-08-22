using System.Globalization;

using MenuAPI;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.World;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using TimeOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.TimeOptions;
using TimeOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.TimeOptions;
using WeatherOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.WeatherOptions;
using WeatherOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.WeatherOptions;

namespace vMenu.Enhanced.Menus;

/// <summary>Weather and time, which apply to everybody on the server.</summary>
[VMenu(
    TitleKey = Loc.World.Title,
    SubtitleKey = Loc.World.Subtitle,
    DescriptionKey = Loc.World.LinkDescription)]
public sealed class WorldMenu : MenuDefinition
{
    private static readonly MenuGate WeatherAllowed =
        MenuGate.Setting(WeatherOptionsSettings.Enabled) & MenuGate.Permission(WeatherOptionsPermissions.SetWeather);

    private static readonly MenuGate TimeAllowed =
        MenuGate.Setting(TimeOptionsSettings.Enabled) & MenuGate.Permission(TimeOptionsPermissions.SetTime);

    private static readonly MenuGate ForecastAllowed =
        MenuGate.Setting(WeatherOptionsSettings.Enabled) & MenuGate.Permission(WeatherOptionsPermissions.Forecast);

    private static readonly TimePresetOptions Presets = new();

    /// <summary>Hides the row rather than showing an empty list when an owner clears the convar.</summary>
    private static readonly MenuGate PresetsAllowed = TimeAllowed & MenuGate.When(() => Presets.Count > 0);

    private static TickHandle? _status;

    private static Menu? _open;

    public override MenuGate Gate =>
        (MenuGate.Setting(WeatherOptionsSettings.Enabled) & MenuGate.Permission(WeatherOptionsPermissions.Menu))
        | (MenuGate.Setting(TimeOptionsSettings.Enabled) & MenuGate.Permission(TimeOptionsPermissions.Menu));

    public override GateBehaviour? LinkBehaviour => GateBehaviour.Hide;

    public override GateBehaviour? DefaultGateBehaviour => GateBehaviour.Hide;

    protected override void Build(MenuBuilder menu)
    {
        menu.OnOpened = opened => StartStatus(opened.Menu);
        menu.OnClosed = _ => StopStatus();

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.World.Weather),
            Description = MenuText.Key(Loc.World.WeatherDescription),
            Gate = WeatherAllowed,
            Options = WeatherOptions(),
            ReadSelectedIndex = () => WorldState.WeatherOverride is { } type ? (int)type + 1 : 0,
            OnSelectedAsync = async selected =>
            {
                if (selected.SelectedIndex <= 0)
                {
                    await SendAsync(
                        ActionIds.WeatherOptions.SetWeather,
                        WorldStateConvars.Dynamic,
                        Loc.World.WeatherReset,
                        WeatherOptionsSettings.TransitionSeconds);

                    return;
                }

                var type = WeatherTypes.Selectable[selected.SelectedIndex - 1];

                await SendAsync(
                    ActionIds.WeatherOptions.SetWeather,
                    WeatherTypes.NameOf(type),
                    Loc.World.WeatherSet,
                    WeatherOptionsSettings.TransitionSeconds,
                    MenuText.Key(Loc.World.WeatherName(type)));
            },
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.World.Forecast),
            Description = MenuText.Key(Loc.World.ForecastDescription),
            Gate = ForecastAllowed,
            ReadState = () => UserDefaults.WorldWeatherForecast.Value,
            OnChanged = changed => WeatherForecast.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.World.ForecastStyle),
            Description = MenuText.Key(Loc.World.ForecastStyleDescription),
            LockedDescription = MenuText.Key(Loc.World.ForecastStyleLocked),
            Gate = ForecastAllowed & MenuGate.When(() => UserDefaults.WorldWeatherForecast.Value),
            Options =
            [
                MenuText.Key(Loc.World.ForecastStyleFull),
                MenuText.Key(Loc.World.ForecastStyleCompact),
            ],
            ReadSelectedIndex = () => WeatherForecast.Style,
            OnIndexChanged = changed => WeatherForecast.SetStyle(changed.NewIndex),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.World.SetTime),
            Description = MenuText.Key(Loc.World.SetTimeDescription),
            Gate = TimeAllowed,
            OnSelectedAsync = _ => PromptForTimeAsync(),
        });

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.World.TimePreset),
            Description = MenuText.Key(Loc.World.TimePresetDescription),
            Gate = PresetsAllowed,
            Options = Presets,
            OnSelectedAsync = selected => Presets.SecondOfDay(selected.SelectedIndex) is { } secondOfDay
                ? SetTimeAsync(secondOfDay)
                : Task.CompletedTask,
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.World.ResetWeather),
            Description = MenuText.Key(Loc.World.ResetWeatherDescription),
            Gate = WeatherAllowed,
            OnSelectedAsync = _ => SendAsync(
                ActionIds.WeatherOptions.SetWeather,
                WorldStateConvars.Dynamic,
                Loc.World.WeatherReset,
                WeatherOptionsSettings.TransitionSeconds),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.World.ResetTime),
            Description = MenuText.Key(Loc.World.ResetTimeDescription),
            Gate = TimeAllowed,
            OnSelectedAsync = _ => SendAsync(
                ActionIds.TimeOptions.SetTime,
                ActionIds.TimeOptions.RealTime,
                Loc.World.TimeReset,
                TimeOptionsSettings.TransitionSeconds),
        });
    }

    private static async Task PromptForTimeAsync()
    {
        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.World.SetTimePrompt),
            TimeText.MaxInputLength,
            TimeText.Example);

        if (typed is null)
        {
            return;
        }

        if (!TimeText.TryParse(typed, out var secondOfDay))
        {
            Notifications.Error(MenuText.Key(Loc.World.TimeNotUnderstood, ("input", MenuText.Literal(typed))));

            return;
        }

        await SetTimeAsync(secondOfDay);
    }

    private static Task SetTimeAsync(int secondOfDay)
    {
        // Stored as an offset from the derived clock, not as a fixed time, so the day keeps running.
        var offset = (int)GameClock.Mod(
            secondOfDay - GameClock.SecondOfDay(WorldState.UnixSeconds, WorldState.TimeSpeed),
            GameClock.SecondsPerGameDay);

        return SendAsync(
            ActionIds.TimeOptions.SetTime,
            offset.ToString(CultureInfo.InvariantCulture),
            Loc.World.TimeSet,
            TimeOptionsSettings.TransitionSeconds,
            MenuText.Literal(TimeText.Format(secondOfDay)));
    }

    /// <param name="transition">
    /// How long the sky takes to get there, named in the notification. Neither change is instant, and
    /// without saying so the weather blend in particular reads as nothing having happened.
    /// </param>
    private static async Task SendAsync(
        string action,
        string argument,
        string successKey,
        IntSetting transition,
        MenuText? value = null)
    {
        var result = await ServerActions.InvokeAsync(action, argument);

        if (result.Status == ActionStatus.Ok)
        {
            Notifications.Success(value is { } text
                ? MenuText.Key(successKey, ("value", text), ("transition", Transition(transition)))
                : MenuText.Key(successKey, ("transition", Transition(transition))));

            return;
        }

        Notifications.Error(MenuText.Key(result.Status switch
        {
            ActionStatus.Denied => Loc.World.Denied,
            ActionStatus.Refused => Loc.World.Disabled,
            _ => Loc.World.Failed,
        }));
    }

    /// <summary>
    /// Empty when the owner has set the blend to zero, so the message never promises a wait that is
    /// not coming. That leaves a trailing space in the sentence, which nobody can see.
    /// </summary>
    private static MenuText Transition(IntSetting setting)
    {
        var seconds = Math.Max(0, ClientConfig.Value(setting));

        return seconds == 0
            ? MenuText.Empty
            : MenuText.Key(
                Loc.World.Transition,
                ("duration", MenuText.Literal(seconds.ToString(CultureInfo.InvariantCulture) + "s")));
    }

    private static List<MenuText> WeatherOptions()
    {
        var options = new List<MenuText>(WeatherTypes.Selectable.Count + 1)
        {
            MenuText.Key(Loc.World.WeatherDynamic),
        };

        foreach (var type in WeatherTypes.Selectable)
        {
            options.Add(MenuText.Key(Loc.World.WeatherName(type)));
        }

        return options;
    }

    // A tick rather than a refresh pass: the framework only re-applies presentation when permissions,
    // config or the language change, and this has to move every quarter second.
    private static void StartStatus(Menu menu)
    {
        _open = menu;

        _status ??= TickRegistry.Register(
            "World.MenuStatus",
            UpdateSubtitle,
            TickRate.Every(250),
            autoStart: false);

        UpdateSubtitle();

        _status.Start();
    }

    private static void StopStatus()
    {
        _status?.Stop();

        _open = null;
    }

    private static void UpdateSubtitle()
    {
        if (_open is not { } menu)
        {
            return;
        }

        var forced = WorldState.WeatherOverride is not null;
        var offset = WorldState.TimeOffsetSeconds != 0;

        var status = MenuText.Key(
            (forced, offset) switch
            {
                (true, true) => Loc.World.StatusBothForced,
                (true, false) => Loc.World.StatusWeatherForced,
                (false, true) => Loc.World.StatusTimeForced,
                _ => Loc.World.Status,
            },
            ("weather", MenuText.Key(Loc.World.WeatherName(WorldState.Weather))),
            ("time", MenuText.Literal(TimeText.Format((int)WorldState.SecondOfDay))));

        menu.MenuSubtitle = status.Resolve(Localizer.Current);
    }
}

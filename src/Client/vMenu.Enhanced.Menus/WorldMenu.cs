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

    private static readonly MenuGate SnowAllowed =
        MenuGate.Setting(WeatherOptionsSettings.Enabled) & MenuGate.Permission(WeatherOptionsPermissions.Snow);

    private static readonly MenuGate FreezeAllowed =
        MenuGate.Setting(TimeOptionsSettings.Enabled) & MenuGate.Permission(TimeOptionsPermissions.FreezeTime);

    private static readonly MenuGate BlackoutAllowed = MenuGate.Permission(WeatherOptionsPermissions.Blackout);

    private static readonly TimePresetOptions Presets = new();

    // Hides the row rather than showing an empty list when an owner clears the convar.
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

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.World.FreezeTime),
            Description = MenuText.Key(Loc.World.FreezeTimeDescription),
            Gate = FreezeAllowed,
            ReadState = static () => WorldState.IsTimeFrozen,
            OnChangedAsync = changed => SendStateAsync(
                ActionIds.TimeOptions.SetFrozen,
                changed.Checked ? "true" : "false",
                changed.Checked ? Loc.World.TimeFrozen : Loc.World.TimeUnfrozen),
        });

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.World.Blackout),
            Description = MenuText.Key(Loc.World.BlackoutDescription),
            Gate = BlackoutAllowed,
            Options = ModeOptions(BlackoutModes.Selectable, Loc.World.BlackoutName),
            ReadSelectedIndex = static () => (int)WorldState.Blackout,
            OnSelectedAsync = selected =>
            {
                var mode = BlackoutModes.Selectable[selected.SelectedIndex];

                return SendStateAsync(
                    ActionIds.WeatherOptions.SetBlackout,
                    BlackoutModes.NameOf(mode),
                    Loc.World.BlackoutSet,
                    MenuText.Key(Loc.World.BlackoutName(mode)));
            },
        });

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.World.Snow),
            Description = MenuText.Key(Loc.World.SnowDescription),
            Gate = SnowAllowed,
            Options = ModeOptions(SnowModes.Selectable, Loc.World.SnowName),
            ReadSelectedIndex = static () => (int)WorldState.SnowSetting,
            OnSelectedAsync = selected =>
            {
                var mode = SnowModes.Selectable[selected.SelectedIndex];

                return SendStateAsync(
                    ActionIds.WeatherOptions.SetSnow,
                    SnowModes.NameOf(mode),
                    Loc.World.SnowSet,
                    MenuText.Key(Loc.World.SnowName(mode)));
            },
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
            secondOfDay - GameClock.SecondOfDay(WorldState.ClockUnixSeconds, WorldState.TimeSpeed),
            GameClock.SecondsPerGameDay);

        return SendAsync(
            ActionIds.TimeOptions.SetTime,
            offset.ToString(CultureInfo.InvariantCulture),
            Loc.World.TimeSet,
            TimeOptionsSettings.TransitionSeconds,
            MenuText.Literal(TimeText.Format(secondOfDay)));
    }

    // transition is how long the sky takes to get there, named in the notification. Neither change is
    // instant, and without saying so the weather blend in particular reads as nothing having happened.
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

    private static async Task SendStateAsync(string action, string argument, string successKey, MenuText? value = null)
    {
        var result = await ServerActions.InvokeAsync(action, argument);

        if (result.Status == ActionStatus.Ok)
        {
            Notifications.Success(value is { } text
                ? MenuText.Key(successKey, ("value", text))
                : MenuText.Key(successKey));

            return;
        }

        // A refusal changes no convar, so nothing else would put the row back on the real value.
        RefreshOpen();

        Notifications.Error(MenuText.Key(result.Status switch
        {
            ActionStatus.Denied => Loc.World.Denied,
            ActionStatus.Refused => Loc.World.Disabled,
            _ => Loc.World.Failed,
        }));
    }

    private static List<MenuText> ModeOptions<T>(IReadOnlyList<T> modes, Func<T, string> key)
    {
        var options = new List<MenuText>(modes.Count);

        foreach (var mode in modes)
        {
            options.Add(MenuText.Key(key(mode)));
        }

        return options;
    }

    // Empty when the owner has set the blend to zero, so the message never promises a wait that is not
    // coming. That leaves a trailing space in the sentence, which nobody can see.
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

        // State convars are quiet, so the framework's blanket refresh never sees them.
        WorldState.Changed -= RefreshOpen;
        WorldState.Changed += RefreshOpen;

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

        WorldState.Changed -= RefreshOpen;

        _open = null;
    }

    private static void RefreshOpen()
    {
        if (_open is { } menu)
        {
            MenuRegistry.Refresh(menu);
        }
    }

    private static void UpdateSubtitle()
    {
        if (_open is not { } menu)
        {
            return;
        }

        var forced = WorldState.WeatherOverride is not null;
        var frozen = WorldState.IsTimeFrozen;
        var offset = WorldState.TimeOffsetSeconds != 0;

        var status = MenuText.Key(
            (forced, frozen, offset) switch
            {
                (true, true, _) => Loc.World.StatusWeatherForcedTimeFrozen,
                (false, true, _) => Loc.World.StatusTimeFrozen,
                (true, false, true) => Loc.World.StatusBothForced,
                (true, false, false) => Loc.World.StatusWeatherForced,
                (false, false, true) => Loc.World.StatusTimeForced,
                _ => Loc.World.Status,
            },
            ("weather", MenuText.Key(Loc.World.WeatherName(WorldState.Weather))),
            ("time", MenuText.Literal(TimeText.Format((int)WorldState.SecondOfDay))));

        menu.MenuSubtitle = status.Resolve(Localizer.Current);
    }
}

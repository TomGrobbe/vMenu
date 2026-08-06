using System.Globalization;

using MenuAPI;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.World;
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
                var argument = selected.SelectedIndex <= 0
                    ? WorldStateConvars.Dynamic
                    : WeatherTypes.NameOf(WeatherTypes.Selectable[selected.SelectedIndex - 1]);

                await SendAsync(ActionIds.WeatherOptions.SetWeather, argument, Loc.World.WeatherSet);
            },
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.World.SetTime),
            Description = MenuText.Key(Loc.World.SetTimeDescription),
            Gate = TimeAllowed,
            OnSelectedAsync = _ => PromptForTimeAsync(),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.World.ResetWeather),
            Description = MenuText.Key(Loc.World.ResetWeatherDescription),
            Gate = WeatherAllowed,
            OnSelectedAsync = _ =>
                SendAsync(ActionIds.WeatherOptions.SetWeather, WorldStateConvars.Dynamic, Loc.World.WeatherReset),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.World.ResetTime),
            Description = MenuText.Key(Loc.World.ResetTimeDescription),
            Gate = TimeAllowed,
            OnSelectedAsync = _ => SendAsync(ActionIds.TimeOptions.SetTime, "0", Loc.World.TimeReset),
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

        // Stored as an offset from the derived clock, not as a fixed time, so the day keeps running.
        var offset = (int)GameClock.Mod(
            secondOfDay - GameClock.SecondOfDay(WorldState.UnixSeconds),
            GameClock.SecondsPerGameDay);

        await SendAsync(
            ActionIds.TimeOptions.SetTime,
            offset.ToString(CultureInfo.InvariantCulture),
            Loc.World.TimeSet,
            MenuText.Literal(TimeText.Format(secondOfDay)));
    }

    private static async Task SendAsync(string action, string argument, string successKey, MenuText? value = null)
    {
        var result = await ServerActions.InvokeAsync(action, argument);

        if (result.Status == ActionStatus.Ok)
        {
            Notifications.Success(value is { } text
                ? MenuText.Key(successKey, ("value", text))
                : MenuText.Key(successKey));

            return;
        }

        Notifications.Error(MenuText.Key(result.Status switch
        {
            ActionStatus.Denied => Loc.World.Denied,
            ActionStatus.Refused => Loc.World.Disabled,
            _ => Loc.World.Failed,
        }));
    }

    private static IReadOnlyList<MenuText> WeatherOptions()
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

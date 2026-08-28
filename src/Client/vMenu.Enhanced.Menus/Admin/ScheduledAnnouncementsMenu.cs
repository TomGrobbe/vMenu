using System.Globalization;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Admin;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using AdminPermissions = vMenu.Enhanced.Data.Permissions.Menus.Admin;
using AdminSettings = vMenu.Enhanced.Data.Configuration.Settings.Admin;

namespace vMenu.Enhanced.Menus.Admin;

[VMenu(
    TitleKey = Loc.Admin.Schedule,
    DescriptionKey = Loc.Admin.ScheduleDescription,
    Permission = AdminPermissions.ManageAnnouncements)]
public sealed class ScheduledAnnouncementsMenu : MenuDefinition
{
    private const int NameLimit = 40;

    private const int TextLimit = 200;

    private const int TriggerLimit = 20;

    private const int MinEveryMinutes = 1;

    private const int MaxEveryMinutes = 1440;

    private const char IntervalMarker = '@';

    private const string GameSuffix = "game";

    private readonly List<Scheduled> _entries = [];

    private MenuBuilder? _menu;

    private DetachedMenu? _actions;

    private Scheduled? _selected;

    private bool _busy;

    public override GateBehaviour? LinkBehaviour => GateBehaviour.Hide;

    public override MenuText Subtitle =>
        MenuText.From(() => MenuText
            .Key(Loc.Admin.ScheduleSubtitle, ("count", MenuText.Literal(Count())))
            .Resolve(Localizer.Current));

    protected override void Build(MenuBuilder menu)
    {
        _menu = menu;

        _actions = menu.AddDetachedMenu(
            MenuText.From(() => _selected?.Name ?? string.Empty),
            MenuText.From(() => _selected is { } entry ? TriggerSentence(entry) : string.Empty),
            BuildActions);

        menu.OnOpened = _ => Refresh();
    }

    private void BuildActions(MenuBuilder actions) =>
        actions.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.Admin.ScheduleRemove),
            Description = MenuText.Key(Loc.Admin.ScheduleRemoveDescription),
            OnConfirmedAsync = _ => RemoveAsync(),
        });

    private void Refresh() => _ = RefreshAsync();

    private async Task RefreshAsync()
    {
        if (_menu is not { } menu || _busy)
        {
            return;
        }

        _busy = true;

        try
        {
            var result = await ServerActions.InvokeAsync(ActionIds.Admin.GetAnnouncements);

            _entries.Clear();

            if (result.Status != ActionStatus.Ok)
            {
                AdminReport.Show(result);
            }
            else
            {
                foreach (var row in result.Data)
                {
                    if (AnnouncementRow.TryParse(row, out var index, out var name, out var text, out var minutes, out var at, out var clock))
                    {
                        _entries.Add(new Scheduled(index, name, text, minutes, at, clock));
                    }
                }
            }

            RebuildRows(menu);

            menu.Menu.MenuSubtitle = MenuText
                .Key(Loc.Admin.ScheduleSubtitle, ("count", MenuText.Literal(Count())))
                .Resolve(Localizer.Current);
        }
        finally
        {
            _busy = false;
        }
    }

    private void RebuildRows(MenuBuilder menu)
    {
        menu.ClearEntries();

        var rows = new List<MenuEntry>(_entries.Count + 2);

        if (!ScheduleRunning)
        {
            rows.Add(new SeparatorEntry
            {
                Text = MenuText.Key(Loc.Admin.ScheduleDisabled),
                Description = MenuText.Key(Loc.Admin.ScheduleDisabledDescription),
                ShowArrows = false,
            });
        }

        if (_entries.Count == 0)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.Admin.ScheduleEmpty),
                Description = MenuText.Key(Loc.Admin.ScheduleEmptyDescription),
            });
        }

        foreach (var entry in _entries)
        {
            var current = entry;

            rows.Add(new ButtonEntry
            {
                Text = MenuText.Literal(current.Name),
                Label = MenuText.Literal(TriggerLabel(current)),
                Description = MenuText.Key(
                    Loc.Admin.ScheduleRowDescription,
                    ("trigger", MenuText.Literal(TriggerSentence(current))),
                    ("text", MenuText.Literal(current.Text))),
                OnSelected = _ => Open(current),
            });
        }

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.Admin.ScheduleAdd),
            Description = MenuText.Key(Loc.Admin.ScheduleAddDescription),
            OnSelectedAsync = _ => AddAsync(),
        });

        menu.AddRange(rows);
    }

    private void Open(Scheduled entry)
    {
        if (_actions is not { } actions)
        {
            return;
        }

        _selected = entry;

        actions.Open();
    }

    private async Task AddAsync()
    {
        var typedTrigger = await UserInput.GetTextAsync(
            MenuText.Key(Loc.Admin.ScheduleTriggerPrompt),
            TriggerLimit,
            description: MenuText.Key(Loc.Admin.ScheduleTriggerHelp));

        if (typedTrigger is null)
        {
            return;
        }

        if (!TryReadTrigger(typedTrigger, out var minutes, out var at, out var clock))
        {
            Notifications.Warning(MenuText.Key(Loc.Admin.ScheduleTriggerInvalid));

            return;
        }

        var answers = await UserInput.GetTextAsync(
            new InputPrompt(
                MenuText.Key(Loc.Admin.ScheduleNamePrompt),
                NameLimit,
                description: MenuText.Key(Loc.Admin.ScheduleNameHelp)),
            new InputPrompt(
                MenuText.Key(Loc.Admin.ScheduleTextPrompt),
                TextLimit,
                description: MenuText.Key(Loc.Admin.ScheduleTextHelp)));

        if (answers is null)
        {
            return;
        }

        var name = answers[0].Trim();
        var text = answers[1].Trim();

        if (name.Length == 0 || text.Length == 0)
        {
            Notifications.Warning(MenuText.Key(Loc.Admin.AnnounceEmpty));

            return;
        }

        var result = await ServerActions.InvokeAsync(
            ActionIds.Admin.AddAnnouncement,
            name,
            text,
            minutes.ToString(CultureInfo.InvariantCulture),
            at,
            clock);

        if (result.Status != ActionStatus.Ok)
        {
            AdminReport.Show(result);

            return;
        }

        Notifications.Success(MenuText.Key(Loc.Admin.ScheduleAdded, ("name", MenuText.Literal(name))));

        await RefreshAsync();
    }

    private async Task RemoveAsync()
    {
        if (_selected is not { } entry)
        {
            return;
        }

        var result = await ServerActions.InvokeAsync(
            ActionIds.Admin.RemoveAnnouncement,
            entry.Index.ToString(CultureInfo.InvariantCulture),
            entry.Name);

        if (result.Status != ActionStatus.Ok)
        {
            AdminReport.Show(result);

            return;
        }

        Notifications.Success(MenuText.Key(Loc.Admin.ScheduleRemoved, ("name", MenuText.Literal(entry.Name))));

        _actions?.Menu.GoBack();

        await RefreshAsync();
    }

    private static bool TryReadTrigger(string typed, out int minutes, out string at, out string clock)
    {
        minutes = 0;
        at = string.Empty;
        clock = AnnouncementClock.Real;

        var rest = typed.Trim();

        if (rest.Length == 0)
        {
            return false;
        }

        if (rest.EndsWith(GameSuffix, StringComparison.OrdinalIgnoreCase))
        {
            clock = AnnouncementClock.Game;
            rest = rest[..^GameSuffix.Length].TrimEnd();

            if (rest.Length == 0)
            {
                return false;
            }
        }

        if (rest[0] == IntervalMarker)
        {
            return int.TryParse(
                    rest[1..].Trim(),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out minutes)
                && minutes >= MinEveryMinutes
                && minutes <= MaxEveryMinutes;
        }

        var time = rest.Split(':', 2);

        if (time.Length != 2
            || !int.TryParse(time[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var hour)
            || !int.TryParse(time[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var minute)
            || hour is < 0 or > 23
            || minute is < 0 or > 59)
        {
            return false;
        }

        at = $"{hour:00}:{minute:00}";

        return true;
    }
    private static string TriggerLabel(Scheduled entry)
    {
        var localizer = Localizer.Current;

        var text = entry.EveryMinutes > 0
            ? MenuText.Key(Loc.Admin.ScheduleEveryLabel, ("minutes", Number(entry.EveryMinutes))).Resolve(localizer)
            : MenuText.Key(Loc.Admin.ScheduleAtLabel, ("time", MenuText.Literal(entry.At))).Resolve(localizer);

        return IsGame(entry) ? text + localizer.Get(Loc.Admin.ScheduleGameMark) : text;
    }

    private static string TriggerSentence(Scheduled entry)
    {
        var game = IsGame(entry);

        var text = entry.EveryMinutes > 0
            ? MenuText.Key(
                game ? Loc.Admin.ScheduleEveryGame : Loc.Admin.ScheduleEveryReal,
                ("minutes", Number(entry.EveryMinutes)))
            : MenuText.Key(
                game ? Loc.Admin.ScheduleAtGame : Loc.Admin.ScheduleAtReal,
                ("time", MenuText.Literal(entry.At)));

        return text.Resolve(Localizer.Current);
    }

    private static bool IsGame(Scheduled entry) => AnnouncementClock.IsGame(entry.Clock);

    private static MenuText Number(int value) =>
        MenuText.Literal(value.ToString(CultureInfo.InvariantCulture));
    private string Count() => _entries.Count.ToString(CultureInfo.InvariantCulture);

    private static bool ScheduleRunning => ClientConfig.Value(AdminSettings.ScheduledAnnouncements);

    private sealed class Scheduled(int index, string name, string text, int everyMinutes, string at, string clock)
    {
        public int Index { get; } = index;

        public string Name { get; } = name;

        public string Text { get; } = text;

        public int EveryMinutes { get; } = everyMinutes;

        public string At { get; } = at;

        public string Clock { get; } = clock;
    }
}

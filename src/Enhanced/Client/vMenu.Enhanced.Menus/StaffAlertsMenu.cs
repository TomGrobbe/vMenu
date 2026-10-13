using System.Globalization;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Data.StaffAlerts;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using StaffAlertSettings = vMenu.Enhanced.Data.Configuration.Settings.StaffAlerts;
using StaffAlertsFeature = vMenu.Enhanced.Menus.Misc.StaffAlerts;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.StaffAlerts.Title,
    DescriptionKey = Loc.StaffAlerts.LinkDescription)]
public sealed class StaffAlertsMenu : MenuDefinition
{
    private readonly List<WaitingAlert> _alerts = [];

    private MenuBuilder? _menu;

    private DetachedMenu? _actions;

    private WaitingAlert? _selected;

    private bool _busy;

    public override MenuGate Gate =>
        MenuGate.Setting(StaffAlertSettings.Enabled) & MenuGate.Permission(Global.Staff);

    public override GateBehaviour? LinkBehaviour => GateBehaviour.Hide;

    public override MenuText Subtitle =>
        MenuText.From(() => MenuText
            .Key(Loc.StaffAlerts.Subtitle, ("count", MenuText.Literal(_alerts.Count.ToString(CultureInfo.InvariantCulture))))
            .Resolve(Localizer.Current));

    protected override void Build(MenuBuilder menu)
    {
        _menu = menu;

        _actions = menu.AddDetachedMenu(
            MenuText.From(() => _selected?.Player ?? string.Empty),
            MenuText.From(() => _selected is { } alert
                ? MenuText.Key(
                    Loc.StaffAlerts.ActionsSubtitle,
                    ("id", MenuText.Literal(alert.Id.ToString(CultureInfo.InvariantCulture))),
                    ("time", MenuText.Literal(Countdown(alert)))).Resolve(Localizer.Current)
                : string.Empty),
            BuildActions);

        menu.OnOpened = _ => Refresh();
    }

    private void BuildActions(MenuBuilder actions)
    {
        actions.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.StaffAlerts.Respond),
            Description = MenuText.Key(Loc.StaffAlerts.RespondDescription),
            OnSelectedAsync = _ => RespondAsync(),
        });

        actions.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.StaffAlerts.Show),
            Description = MenuText.Key(Loc.StaffAlerts.ShowDescription),
            OnSelected = _ =>
            {
                if (_selected is { } alert)
                {
                    StaffAlertsFeature.ShowAgain(alert.Id, alert.Player, alert.Description);
                }
            },
        });

        actions.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.StaffAlerts.Dismiss),
            Description = MenuText.Key(Loc.StaffAlerts.DismissDescription),
            OnConfirmedAsync = _ => DismissAsync(),
        });
    }

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
            var result = await ServerActions.InvokeAsync(ActionIds.StaffAlerts.GetList);

            _alerts.Clear();

            if (result.Status == ActionStatus.Ok)
            {
                foreach (var row in result.Data)
                {
                    if (AlertRow.TryParse(row, out var id, out var secondsLeft, out var player, out var description))
                    {
                        _alerts.Add(new WaitingAlert(id, secondsLeft, player, description));
                    }
                }
            }
            else
            {
                Log.Error($"[StaffAlerts] The alert list came back as {result.Status}.");

                Notifications.Error(MenuText.Key(Loc.StaffAlerts.Failed));
            }

            RebuildRows(menu);
        }
        finally
        {
            _busy = false;
        }
    }

    private void RebuildRows(MenuBuilder menu)
    {
        menu.ClearEntries();

        if (_alerts.Count == 0)
        {
            menu.AddRange([
                new ButtonEntry
                {
                    Text = MenuText.Key(Loc.StaffAlerts.Empty),
                    Description = MenuText.Key(Loc.StaffAlerts.EmptyDescription),
                },
            ]);

            AddHideRow(menu);

            return;
        }

        var rows = new List<MenuEntry>(_alerts.Count + 2);

        foreach (var alert in _alerts)
        {
            var current = alert;

            rows.Add(new ButtonEntry
            {
                Text = MenuText.Literal(current.Player),
                Label = MenuText.Literal(Countdown(current)),
                Description = MenuText.Key(
                    Loc.StaffAlerts.RowDescription,
                    ("player", MenuText.Literal(current.Player)),
                    ("description", MenuText.Literal(current.Description)),
                    ("id", MenuText.Literal(current.Id.ToString(CultureInfo.InvariantCulture))),
                    ("time", MenuText.Literal(Countdown(current)))),
                OnSelected = _ => OpenActions(current),
            });
        }

        menu.AddRange(rows);

        AddHideRow(menu);
    }

    private void AddHideRow(MenuBuilder menu)
    {
        menu.AddRange([
            new SeparatorEntry
            {
                Text = MenuText.Key(Loc.StaffAlerts.HideGroup),
                Description = MenuText.Key(Loc.StaffAlerts.HideGroupDescription),
            },
            new CheckboxEntry
            {
                Text = MenuText.Key(Loc.StaffAlerts.Hide),
                Description = MenuText.Key(Loc.StaffAlerts.HideDescription),
                ReadState = () => StaffAlertsFeature.Hidden,
                OnChanged = changed =>
                {
                    StaffAlertsFeature.SetHidden(changed.Checked);

                    Refresh();
                },
            },
        ]);
    }

    private void OpenActions(WaitingAlert alert)
    {
        if (_actions is not { } actions)
        {
            return;
        }

        _selected = alert;

        actions.Open();
    }

    private async Task RespondAsync()
    {
        if (_selected is not { } alert)
        {
            return;
        }

        if (!await StaffAlertsFeature.RespondToAsync(alert.Id.ToString(CultureInfo.InvariantCulture)))
        {
            return;
        }

        Drop(alert);

        _actions?.Menu.GoBack();
    }

    private async Task DismissAsync()
    {
        if (_selected is not { } alert)
        {
            return;
        }

        var result = await ServerActions.InvokeAsync(
            ActionIds.StaffAlerts.Dismiss,
            alert.Id.ToString(CultureInfo.InvariantCulture));

        if (result.Status != ActionStatus.Ok)
        {
            Notifications.Error(MenuText.Key(Loc.StaffAlerts.Failed));

            return;
        }

        Drop(alert);

        Notifications.Success(MenuText.Key(
            Loc.StaffAlerts.DismissDone,
            ("player", MenuText.Literal(alert.Player))));

        _actions?.Menu.GoBack();
    }

    private void Drop(WaitingAlert alert)
    {
        _alerts.Remove(alert);

        if (_menu is { } menu)
        {
            RebuildRows(menu);
        }
    }

    private static string Countdown(WaitingAlert alert)
    {
        var left = Math.Max(0, alert.SecondsLeft);

        return $"{left / 60}:{left % 60:00}";
    }

    private sealed class WaitingAlert(int id, int secondsLeft, string player, string description)
    {
        public int Id { get; } = id;

        public int SecondsLeft { get; } = secondsLeft;

        public string Player { get; } = player;

        public string Description { get; } = description;
    }
}

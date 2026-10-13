using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Bully;

using BullyPermissions = vMenu.Enhanced.Data.Permissions.Menus.Bully;
using BullySettings = vMenu.Enhanced.Data.Configuration.Settings.Bully;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.Bully.Title,
    SubtitleKey = Loc.Bully.Subtitle,
    DescriptionKey = Loc.Bully.LinkDescription)]
public sealed class BullyMenu : MenuDefinition
{
    private readonly HashSet<string> _serverToggles = new(StringComparer.Ordinal);

    private MenuBuilder? _server;

    private MenuBuilder? _active;

    public override MenuGate Gate =>
        MenuGate.Setting(BullySettings.Enabled) & MenuGate.Permission(BullyPermissions.Menu);

    public override GateBehaviour? LinkBehaviour => GateBehaviour.Hide;

    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(Submenu(Loc.Bully.SelfMenu, Loc.Bully.SelfMenuDescription, BullyPanel.ForSelf().Build));
        menu.Entries.Add(Submenu(Loc.Bully.EveryoneMenu, Loc.Bully.EveryoneMenuDescription, BullyPanel.ForEveryone().Build));
        menu.Entries.Add(Submenu(Loc.Bully.ServerMenu, Loc.Bully.ServerMenuDescription, BuildServer));
        menu.Entries.Add(Submenu(Loc.Bully.ActiveMenu, Loc.Bully.ActiveMenuDescription, BuildActive));

        menu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.Bully.StopAll),
            Description = MenuText.Key(Loc.Bully.StopAllDescription),
            ConfirmationDescription = MenuText.Key(Loc.Bully.StopAllConfirm),
            OnConfirmedAsync = _ => InvokeAsync(MenuText.Key(Loc.Bully.StopAllDone), ActionIds.Bully.StopAll),
        });
    }

    private static SubmenuEntry Submenu(string text, string description, Action<MenuBuilder> build) => new()
    {
        Text = MenuText.Key(text),
        Description = MenuText.Key(description),
        Build = build,
    };

    private void BuildServer(MenuBuilder menu)
    {
        _server = menu;

        menu.OnOpenedAsync = _ => ReadActiveAsync();

        menu.Entries.AddRange(BullyToggles.ServerWide.Select(toggle => new CheckboxEntry
        {
            Text = MenuText.Key(Loc.Bully.ToggleName(toggle)),
            Description = MenuText.Key(Loc.Bully.ToggleDescription(toggle)),
            ReadState = () => _serverToggles.Contains(toggle),
            OnChangedAsync = changed => InvokeAsync(
                MenuText.Key(changed.Checked ? Loc.Bully.ServerToggleOn : Loc.Bully.ServerToggleOff),
                ActionIds.Bully.ServerToggle(toggle, changed.Checked)),
        }));
    }

    private void BuildActive(MenuBuilder menu)
    {
        _active = menu;

        menu.OnOpenedAsync = _ => ReadActiveAsync();
    }

    private async Task InvokeAsync(MenuText done, string actionId, params string[] args)
    {
        var result = await ServerActions.InvokeAsync(actionId, args);

        if (result.Status == ActionStatus.Ok)
        {
            Notifications.Success(done);
        }
        else
        {
            BullyPanel.Report(result, string.Empty);
        }

        await ReadActiveAsync();
    }

    private async Task ReadActiveAsync()
    {
        var result = await ServerActions.InvokeAsync(ActionIds.Bully.GetActive);

        if (result.Status != ActionStatus.Ok)
        {
            BullyPanel.Report(result, string.Empty);

            return;
        }

        var rows = result.Data
            .Select(line => line.Split(BullyEvents.Separator, StringSplitOptions.None))
            .Where(parts => parts.Length >= 2 && BullyToggles.IsKnown(parts[1]))
            .ToList();

        _serverToggles.Clear();
        _serverToggles.UnionWith(rows.Where(IsServerWide).Select(parts => parts[1]));

        if (_server is { } server)
        {
            MenuRegistry.Refresh(server.Menu);
        }

        if (_active is not { } active)
        {
            return;
        }

        var entries = rows.Where(parts => IsServerWide(parts) || parts.Length >= 4).Select(ActiveRow).ToList<MenuEntry>();

        if (entries.Count == 0)
        {
            entries.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.Bully.ActiveEmpty),
                Description = MenuText.Key(Loc.Bully.ActiveEmptyDescription),
            });
        }

        active.ClearEntries();
        active.AddRange(entries);
    }

    private static bool IsServerWide(string[] parts) => parts[0] == BullyEvents.ServerScope;

    private CheckboxEntry ActiveRow(string[] parts)
    {
        var toggle = parts[1];
        var serverWide = IsServerWide(parts);
        var who = serverWide ? Localizer.Current.Get(Loc.Bully.ActiveServerWide) : parts[3];

        return new CheckboxEntry
        {
            Text = MenuText.From(() => who + ": " + Localizer.Current.Get(Loc.Bully.ToggleName(toggle))),
            Description = MenuText.Key(Loc.Bully.ActiveRowDescription, ("player", MenuText.Literal(who))),
            Checked = true,
            OnChangedAsync = _ => serverWide
                ? InvokeAsync(MenuText.Key(Loc.Bully.ServerToggleOff), ActionIds.Bully.ServerToggle(toggle, false))
                : InvokeAsync(
                    MenuText.Key(Loc.Bully.ToggleOff, ("player", MenuText.Literal(who))),
                    ActionIds.Bully.Toggle(toggle, false),
                    parts[2]),
        };
    }
}

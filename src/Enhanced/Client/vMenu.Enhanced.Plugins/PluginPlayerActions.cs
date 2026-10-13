using MenuAPI;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.Plugins;

// The one place plugins reach outside their own tree: a shared "Plugin Actions" submenu inside every
// player's entry of the Online Players menu. The same action rows serve every player, the target
// being read from the selected snapshot row at the moment an action fires.
public static class PluginPlayerActions
{
    private static readonly Dictionary<MenuItem, ItemNode> NodesByItem = new(ReferenceComparer<MenuItem>.Instance);

    private static MenuBuilder? _builder;

    private static Func<(int ServerId, string Name)?>? _readTarget;

    // Set once the menu has materialised, after which filters are safe to apply.
    private static bool _live;

    private static bool _subscribed;

    // Whether any registered plugin action would currently show. Gates the row.
    public static bool AnyVisible()
    {
        foreach (var state in PluginHost.All)
        {
            foreach (var action in state.PlayerActions)
            {
                if (action.Visible != false && state.EvaluateGate(action.Gate))
                {
                    return true;
                }
            }
        }

        return false;
    }

    // Called by the Online Players menu while it declares its per player actions. readTarget reads the
    // player the shared menu is currently showing.
    public static void Attach(MenuBuilder builder, Func<(int ServerId, string Name)?> readTarget)
    {
        _builder = builder;
        _readTarget = readTarget;

        if (!_subscribed)
        {
            _subscribed = true;

            PluginHost.PluginsChanged += Rebuild;
        }

        // The filter can only be applied once the rows exist, and while Attach runs they do not yet. The
        // first open is the earliest safe moment.
        builder.OnOpened += _ =>
        {
            _live = true;

            builder.SetUserFilter(Filter);
        };

        Rebuild();
    }

    private static void Rebuild()
    {
        if (_builder is not { } builder)
        {
            return;
        }

        NodesByItem.Clear();

        builder.ClearEntries();

        var rows = new List<MenuEntry>();

        var contributors = PluginHost.All
            .Where(static state => state.PlayerActions.Count > 0)
            .OrderBy(static state => state.Resource, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var state in contributors)
        {
            if (contributors.Count > 1)
            {
                var plugin = state;

                rows.Add(new SeparatorEntry
                {
                    Text = MenuText.From(() => plugin.DisplayName is { } name && plugin.Resolve(name) is { Length: > 0 } resolved
                        ? resolved
                        : plugin.Resource),
                });
            }

            foreach (var action in state.PlayerActions)
            {
                if (CreateAction(state, action) is { } entry)
                {
                    rows.Add(entry);
                }
            }
        }

        builder.AddRange(rows);

        if (_live)
        {
            builder.SetUserFilter(Filter);
        }
    }

    private static bool Filter(MenuItem item) =>
        !NodesByItem.TryGetValue(item, out var node) || node.Visible != false;

    private static MenuEntry? CreateAction(PluginState state, ItemNode node)
    {
        switch (node.Type)
        {
            case EntryTypes.Button:
                return new ButtonEntry
                {
                    Text = Text(state, () => node.Text),
                    Description = Description(state, () => node.Description),
                    Label = Text(state, () => node.Label),
                    LockedDescription = Text(state, () => node.LockedDescription),
                    Gate = MenuGate.When(() => state.EvaluateGate(node.Gate)),
                    Behaviour = Behaviour(node),
                    ReadEnabled = () => node.Enabled != false,
                    Configure = item => NodesByItem[item] = node,
                    OnSelected = _ => EmitTargeted(state, node, CallbackTypes.PlayerActionSelected, null),
                };

            case EntryTypes.ConfirmButton:
                return new ConfirmButtonEntry
                {
                    Text = Text(state, () => node.Text),
                    Description = Description(state, () => node.Description),
                    Label = Text(state, () => node.Label),
                    LockedDescription = Text(state, () => node.LockedDescription),
                    ConfirmationDescription = MenuText.From(() => node.ConfirmationDescription is { } wording
                        ? state.Resolve(wording)
                        : Localizer.Current.Get(Loc.Framework.ConfirmDescription)),
                    Gate = MenuGate.When(() => state.EvaluateGate(node.Gate)),
                    Behaviour = Behaviour(node),
                    ReadEnabled = () => node.Enabled != false,
                    Configure = item => NodesByItem[item] = node,
                    OnConfirmed = _ => EmitTargeted(state, node, CallbackTypes.PlayerActionConfirmed, null),
                };

            case EntryTypes.List:
                var options = new List<MenuText>();

                state.OptionsByItemId[node.Id] = options;

                PluginEntryFactory.FillOptions(state, node, options);

                return new ListEntry
                {
                    Text = Text(state, () => node.Text),
                    Description = Description(state, () => node.Description),
                    LockedDescription = Text(state, () => node.LockedDescription),
                    Gate = MenuGate.When(() => state.EvaluateGate(node.Gate)),
                    Behaviour = Behaviour(node),
                    ReadEnabled = () => node.Enabled != false,
                    Configure = item => NodesByItem[item] = node,
                    Options = options,
                    ReadSelectedIndex = () => options.Count == 0
                        ? 0
                        : Math.Clamp(node.SelectedIndex ?? 0, 0, options.Count - 1),
                    OnIndexChanged = changed => node.SelectedIndex = changed.NewIndex,
                    OnSelected = selected => EmitTargeted(
                        state, node, CallbackTypes.PlayerActionListSelected, selected.SelectedIndex),
                };

            case EntryTypes.Separator:
                return new SeparatorEntry
                {
                    Text = Text(state, () => node.Text),
                    Description = Description(state, () => node.Description),
                    Gate = MenuGate.When(() => state.EvaluateGate(node.Gate)),
                    Behaviour = Behaviour(node),
                    Configure = item => NodesByItem[item] = node,
                };

            default:
                return null;
        }
    }

    private static void EmitTargeted(PluginState state, ItemNode node, string type, int? selectedIndex)
    {
        if (_readTarget?.Invoke() is not { } target)
        {
            Log.Warning($"[Plugins] A player action from '{state.Resource}' fired without a selected player.");
            return;
        }

        PluginHost.Emit(state, new PluginCallback
        {
            Type = type,
            MenuId = PluginState.PlayerActionsMenuId,
            ItemId = node.Id,
            SelectedIndex = selectedIndex,
            TargetServerId = target.ServerId,
            TargetName = target.Name,
        });
    }

    private static MenuText Text(PluginState state, Func<TextRef?> read) =>
        MenuText.From(() => state.Resolve(read()));

    // These rows sit among vMenu's own, inside a menu the player did not go to a plugin to reach, so
    // without this there is nothing anywhere on screen saying where they came from.
    private static MenuText Description(PluginState state, Func<TextRef?> read) =>
        MenuText.From(() =>
        {
            var source = Localizer.Current
                .Get(Loc.Plugins.RowDescription)
                .Replace("{resource}", state.Resource);

            return state.Resolve(read()) is { Length: > 0 } description
                ? description + "~n~" + source
                : source;
        });

    private static GateBehaviour? Behaviour(ItemNode node) => node.Behaviour?.ToLowerInvariant() switch
    {
        "hide" => GateBehaviour.Hide,
        "lock" => GateBehaviour.Lock,
        _ => null,
    };
}

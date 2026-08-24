using MenuAPI;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.Plugins;

// Turns declared nodes into live framework entries. Every entry closes over its node, so an update
// op only has to mutate the node and ask for a refresh.
internal static class PluginEntryFactory
{
    // A payload text as late bound menu text, resolved against the plugin's catalogs.
    internal static MenuText TextFor(PluginState state, TextRef? reference) =>
        reference is null ? MenuText.Empty : MenuText.From(() => state.Resolve(reference));

    // The same, but re-reading the node so a set op that replaces the reference lands.
    internal static MenuText LiveText(PluginState state, Func<TextRef?> read) =>
        MenuText.From(() => state.Resolve(read()));

    internal static void BuildMenu(PluginState state, MenuNode menuNode, MenuBuilder builder)
    {
        state.Builders[menuNode.Id] = builder;

        var events = menuNode.Events;

        if (events is not null && events.Contains(NodeEvents.Opened))
        {
            builder.OnOpened += _ => PluginHost.Emit(state, new PluginCallback
            {
                Type = CallbackTypes.MenuOpened,
                MenuId = menuNode.Id,
            });
        }

        if (events is not null && events.Contains(NodeEvents.Closed))
        {
            builder.OnClosed += _ => PluginHost.Emit(state, new PluginCallback
            {
                Type = CallbackTypes.MenuClosed,
                MenuId = menuNode.Id,
            });
        }

        if (events is not null && events.Contains(NodeEvents.IndexChanged))
        {
            builder.OnIndexChanged += changed => PluginHost.Emit(state, new PluginCallback
            {
                Type = CallbackTypes.MenuIndexChanged,
                MenuId = menuNode.Id,
                OldIndex = changed.OldIndex,
                NewIndex = changed.NewIndex,
            });
        }

        foreach (var node in menuNode.Items)
        {
            if (CreateEntry(state, node, menuNode.Id) is { } entry)
            {
                builder.Entries.Add(entry);
            }
        }
    }

    internal static MenuEntry? CreateEntry(PluginState state, ItemNode node, string menuId)
    {
        switch (node.Type)
        {
            case EntryTypes.Button:
                return new ButtonEntry
                {
                    Text = LiveText(state, () => node.Text),
                    Description = LiveText(state, () => node.Description),
                    Label = LiveText(state, () => node.Label),
                    LockedDescription = LiveText(state, () => node.LockedDescription),
                    Gate = GateFor(state, node),
                    Behaviour = BehaviourFor(node),
                    ReadEnabled = () => node.Enabled != false,
                    ReadLeftIcon = () => ParseIcon(node.LeftIcon),
                    ReadRightIcon = () => ParseIcon(node.RightIcon),
                    OnHighlighted = HighlightFor(state, node, menuId),
                    Configure = item => state.NodesByItem[item] = node,
                    OnSelected = _ => PluginHost.Emit(state, Callback(CallbackTypes.ItemSelected, menuId, node)),
                };

            case EntryTypes.ConfirmButton:
                return new ConfirmButtonEntry
                {
                    Text = LiveText(state, () => node.Text),
                    Description = LiveText(state, () => node.Description),
                    Label = LiveText(state, () => node.Label),
                    LockedDescription = LiveText(state, () => node.LockedDescription),
                    ConfirmationDescription = ConfirmTextFor(state, node),
                    Gate = GateFor(state, node),
                    Behaviour = BehaviourFor(node),
                    ReadEnabled = () => node.Enabled != false,
                    ReadLeftIcon = () => ParseIcon(node.LeftIcon),
                    ReadRightIcon = () => ParseIcon(node.RightIcon),
                    OnHighlighted = HighlightFor(state, node, menuId),
                    Configure = item => state.NodesByItem[item] = node,
                    OnConfirmed = _ => PluginHost.Emit(state, Callback(CallbackTypes.Confirmed, menuId, node)),
                };

            case EntryTypes.Checkbox:
                return new CheckboxEntry
                {
                    Text = LiveText(state, () => node.Text),
                    Description = LiveText(state, () => node.Description),
                    LockedDescription = LiveText(state, () => node.LockedDescription),
                    Gate = GateFor(state, node),
                    Behaviour = BehaviourFor(node),
                    ReadEnabled = () => node.Enabled != false,
                    ReadLeftIcon = () => ParseIcon(node.LeftIcon),
                    ReadRightIcon = () => ParseIcon(node.RightIcon),
                    OnHighlighted = HighlightFor(state, node, menuId),
                    Configure = item => state.NodesByItem[item] = node,
                    ReadState = () => node.Checked == true,
                    Style = string.Equals(node.CheckStyle, "cross", StringComparison.OrdinalIgnoreCase)
                        ? MenuCheckboxItem.CheckboxStyle.Cross
                        : MenuCheckboxItem.CheckboxStyle.Tick,
                    OnChanged = changed =>
                    {
                        node.Checked = changed.Checked;

                        var callback = Callback(CallbackTypes.CheckboxChanged, menuId, node);
                        callback.Checked = changed.Checked;

                        PluginHost.Emit(state, callback);
                    },
                };

            case EntryTypes.List:
                return CreateList(state, node, menuId);

            case EntryTypes.ConfirmList:
                return CreateConfirmList(state, node, menuId);

            case EntryTypes.Slider:
                return CreateSlider(state, node, menuId);

            case EntryTypes.DynamicList:
                return new DynamicListEntry
                {
                    Text = LiveText(state, () => node.Text),
                    Description = LiveText(state, () => node.Description),
                    LockedDescription = LiveText(state, () => node.LockedDescription),
                    Gate = GateFor(state, node),
                    Behaviour = BehaviourFor(node),
                    ReadEnabled = () => node.Enabled != false,
                    ReadLeftIcon = () => ParseIcon(node.LeftIcon),
                    ReadRightIcon = () => ParseIcon(node.RightIcon),
                    OnHighlighted = HighlightFor(state, node, menuId),
                    Configure = item => state.NodesByItem[item] = node,
                    ReadValue = () => node.Value ?? string.Empty,
                    // The plugin lives in another resource, so the next value cannot be produced synchronously. The
                    // current value is kept, the change request goes out as an event, and the plugin answers with a
                    // setValue op one round trip later.
                    Change = changing =>
                    {
                        var callback = Callback(CallbackTypes.DynamicChanging, menuId, node);
                        callback.CurrentValue = changing.CurrentValue;
                        callback.Left = changing.Left;

                        PluginHost.Emit(state, callback);

                        return node.Value ?? changing.CurrentValue;
                    },
                    OnSelected = selected =>
                    {
                        var callback = Callback(CallbackTypes.DynamicSelected, menuId, node);
                        callback.Value = selected.Value;

                        PluginHost.Emit(state, callback);
                    },
                };

            case EntryTypes.Separator:
                return new SeparatorEntry
                {
                    Text = LiveText(state, () => node.Text),
                    Description = LiveText(state, () => node.Description),
                    Gate = GateFor(state, node),
                    Behaviour = BehaviourFor(node),
                    Configure = item => state.NodesByItem[item] = node,
                };

            case EntryTypes.Submenu:
                if (node.Menu is not { } childMenu)
                {
                    return null;
                }

                return new SubmenuEntry
                {
                    Text = LiveText(state, () => node.Text),
                    Description = LiveText(state, () => node.Description),
                    Label = LiveText(state, () => node.Label),
                    LockedDescription = LiveText(state, () => node.LockedDescription),
                    Gate = GateFor(state, node),
                    Behaviour = BehaviourFor(node),
                    ReadEnabled = () => node.Enabled != false,
                    ReadLeftIcon = () => ParseIcon(node.LeftIcon),
                    ReadRightIcon = () => ParseIcon(node.RightIcon),
                    OnHighlighted = HighlightFor(state, node, menuId),
                    Configure = item => state.NodesByItem[item] = node,
                    // Live, so a plugin renaming one of its menus after it connected lands.
                    MenuTitle = LiveText(state, () => childMenu.Title),
                    MenuSubtitle = LiveText(state, () => childMenu.Subtitle),
                    Build = builder => BuildMenu(state, childMenu, builder),
                };

            default:
                return null;
        }
    }

    private static ListEntry CreateList(PluginState state, ItemNode node, string menuId)
    {
        var options = BuildOptions(state, node);

        return new ListEntry
        {
            Text = LiveText(state, () => node.Text),
            Description = LiveText(state, () => node.Description),
            LockedDescription = LiveText(state, () => node.LockedDescription),
            Gate = GateFor(state, node),
            Behaviour = BehaviourFor(node),
            ReadEnabled = () => node.Enabled != false,
            ReadLeftIcon = () => ParseIcon(node.LeftIcon),
            ReadRightIcon = () => ParseIcon(node.RightIcon),
            OnHighlighted = HighlightFor(state, node, menuId),
            Configure = item => state.NodesByItem[item] = node,
            Options = options,
            ReadSelectedIndex = () => Clamp(node.SelectedIndex ?? 0, options.Count),
            OnIndexChanged = changed =>
            {
                node.SelectedIndex = changed.NewIndex;

                var callback = Callback(CallbackTypes.ListIndexChanged, menuId, node);
                callback.OldIndex = changed.OldIndex;
                callback.NewIndex = changed.NewIndex;

                PluginHost.Emit(state, callback);
            },
            OnSelected = selected =>
            {
                var callback = Callback(CallbackTypes.ListSelected, menuId, node);
                callback.SelectedIndex = selected.SelectedIndex;

                PluginHost.Emit(state, callback);
            },
        };
    }

    private static ConfirmListEntry CreateConfirmList(PluginState state, ItemNode node, string menuId)
    {
        var options = BuildOptions(state, node);

        return new ConfirmListEntry
        {
            Text = LiveText(state, () => node.Text),
            Description = LiveText(state, () => node.Description),
            LockedDescription = LiveText(state, () => node.LockedDescription),
            ConfirmationDescription = ConfirmTextFor(state, node),
            Gate = GateFor(state, node),
            Behaviour = BehaviourFor(node),
            ReadEnabled = () => node.Enabled != false,
            ReadLeftIcon = () => ParseIcon(node.LeftIcon),
            ReadRightIcon = () => ParseIcon(node.RightIcon),
            OnHighlighted = HighlightFor(state, node, menuId),
            Configure = item => state.NodesByItem[item] = node,
            Options = options,
            ReadSelectedIndex = () => Clamp(node.SelectedIndex ?? 0, options.Count),
            OnIndexChanged = changed =>
            {
                node.SelectedIndex = changed.NewIndex;

                var callback = Callback(CallbackTypes.ListIndexChanged, menuId, node);
                callback.OldIndex = changed.OldIndex;
                callback.NewIndex = changed.NewIndex;

                PluginHost.Emit(state, callback);
            },
            OnConfirmed = confirmed =>
            {
                var callback = Callback(CallbackTypes.Confirmed, menuId, node);
                callback.SelectedIndex = confirmed.SelectedIndex;

                PluginHost.Emit(state, callback);
            },
        };
    }

    private static SliderEntry CreateSlider(PluginState state, ItemNode node, string menuId)
    {
        var min = node.Min ?? 0;
        var max = node.Max ?? 10;

        return new SliderEntry
        {
            Text = LiveText(state, () => node.Text),
            Description = LiveText(state, () => node.Description),
            LockedDescription = LiveText(state, () => node.LockedDescription),
            Gate = GateFor(state, node),
            Behaviour = BehaviourFor(node),
            ReadEnabled = () => node.Enabled != false,
            ReadLeftIcon = () => ParseIcon(node.LeftIcon),
            ReadRightIcon = () => ParseIcon(node.RightIcon),
            OnHighlighted = HighlightFor(state, node, menuId),
            Configure = item => state.NodesByItem[item] = node,
            Min = min,
            Max = max,
            ShowDivider = node.ShowDivider == true,
            ReadPosition = () => Math.Clamp(node.Position ?? min, min, max),
            OnMoved = moved =>
            {
                node.Position = moved.NewPosition;

                var callback = Callback(CallbackTypes.SliderMoved, menuId, node);
                callback.OldPosition = moved.OldPosition;
                callback.NewPosition = moved.NewPosition;

                PluginHost.Emit(state, callback);
            },
            OnSelected = selected =>
            {
                var callback = Callback(CallbackTypes.SliderSelected, menuId, node);
                callback.Position = selected.Position;

                PluginHost.Emit(state, callback);
            },
        };
    }

    // The live option list an entry shows. Kept in the state so a setOptions op can rewrite it in place,
    // the framework re-reading it on every refresh.
    private static List<MenuText> BuildOptions(PluginState state, ItemNode node)
    {
        var options = new List<MenuText>();

        state.OptionsByItemId[node.Id] = options;

        FillOptions(state, node, options);

        return options;
    }

    internal static void FillOptions(PluginState state, ItemNode node, List<MenuText> options)
    {
        options.Clear();

        if (node.Options is null)
        {
            return;
        }

        foreach (var option in node.Options)
        {
            var reference = option;

            options.Add(MenuText.From(() => state.Resolve(reference)));
        }
    }

    private static PluginCallback Callback(string type, string menuId, ItemNode node) => new()
    {
        Type = type,
        MenuId = menuId,
        ItemId = node.Id,
    };

    private static MenuGate GateFor(PluginState state, ItemNode node) =>
        MenuGate.When(() => state.EvaluateGate(node.Gate));

    private static GateBehaviour? BehaviourFor(ItemNode node) => node.Behaviour?.ToLowerInvariant() switch
    {
        "hide" => GateBehaviour.Hide,
        "lock" => GateBehaviour.Lock,
        _ => null,
    };

    // The plugin's confirmation wording, or the framework's own when it declared none.
    private static MenuText ConfirmTextFor(PluginState state, ItemNode node) =>
        MenuText.From(() => node.ConfirmationDescription is { } wording
            ? state.Resolve(wording)
            : Localizer.Current.Get(Loc.Framework.ConfirmDescription));

    private static Action<MenuItem>? HighlightFor(PluginState state, ItemNode node, string menuId)
    {
        if (node.Events is null || !node.Events.Contains(NodeEvents.Highlighted))
        {
            return null;
        }

        return _ => PluginHost.Emit(state, Callback(CallbackTypes.ItemHighlighted, menuId, node));
    }

    private static int Clamp(int index, int count) => count == 0 ? 0 : Math.Clamp(index, 0, count - 1);

    private static MenuItem.Icon ParseIcon(string? name) =>
        name is { Length: > 0 } && Enum.TryParse<MenuItem.Icon>(name, true, out var icon)
            ? icon
            : MenuItem.Icon.NONE;
}

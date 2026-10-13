using MenuAPI;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.Plugins;

internal static class PluginUpdateOps
{
    internal static void Apply(PluginState state, UpdateBatch batch)
    {
        var changes = new Changes(state);

        try
        {
            foreach (var op in batch.Ops)
            {
                ApplyOne(state, op, changes);
            }
        }
        finally
        {
            changes.Settle();
        }
    }

    private static void ApplyOne(PluginState state, UpdateOp op, Changes changes)
    {
        switch (op.Op)
        {
            case UpdateOps.SetText:
                Present(state, op, changes, static (node, op) => node.Text = op.TextValue);
                break;

            case UpdateOps.SetDescription:
                Present(state, op, changes, static (node, op) => node.Description = op.TextValue);
                break;

            case UpdateOps.SetLabel:
                Present(state, op, changes, static (node, op) => node.Label = op.TextValue);
                break;

            case UpdateOps.SetLockedDescription:
                Present(state, op, changes, static (node, op) => node.LockedDescription = op.TextValue);
                break;

            case UpdateOps.SetConfirmationDescription:
                Present(state, op, changes, static (node, op) => node.ConfirmationDescription = op.TextValue);
                break;

            case UpdateOps.SetIcons:
                Present(state, op, changes, static (node, op) =>
                {
                    node.LeftIcon = op.LeftIcon;
                    node.RightIcon = op.RightIcon;
                });
                break;

            case UpdateOps.SetChecked:
                Present(state, op, changes, static (node, op) => node.Checked = op.Flag ?? false);
                break;

            case UpdateOps.SetSelectedIndex:
                Present(state, op, changes, static (node, op) => node.SelectedIndex = op.Index ?? 0);
                break;

            case UpdateOps.SetSliderPosition:
                Present(state, op, changes, static (node, op) => node.Position = op.Index ?? 0);
                break;

            case UpdateOps.SetValue:
                Present(state, op, changes, static (node, op) => node.Value = op.Value ?? string.Empty);
                break;

            case UpdateOps.SetEnabled:
                Present(state, op, changes, static (node, op) => node.Enabled = op.Flag ?? true);
                break;

            case UpdateOps.SetBehaviour:
                Present(state, op, changes, static (node, op) => node.Behaviour = op.Value);
                break;

            case UpdateOps.SetGate:
                Present(state, op, changes, static (node, op) => node.Gate = op.Gate);
                changes.GatesChanged = true;
                break;

            case UpdateOps.SetOptions:
                if (TryItem(state, op, out var node))
                {
                    node.Options = op.Options ?? [];

                    if (op.Index is { } selected)
                    {
                        node.SelectedIndex = selected;
                    }

                    if (state.OptionsByItemId.TryGetValue(node.Id, out var live))
                    {
                        PluginEntryFactory.FillOptions(state, node, live);
                    }

                    changes.Item(node);
                }

                break;

            case UpdateOps.SetVisible:
                if (TryItem(state, op, out node))
                {
                    node.Visible = op.Flag ?? true;

                    changes.Visibility(node);
                }

                break;

            case UpdateOps.SetLog:
                if (TryItem(state, op, out node))
                {
                    node.Log = op.Flag ?? false;
                }

                break;

            case UpdateOps.SetItemEvents:
                if (TryItem(state, op, out node))
                {
                    node.Events = op.Events;
                }

                break;

            case UpdateOps.SetMenuTitle:
                if (TryMenuNode(state, op, out var menuNode))
                {
                    menuNode.Title = op.TextValue;
                    changes.Menu(menuNode.Id);
                }

                break;

            case UpdateOps.SetMenuSubtitle:
                if (TryMenuNode(state, op, out menuNode))
                {
                    menuNode.Subtitle = op.TextValue;
                    changes.Menu(menuNode.Id);
                }

                break;

            case UpdateOps.SetMenuEvents:
                if (TryMenuNode(state, op, out menuNode))
                {
                    menuNode.Events = op.Events;
                }

                break;

            case UpdateOps.AddItems:
                if (TryMenuNode(state, op, out menuNode) && op.Items is { Count: > 0 })
                {
                    AddItems(state, menuNode, op, changes);
                }

                break;

            case UpdateOps.RemoveItems:
                if (op.ItemIds is { Count: > 0 })
                {
                    RemoveItems(state, op.ItemIds, changes);
                }

                break;

            case UpdateOps.MoveItem:
                if (TryItem(state, op, out node))
                {
                    MoveItem(state, node, op.BeforeItemId, changes);
                }

                break;

            case UpdateOps.ClearMenu:
                if (TryMenuNode(state, op, out menuNode))
                {
                    ClearMenu(state, menuNode);
                }

                break;

            case UpdateOps.SetFilter:
                if (TryMenuNode(state, op, out menuNode))
                {
                    if (op.Flag == true && state.MenuFilters.TryGetValue(menuNode.Id, out var hidden))
                    {
                        foreach (var hiddenId in op.ItemIds ?? [])
                        {
                            hidden.Add(hiddenId);
                        }
                    }
                    else
                    {
                        state.MenuFilters[menuNode.Id] = new HashSet<string>(op.ItemIds ?? [], StringComparer.Ordinal);
                    }

                    changes.Filter(menuNode.Id);
                }

                break;

            case UpdateOps.ClearFilter:
                if (TryMenuNode(state, op, out menuNode) && state.MenuFilters.Remove(menuNode.Id))
                {
                    changes.Filter(menuNode.Id);
                }

                break;

            case UpdateOps.AddPlayerActions:
                if (op.Items is { Count: > 0 })
                {
                    var report = new RegisterResult();

                    foreach (var added in op.Items)
                    {
                        if (PluginValidation.IndexPlayerAction(state, added, report))
                        {
                            state.PlayerActions.Add(added);
                            changes.PlayerActionsChanged = true;
                        }
                    }

                    LogReport(state, report);
                }

                break;

            case UpdateOps.OpenMenu:
                if (TryMenu(state, op, out _, out var builder))
                {
                    changes.OpenAfter = builder;
                    changes.CloseAfter = false;
                }

                break;

            case UpdateOps.SelectItem:
                if (TryItem(state, op, out node)
                    && state.ItemOwners.TryGetValue(node.Id, out var selectOwner)
                    && state.Builders.TryGetValue(selectOwner, out var selectBuilder)
                    && state.EntriesById.TryGetValue(node.Id, out var selectEntry))
                {
                    changes.SelectAfter = (selectBuilder, selectEntry);
                }

                break;

            case UpdateOps.CloseMenu:
                changes.OpenAfter = null;
                changes.CloseAfter = true;
                break;

            case UpdateOps.AddKeys:
                if (TryMenuNode(state, op, out menuNode) && op.Keys is { Count: > 0 })
                {
                    AddKeys(state, menuNode, op.Keys, changes);
                }

                break;

            case UpdateOps.SetKeyText:
                if (TryKey(state, op, out var key, out var keyMenu))
                {
                    key.Text = op.TextValue;
                    changes.Menu(keyMenu);
                }

                break;

            case UpdateOps.SetKeyEnabled:
                if (TryKey(state, op, out key, out keyMenu))
                {
                    key.Enabled = op.Flag ?? true;
                    changes.Menu(keyMenu);
                }

                break;

            case UpdateOps.SetKeyGate:
                if (TryKey(state, op, out key, out keyMenu))
                {
                    key.Gate = op.Gate;
                    changes.Menu(keyMenu);
                }

                break;

            case UpdateOps.MergeTranslations:
                if (op.Language is { Length: > 0 } language && op.Entries is { Count: > 0 } entries)
                {
                    var code = language.Trim().ToLowerInvariant();

                    if (!state.Translations.TryGetValue(code, out var table))
                    {
                        table = new Dictionary<string, string>(StringComparer.Ordinal);
                        state.Translations[code] = table;
                    }

                    foreach (var pair in entries)
                    {
                        table[pair.Key] = pair.Value;
                    }

                    changes.Everything();
                }

                break;

            default:
                Log.Warning($"[Plugins] '{state.Resource}' sent unknown update op '{op.Op}', skipping it.");
                break;
        }
    }

    private static void Present(PluginState state, UpdateOp op, Changes changes, Action<ItemNode, UpdateOp> apply)
    {
        if (TryItem(state, op, out var node))
        {
            apply(node, op);
            changes.Item(node);
        }
    }

    private static void AddItems(PluginState state, MenuNode menuNode, UpdateOp op, Changes changes)
    {
        var report = new RegisterResult();
        var live = state.Builders.TryGetValue(menuNode.Id, out var builder);
        var nodeIndex = AnchorIndex(state, menuNode, op.BeforeItemId);
        var entryIndex = live ? EntryIndex(state, builder!, op.BeforeItemId) : 0;
        var added = false;

        foreach (var node in op.Items!)
        {
            if (node.Id is { } nodeId && state.ItemOwners.TryGetValue(nodeId, out var owner) && owner == menuNode.Id)
            {
                Log.Debug($"[Plugins] '{state.Resource}': item '{node.Id}' is already in menu '{menuNode.Id}', the repeat was skipped.");

                continue;
            }

            if (!PluginValidation.IndexLateItem(state, node, menuNode.Id, report))
            {
                continue;
            }

            menuNode.Items.Insert(nodeIndex++, node);
            added = true;

            if (live && PluginEntryFactory.CreateEntry(state, node, menuNode.Id) is { } entry)
            {
                changes.Structure(builder!);
                builder!.Insert(entryIndex++, entry);
            }
        }

        LogReport(state, report);

        if (added && !live)
        {
            PluginHost.MaterialiseRow(state);
        }
    }

    private static void RemoveItems(PluginState state, List<string> ids, Changes changes)
    {
        foreach (var id in ids)
        {
            if (id is null)
            {
                continue;
            }

            if (!state.ItemsById.TryGetValue(id, out var node) || !state.ItemOwners.TryGetValue(id, out var owner))
            {
                Log.Warning($"[Plugins] '{state.Resource}' asked to remove unknown item '{id}'.");
                continue;
            }

            if (owner == PluginState.PlayerActionsMenuId)
            {
                RemoveByReference(state.PlayerActions, node);
                PluginValidation.Unindex(state, node);

                changes.PlayerActionsChanged = true;

                continue;
            }

            if (state.MenusById.TryGetValue(owner, out var menuNode))
            {
                RemoveByReference(menuNode.Items, node);
            }

            if (state.Builders.TryGetValue(owner, out var builder) && state.EntriesById.TryGetValue(id, out var entry))
            {
                changes.Structure(builder);
                builder.Remove(entry);
            }

            if (state.MenuFilters.TryGetValue(owner, out var filtered))
            {
                filtered.Remove(id);
            }

            PluginValidation.Unindex(state, node);
        }
    }

    private static void MoveItem(PluginState state, ItemNode node, string? beforeItemId, Changes changes)
    {
        if (!state.ItemOwners.TryGetValue(node.Id, out var owner)
            || !state.MenusById.TryGetValue(owner, out var menuNode)
            || beforeItemId == node.Id)
        {
            return;
        }

        if (beforeItemId is not null
            && (!state.ItemOwners.TryGetValue(beforeItemId, out var anchorOwner) || anchorOwner != owner))
        {
            Log.Warning($"[Plugins] '{state.Resource}' tried to move '{node.Id}' next to '{beforeItemId}', which is not in the same menu.");
            return;
        }

        RemoveByReference(menuNode.Items, node);
        menuNode.Items.Insert(AnchorIndex(state, menuNode, beforeItemId), node);

        if (!state.Builders.TryGetValue(owner, out var builder) || !state.EntriesById.TryGetValue(node.Id, out var entry))
        {
            return;
        }

        var from = IndexOf(builder.Entries, entry);
        var to = EntryIndex(state, builder, beforeItemId);

        changes.Structure(builder);
        builder.Move(entry, to > from ? to - 1 : to);
    }

    private static void ClearMenu(PluginState state, MenuNode menuNode)
    {
        foreach (var cleared in menuNode.Items)
        {
            PluginValidation.Unindex(state, cleared);
        }

        menuNode.Items.Clear();

        state.MenuFilters.Remove(menuNode.Id);

        if (state.Builders.TryGetValue(menuNode.Id, out var builder))
        {
            state.ForgetItemsOf(builder);
            builder.ClearEntries();
        }
    }

    private static void AddKeys(PluginState state, MenuNode menuNode, List<KeyNode> keys, Changes changes)
    {
        var report = new RegisterResult();
        var live = state.Builders.TryGetValue(menuNode.Id, out var builder);

        foreach (var key in keys)
        {
            if (!PluginValidation.IndexLateKey(state, key, report))
            {
                continue;
            }

            menuNode.Keys ??= [];
            menuNode.Keys.Add(key);

            if (live)
            {
                builder!.Keys.Add(PluginEntryFactory.CreateKey(state, key, menuNode.Id));
                changes.Menu(menuNode.Id);
            }
        }

        LogReport(state, report);
    }

    private static int AnchorIndex(PluginState state, MenuNode menuNode, string? beforeItemId)
    {
        if (beforeItemId is not null && state.ItemsById.TryGetValue(beforeItemId, out var anchor))
        {
            for (var index = 0; index < menuNode.Items.Count; index++)
            {
                if (ReferenceEquals(menuNode.Items[index], anchor))
                {
                    return index;
                }
            }
        }

        return menuNode.Items.Count;
    }

    private static int EntryIndex(PluginState state, MenuBuilder builder, string? beforeItemId)
    {
        if (beforeItemId is not null
            && state.EntriesById.TryGetValue(beforeItemId, out var anchor)
            && IndexOf(builder.Entries, anchor) is >= 0 and var index)
        {
            return index;
        }

        return builder.Entries.Count;
    }

    private static int IndexOf(List<MenuEntry> entries, MenuEntry entry)
    {
        for (var index = 0; index < entries.Count; index++)
        {
            if (ReferenceEquals(entries[index], entry))
            {
                return index;
            }
        }

        return -1;
    }

    private static void RemoveByReference(List<ItemNode> nodes, ItemNode node)
    {
        for (var index = nodes.Count - 1; index >= 0; index--)
        {
            if (ReferenceEquals(nodes[index], node))
            {
                nodes.RemoveAt(index);
                return;
            }
        }
    }

    private static void LogReport(PluginState state, RegisterResult report)
    {
        foreach (var warning in report.Warnings)
        {
            Log.Warning($"[Plugins] '{state.Resource}': {warning}");
        }

        foreach (var error in report.Errors)
        {
            Log.Warning($"[Plugins] '{state.Resource}': {error}");
        }
    }

    private static bool TryItem(PluginState state, UpdateOp op, out ItemNode node)
    {
        if (op.ItemId is { Length: > 0 } id && state.ItemsById.TryGetValue(id, out var found))
        {
            node = found;
            return true;
        }

        Log.Warning($"[Plugins] '{state.Resource}' targeted unknown item '{op.ItemId}' with op '{op.Op}'.");

        node = null!;
        return false;
    }

    private static bool TryKey(PluginState state, UpdateOp op, out KeyNode key, out string menuId)
    {
        if (op.KeyId is { Length: > 0 } id && state.KeysById.TryGetValue(id, out var found))
        {
            key = found;
            menuId = string.Empty;

            foreach (var menu in state.MenusById.Values)
            {
                if (menu.Keys is { } keys && keys.Exists(candidate => ReferenceEquals(candidate, found)))
                {
                    menuId = menu.Id;
                    break;
                }
            }

            return true;
        }

        Log.Warning($"[Plugins] '{state.Resource}' targeted unknown key '{op.KeyId}' with op '{op.Op}'.");

        key = null!;
        menuId = string.Empty;
        return false;
    }

    private static bool TryMenuNode(PluginState state, UpdateOp op, out MenuNode menu)
    {
        if (op.MenuId is { Length: > 0 } id && state.MenusById.TryGetValue(id, out var foundMenu))
        {
            menu = foundMenu;
            return true;
        }

        Log.Warning($"[Plugins] '{state.Resource}' targeted unknown menu '{op.MenuId}' with op '{op.Op}'.");

        menu = null!;
        return false;
    }

    private static bool TryMenu(PluginState state, UpdateOp op, out MenuNode menu, out MenuBuilder builder)
    {
        if (op.MenuId is { Length: > 0 } id
            && state.MenusById.TryGetValue(id, out var foundMenu)
            && state.Builders.TryGetValue(id, out var foundBuilder))
        {
            menu = foundMenu;
            builder = foundBuilder;
            return true;
        }

        Log.Warning($"[Plugins] '{state.Resource}' targeted unknown menu '{op.MenuId}' with op '{op.Op}'.");

        menu = null!;
        builder = null!;
        return false;
    }

    private sealed class Changes(PluginState state)
    {
        private readonly HashSet<string> _refresh = new(StringComparer.Ordinal);

        private readonly HashSet<string> _filter = new(StringComparer.Ordinal);

        private readonly List<(MenuBuilder Builder, IDisposable Update)> _updates = [];

        private bool _everything;

        internal bool GatesChanged { get; set; }

        internal bool PlayerActionsChanged { get; set; }

        internal MenuBuilder? OpenAfter { get; set; }

        internal bool CloseAfter { get; set; }

        internal (MenuBuilder Builder, MenuEntry Entry)? SelectAfter { get; set; }

        internal void Item(ItemNode node)
        {
            if (state.PlayerActionIds.Contains(node.Id))
            {
                PlayerActionsChanged = true;
            }
            else if (state.ItemOwners.TryGetValue(node.Id, out var owner))
            {
                _refresh.Add(owner);
            }
        }

        internal void Visibility(ItemNode node)
        {
            if (state.PlayerActionIds.Contains(node.Id))
            {
                PlayerActionsChanged = true;
            }
            else if (state.ItemOwners.TryGetValue(node.Id, out var owner))
            {
                _filter.Add(owner);
            }
        }

        internal void Menu(string menuId)
        {
            if (menuId.Length > 0)
            {
                _refresh.Add(menuId);
            }
        }

        internal void Filter(string menuId) => _filter.Add(menuId);

        internal void Everything() => _everything = true;

        internal void Structure(MenuBuilder builder)
        {
            foreach (var (open, _) in _updates)
            {
                if (ReferenceEquals(open, builder))
                {
                    return;
                }
            }

            _updates.Add((builder, builder.BeginUpdate()));
        }

        internal void Settle()
        {
            if (_everything)
            {
                foreach (var menuId in state.Builders.Keys)
                {
                    _refresh.Add(menuId);
                }

                PluginHost.RefreshRow(state);

                PlayerActionsChanged |= state.PlayerActions.Count > 0;
            }

            foreach (var menuId in _refresh)
            {
                if (state.Builders.TryGetValue(menuId, out var builder))
                {
                    Structure(builder);
                    builder.Invalidate();
                }
            }

            foreach (var menuId in _filter)
            {
                if (state.Builders.TryGetValue(menuId, out var builder))
                {
                    Structure(builder);
                    builder.InvalidateFilter();
                }
            }

            foreach (var (_, update) in _updates)
            {
                update.Dispose();
            }

            if (GatesChanged)
            {
                MenuRegistry.BackOutIfUnreachable();
            }

            if (CloseAfter && MenuController.GetCurrentMenu() is { } open && IsOwn(open))
            {
                open.CloseMenu();
            }

            if (OpenAfter is { } target)
            {
                MenuController.CloseAllMenus();
                target.Menu.OpenMenu();
            }

            if (SelectAfter is { } select)
            {
                select.Builder.Select(select.Entry);
            }

            if (PlayerActionsChanged)
            {
                PluginHost.RaiseChanged();
            }
        }

        private bool IsOwn(Menu menu)
        {
            foreach (var builder in state.Builders.Values)
            {
                if (ReferenceEquals(builder.Menu, menu))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

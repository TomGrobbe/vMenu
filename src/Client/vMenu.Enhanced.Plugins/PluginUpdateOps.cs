using MenuAPI;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.Plugins;

// Applies an update batch to a plugin's live tree. Most operations are a node mutation followed by
// one shared refresh at the end. Structural operations rebuild the owning menu on the spot, which
// already refreshes the whole tree.
internal static class PluginUpdateOps
{
    internal static void Apply(PluginState state, UpdateBatch batch)
    {
        // Set by a presentation op and cleared by a structural one, which refreshes the whole tree as it
        // materialises. Tracked in op order, so a rename after an insert still lands.
        var dirty = false;
        var filtersDirty = false;
        var playerActionsChanged = false;

        foreach (var op in batch.Ops)
        {
            switch (op.Op)
            {
                case UpdateOps.SetText:
                    if (TryItem(state, op, out var node))
                    {
                        node.Text = op.TextValue;
                        dirty = true;
                    }

                    break;

                case UpdateOps.SetDescription:
                    if (TryItem(state, op, out node))
                    {
                        node.Description = op.TextValue;
                        dirty = true;
                    }

                    break;

                case UpdateOps.SetLabel:
                    if (TryItem(state, op, out node))
                    {
                        node.Label = op.TextValue;
                        dirty = true;
                    }

                    break;

                case UpdateOps.SetLockedDescription:
                    if (TryItem(state, op, out node))
                    {
                        node.LockedDescription = op.TextValue;
                        dirty = true;
                    }

                    break;

                case UpdateOps.SetConfirmationDescription:
                    if (TryItem(state, op, out node))
                    {
                        node.ConfirmationDescription = op.TextValue;
                        dirty = true;
                    }

                    break;

                case UpdateOps.SetIcons:
                    if (TryItem(state, op, out node))
                    {
                        node.LeftIcon = op.LeftIcon;
                        node.RightIcon = op.RightIcon;
                        dirty = true;
                    }

                    break;

                case UpdateOps.SetChecked:
                    if (TryItem(state, op, out node))
                    {
                        node.Checked = op.Flag ?? false;
                        dirty = true;
                    }

                    break;

                case UpdateOps.SetOptions:
                    if (TryItem(state, op, out node))
                    {
                        node.Options = op.Options ?? new List<TextRef>();

                        if (op.Index is { } selected)
                        {
                            node.SelectedIndex = selected;
                        }

                        if (state.OptionsByItemId.TryGetValue(node.Id, out var live))
                        {
                            PluginEntryFactory.FillOptions(state, node, live);
                        }

                        dirty = true;
                    }

                    break;

                case UpdateOps.SetSelectedIndex:
                    if (TryItem(state, op, out node))
                    {
                        node.SelectedIndex = op.Index ?? 0;
                        dirty = true;
                    }

                    break;

                case UpdateOps.SetSliderPosition:
                    if (TryItem(state, op, out node))
                    {
                        node.Position = op.Index ?? 0;
                        dirty = true;
                    }

                    break;

                case UpdateOps.SetValue:
                    if (TryItem(state, op, out node))
                    {
                        node.Value = op.Value ?? string.Empty;
                        dirty = true;
                    }

                    break;

                case UpdateOps.SetVisible:
                    if (TryItem(state, op, out node))
                    {
                        node.Visible = op.Flag ?? true;
                        dirty = true;
                        filtersDirty = true;

                        if (state.PlayerActionIds.Contains(node.Id))
                        {
                            playerActionsChanged = true;
                        }
                    }

                    break;

                case UpdateOps.SetEnabled:
                    if (TryItem(state, op, out node))
                    {
                        node.Enabled = op.Flag ?? true;
                        dirty = true;
                    }

                    break;

                case UpdateOps.SetLog:
                    if (TryItem(state, op, out node))
                    {
                        node.Log = op.Flag ?? false;
                    }

                    break;

                case UpdateOps.SetGate:
                    if (TryItem(state, op, out node))
                    {
                        node.Gate = op.Gate;
                        dirty = true;

                        if (state.PlayerActionIds.Contains(node.Id))
                        {
                            playerActionsChanged = true;
                        }
                    }

                    break;

                case UpdateOps.SetMenuTitle:
                    if (TryMenu(state, op, out var menuNode, out _))
                    {
                        menuNode.Title = op.TextValue;
                        dirty = true;
                    }

                    break;

                case UpdateOps.SetMenuSubtitle:
                    if (TryMenu(state, op, out menuNode, out _))
                    {
                        menuNode.Subtitle = op.TextValue;
                        dirty = true;
                    }

                    break;

                case UpdateOps.AddItems:
                    if (TryMenuNode(state, op, out menuNode) && op.Items is { Count: > 0 })
                    {
                        var report = new RegisterResult();
                        var entries = new List<MenuEntry>();

                        // A menu the plugin declared while empty has no row and so no live menu behind it yet. Its nodes are
                        // still indexed, so the rows land on the node now and the whole tree is materialised once at the end.
                        var live = state.Builders.TryGetValue(menuNode.Id, out var target);

                        foreach (var added in op.Items)
                        {
                            if (!PluginValidation.IndexLateItem(state, added, menuNode.Id, report))
                            {
                                continue;
                            }

                            menuNode.Items.Add(added);

                            if (live && PluginEntryFactory.CreateEntry(state, added, menuNode.Id) is { } entry)
                            {
                                entries.Add(entry);
                            }
                        }

                        LogReport(state, report);

                        if (entries.Count > 0)
                        {
                            target!.AddRange(entries);
                            dirty = false;
                            filtersDirty = true;
                        }
                        else if (!live && menuNode.Items.Count > 0)
                        {
                            PluginHost.MaterialiseRows();
                            dirty = false;
                            filtersDirty = true;
                        }
                    }

                    break;

                case UpdateOps.RemoveItems:
                    if (op.ItemIds is { Count: > 0 })
                    {
                        if (RemoveItems(state, op.ItemIds, ref playerActionsChanged))
                        {
                            dirty = false;
                        }

                        filtersDirty = true;
                    }

                    break;

                case UpdateOps.ClearMenu:
                    if (TryMenu(state, op, out menuNode, out var builder))
                    {
                        foreach (var cleared in menuNode.Items)
                        {
                            PluginValidation.Unindex(state, cleared);
                        }

                        menuNode.Items.Clear();

                        state.ForgetItemsOf(builder);
                        builder.ClearEntries();
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
                                playerActionsChanged = true;
                            }
                        }

                        LogReport(state, report);
                    }

                    break;

                case UpdateOps.OpenMenu:
                    if (TryMenu(state, op, out _, out builder))
                    {
                        builder.Menu.OpenMenu();
                    }

                    break;

                case UpdateOps.CloseMenu:
                    if (MenuController.GetCurrentMenu() is { } open && IsOwn(state, open))
                    {
                        open.CloseMenu();
                    }

                    break;

                case UpdateOps.MergeTranslations:
                    if (op.Language is { Length: > 0 } language && op.Entries is { Count: > 0 } entriesToMerge)
                    {
                        var code = language.Trim().ToLowerInvariant();

                        if (!state.Translations.TryGetValue(code, out var table))
                        {
                            table = new Dictionary<string, string>(StringComparer.Ordinal);
                            state.Translations[code] = table;
                        }

                        foreach (var pair in entriesToMerge)
                        {
                            table[pair.Key] = pair.Value;
                        }

                        dirty = true;
                    }

                    break;

                default:
                    Log.Warning($"[Plugins] '{state.Resource}' sent unknown update op '{op.Op}', skipping it.");
                    break;
            }
        }

        // Only when something changed after the last structural op, those refreshing as they go.
        if (dirty)
        {
            MenuRegistry.RefreshAll();
        }

        if (filtersDirty)
        {
            foreach (var menuId in state.Builders.Keys.ToList())
            {
                PluginHost.ReapplyFilter(state, menuId);
            }
        }

        if (playerActionsChanged)
        {
            PluginHost.RaiseChanged();
        }
    }

    private static bool RemoveItems(PluginState state, List<string> ids, ref bool playerActionsChanged)
    {
        var byMenu = new Dictionary<string, List<ItemNode>>(StringComparer.Ordinal);

        foreach (var id in ids)
        {
            if (!state.ItemsById.TryGetValue(id, out var node)
                || !state.ItemOwners.TryGetValue(id, out var owner))
            {
                Log.Warning($"[Plugins] '{state.Resource}' asked to remove unknown item '{id}'.");
                continue;
            }

            if (!byMenu.TryGetValue(owner, out var group))
            {
                group = [];
                byMenu[owner] = group;
            }

            group.Add(node);
        }

        var rebuilt = false;

        foreach (var pair in byMenu)
        {
            if (pair.Key == PluginState.PlayerActionsMenuId)
            {
                foreach (var node in pair.Value)
                {
                    RemoveByReference(state.PlayerActions, node);
                    PluginValidation.Unindex(state, node);
                }

                playerActionsChanged = true;
                continue;
            }

            if (!state.MenusById.TryGetValue(pair.Key, out var menuNode)
                || !state.Builders.TryGetValue(pair.Key, out var builder))
            {
                continue;
            }

            foreach (var node in pair.Value)
            {
                RemoveByReference(menuNode.Items, node);
                PluginValidation.Unindex(state, node);
            }

            // MenuAPI has no removal of a single row, so the menu is rebuilt from the survivors.
            state.ForgetItemsOf(builder);
            builder.ClearEntries();

            var entries = new List<MenuEntry>();

            foreach (var survivor in menuNode.Items)
            {
                if (PluginEntryFactory.CreateEntry(state, survivor, menuNode.Id) is { } entry)
                {
                    entries.Add(entry);
                }
            }

            builder.AddRange(entries);

            rebuilt = true;
        }

        return rebuilt;
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

    private static bool IsOwn(PluginState state, Menu menu)
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

    // The declared menu alone, for an op that works whether or not it is on screen yet.
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
}

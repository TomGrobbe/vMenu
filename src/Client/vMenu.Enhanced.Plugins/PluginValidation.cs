using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.Plugins;

/// <summary>
/// Checks a declared tree before anything is materialised. Structural problems refuse the
/// registration, per item problems skip the item with a warning so the rest still shows.
/// </summary>
internal static class PluginValidation
{
    internal const int MaxItems = 2000;

    internal const int MaxDepth = 8;

    private static readonly HashSet<string> PlayerActionTypes = new(StringComparer.Ordinal)
    {
        EntryTypes.Button,
        EntryTypes.ConfirmButton,
        EntryTypes.List,
        EntryTypes.Separator,
    };

    internal static bool IndexMenuTree(PluginState state, MenuNode menu, RegisterResult result) =>
        IndexMenu(state, menu, 1, result);

    /// <summary>Whether the plugin's live tree is already as big as it may get.</summary>
    // Read off the live index rather than counted per call, so items added after registration count
    // towards the same ceiling as the ones declared with it.
    private static bool AtCapacity(PluginState state) => state.ItemsById.Count >= MaxItems;

    private static bool IndexMenu(PluginState state, MenuNode menu, int depth, RegisterResult result)
    {
        if (string.IsNullOrEmpty(menu.Id))
        {
            result.Errors.Add("A menu has no id.");
            return false;
        }

        if (!state.MenusById.TryAdd(menu.Id, menu))
        {
            result.Errors.Add($"Menu id '{menu.Id}' is used twice.");
            return false;
        }

        menu.Items ??= new List<ItemNode>();

        // Recorded so an item added to this menu later knows how deep it already sits, the late path
        // having no walk down from the root to count with.
        state.MenuDepths[menu.Id] = depth;

        // Forwards, and the index only moves on for an item that survived. Walking backwards would
        // be simpler to remove from, but then the last of two rows sharing an id would be the one
        // indexed and the first would be dropped, which is the opposite of what the warning says.
        for (var index = 0; index < menu.Items.Count;)
        {
            var node = menu.Items[index];

            if (AtCapacity(state))
            {
                result.Errors.Add($"The menu tree holds more than {MaxItems} items.");
                return false;
            }

            if (!IndexItem(state, node, menu.Id, result))
            {
                menu.Items.RemoveAt(index);
                continue;
            }

            if (node.Type == EntryTypes.Submenu && node.Menu is { } child)
            {
                if (depth + 1 > MaxDepth)
                {
                    result.Warnings.Add($"Submenu '{node.Id}' was skipped: menus nest deeper than {MaxDepth} levels.");
                    Unindex(state, node);
                    menu.Items.RemoveAt(index);
                    continue;
                }

                if (!IndexMenu(state, child, depth + 1, result))
                {
                    return false;
                }
            }

            index++;
        }

        return true;
    }

    /// <summary>Validates one item and records it. Answers whether it may stay.</summary>
    internal static bool IndexItem(PluginState state, ItemNode node, string menuId, RegisterResult result)
    {
        if (string.IsNullOrEmpty(node.Id))
        {
            result.Warnings.Add("An item without an id was skipped.");
            return false;
        }

        if (state.ItemsById.ContainsKey(node.Id))
        {
            result.Warnings.Add($"Item id '{node.Id}' is used twice, the second one was skipped.");
            return false;
        }

        switch (node.Type)
        {
            case EntryTypes.Button:
            case EntryTypes.ConfirmButton:
            case EntryTypes.Checkbox:
            case EntryTypes.DynamicList:
            case EntryTypes.Separator:
                break;

            case EntryTypes.List:
            case EntryTypes.ConfirmList:
                if (node.Options is not { Count: > 0 })
                {
                    result.Warnings.Add($"List '{node.Id}' was skipped: it has no options.");
                    return false;
                }

                break;

            case EntryTypes.Slider:
                if ((node.Min ?? 0) >= (node.Max ?? 0))
                {
                    result.Warnings.Add($"Slider '{node.Id}' was skipped: min must be smaller than max.");
                    return false;
                }

                break;

            case EntryTypes.Submenu:
                if (node.Menu is null)
                {
                    result.Warnings.Add($"Submenu '{node.Id}' was skipped: it declares no menu.");
                    return false;
                }

                break;

            default:
                result.Warnings.Add($"Item '{node.Id}' was skipped: unknown type '{node.Type}'.");
                return false;
        }

        state.ItemsById[node.Id] = node;
        state.ItemOwners[node.Id] = menuId;

        return true;
    }

    internal static bool IndexPlayerAction(PluginState state, ItemNode node, RegisterResult result)
    {
        if (AtCapacity(state))
        {
            result.Warnings.Add($"Player action '{node.Id}' was skipped: the plugin is at its ceiling of {MaxItems} items.");
            return false;
        }

        if (!PlayerActionTypes.Contains(node.Type))
        {
            result.Warnings.Add(
                $"Player action '{node.Id}' was skipped: type '{node.Type}' cannot be shared across "
                + "players, use button, confirmButton, list or separator.");
            return false;
        }

        if (!IndexItem(state, node, PluginState.PlayerActionsMenuId, result))
        {
            return false;
        }

        state.PlayerActionIds.Add(node.Id);

        return true;
    }

    /// <summary>
    /// Validates and records an item added after registration, including a submenu's whole
    /// subtree. Answers whether it may stay.
    /// </summary>
    internal static bool IndexLateItem(PluginState state, ItemNode node, string menuId, RegisterResult result)
    {
        if (AtCapacity(state))
        {
            result.Warnings.Add($"Item '{node.Id}' was skipped: the plugin is at its ceiling of {MaxItems} items.");
            return false;
        }

        // Where the menu it joins already sits, so its own children are measured from the root like
        // everything declared at registration was.
        var depth = state.MenuDepths.TryGetValue(menuId, out var owner) ? owner : 1;

        if (node.Type == EntryTypes.Submenu && depth + 1 > MaxDepth)
        {
            result.Warnings.Add($"Submenu '{node.Id}' was skipped: menus nest deeper than {MaxDepth} levels.");
            return false;
        }

        if (!IndexItem(state, node, menuId, result))
        {
            return false;
        }

        if (node.Type != EntryTypes.Submenu || node.Menu is not { } menu)
        {
            return true;
        }

        if (IndexMenu(state, menu, depth + 1, result))
        {
            return true;
        }

        Unindex(state, node);

        return false;
    }

    /// <summary>Forgets an item and, for a submenu, everything below it.</summary>
    internal static void Unindex(PluginState state, ItemNode node)
    {
        state.ItemsById.Remove(node.Id);
        state.ItemOwners.Remove(node.Id);
        state.PlayerActionIds.Remove(node.Id);
        state.OptionsByItemId.Remove(node.Id);

        if (node.Type != EntryTypes.Submenu || node.Menu is not { } menu)
        {
            return;
        }

        state.MenusById.Remove(menu.Id);
        state.MenuDepths.Remove(menu.Id);

        if (state.Builders.Remove(menu.Id, out var builder))
        {
            state.ForgetItemsOf(builder);
        }

        foreach (var child in menu.Items)
        {
            Unindex(state, child);
        }
    }
}

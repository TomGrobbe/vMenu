using MenuAPI;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.PluginContracts;

using PluginPermissions = vMenu.Enhanced.Data.Permissions.Plugins;

namespace vMenu.Enhanced.Plugins;

// Entries close over the nodes in here, which is what makes an update op a mutation plus a refresh.
internal sealed class PluginState
{
    // Pseudo menu id player action items are owned by, since no plugin menu holds them.
    internal const string PlayerActionsMenuId = "playerActions";

    internal PluginState(string resource, string id)
    {
        Resource = resource;
        Id = id;
        EventName = PluginEvents.EventFor(resource);
    }

    internal string Resource { get; }

    internal string Id { get; }

    // The per resource event interactions are emitted on.
    internal string EventName { get; }

    internal TextRef? DisplayName { get; set; }

    // Extra line under the resource name in the plugin's row, as a key reference.
    internal TextRef? DescriptionRef { get; set; }

    // Language code to key to text, language codes lowercased.
    internal Dictionary<string, Dictionary<string, string>> Translations { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    // Declared settings by short name, for gates and value reads.
    internal Dictionary<string, BoolSetting> BoolSettings { get; } = new(StringComparer.OrdinalIgnoreCase);

    internal MenuNode? RootMenu { get; set; }

    internal List<ItemNode> PlayerActions { get; } = [];

    internal Dictionary<string, ItemNode> ItemsById { get; } = new(StringComparer.Ordinal);

    // Item id to the id of the menu whose row it is.
    internal Dictionary<string, string> ItemOwners { get; } = new(StringComparer.Ordinal);

    internal HashSet<string> PlayerActionIds { get; } = new(StringComparer.Ordinal);

    internal Dictionary<string, MenuNode> MenusById { get; } = new(StringComparer.Ordinal);

    // How deep each menu sits, the root being 1. Read when an item is added to it later.
    internal Dictionary<string, int> MenuDepths { get; } = new(StringComparer.Ordinal);

    // Live builders by menu id, filled in as each menu's build action runs.
    internal Dictionary<string, MenuBuilder> Builders { get; } = new(StringComparer.Ordinal);

    // The materialised item back to its node, which is what the visibility filter reads.
    internal Dictionary<MenuItem, ItemNode> NodesByItem { get; } = new(ReferenceComparer<MenuItem>.Instance);

    // The live option lists by item id, mutated in place by a setOptions op.
    internal Dictionary<string, List<MenuText>> OptionsByItemId { get; } = new(StringComparer.Ordinal);

    // Hides rows whose node says invisible. Handed to every one of this plugin's menus.
    internal bool VisibilityFilter(MenuItem item) =>
        !NodesByItem.TryGetValue(item, out var node) || node.Visible != false;

    // ClearEntries discards the live items, including those of the rows that survive and are made again.
    // Without this every rebuild would leave a dead item behind in the map the filter reads, which for a
    // menu built from runtime data is once every refresh.
    internal void ForgetItemsOf(MenuBuilder builder)
    {
        foreach (var entry in builder.Entries)
        {
            if (entry.Item is { } item)
            {
                NodesByItem.Remove(item);
            }
        }
    }

    // Forgets every live object so a re-registration starts from a clean slate.
    internal void ResetLiveState()
    {
        ItemsById.Clear();
        ItemOwners.Clear();
        PlayerActionIds.Clear();
        MenusById.Clear();
        MenuDepths.Clear();
        Builders.Clear();
        NodesByItem.Clear();
        OptionsByItemId.Clear();
        PlayerActions.Clear();
    }

    // The current vMenu language first, then the plugin's English table, then a loud marker, mirroring
    // vMenu's own fallback.
    internal string Resolve(TextRef? reference)
    {
        if (reference is null)
        {
            return string.Empty;
        }

        if (reference.Key is { Length: > 0 } key)
        {
            return PluginPlaceholders.Substitute(Lookup(key), reference.Args, this);
        }

        return reference.Text ?? string.Empty;
    }

    private string Lookup(string key)
    {
        var current = Localizer.Current.CurrentLanguage.Code;

        if (Translations.TryGetValue(current, out var table) && table.TryGetValue(key, out var text))
        {
            return text;
        }

        if (Translations.TryGetValue("en", out var english) && english.TryGetValue(key, out var fallback))
        {
            return fallback;
        }

        return "!!" + key + "!!";
    }

    // Fail closed on anything malformed, matching MenuGate's own policy.
    internal bool EvaluateGate(GateNode? gate)
    {
        if (gate is null)
        {
            return true;
        }

        if (gate.Permission is { Length: > 0 } permission)
        {
            return ClientPermissions.IsAllowed(PluginPermissions.For(Id, permission));
        }

        if (gate.Setting is { Length: > 0 } setting)
        {
            return BoolSettings.TryGetValue(setting, out var declared) && ClientConfig.Value(declared);
        }

        if (gate.All is { Count: > 0 } all)
        {
            foreach (var child in all)
            {
                if (!EvaluateGate(child))
                {
                    return false;
                }
            }

            return true;
        }

        if (gate.Any is { Count: > 0 } any)
        {
            foreach (var child in any)
            {
                if (EvaluateGate(child))
                {
                    return true;
                }
            }

            return false;
        }

        return false;
    }
}

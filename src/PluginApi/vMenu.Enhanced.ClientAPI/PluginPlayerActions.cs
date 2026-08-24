using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.ClientAPI;

public sealed class PluginPlayerButton : PluginItem
{
    internal PluginPlayerButton(ItemNode node)
        : base(node)
    {
    }

    /// <summary>Raised when the action is used on a player, with that player as the target.</summary>
    public event Action<PlayerTarget>? Selected;

    internal override void Handle(PluginCallback callback)
    {
        base.Handle(callback);

        if (callback.Type == CallbackTypes.PlayerActionSelected && Target(callback) is { } target)
        {
            Selected?.Invoke(target);
        }
    }

    internal static PlayerTarget? Target(PluginCallback callback) =>
        callback.TargetServerId is { } serverId
            ? new PlayerTarget(serverId, callback.TargetName ?? string.Empty)
            : null;
}

public sealed class PluginPlayerConfirmButton : PluginItem
{
    private Text _confirmationDescription;

    internal PluginPlayerConfirmButton(ItemNode node)
        : base(node)
    {
    }

    /// <summary>What the row asks before its second press. Empty uses vMenu's own wording.</summary>
    public Text ConfirmationDescription
    {
        get => _confirmationDescription;
        set
        {
            _confirmationDescription = value;
            Node.ConfirmationDescription = value.ToRef();
            Emit(new UpdateOp
            {
                Op = UpdateOps.SetConfirmationDescription,
                ItemId = Id,
                TextValue = Node.ConfirmationDescription,
            });
        }
    }

    /// <summary>Raised on the confirming second press, with the targeted player.</summary>
    public event Action<PlayerTarget>? Confirmed;

    internal override void Handle(PluginCallback callback)
    {
        base.Handle(callback);

        if (callback.Type == CallbackTypes.PlayerActionConfirmed && PluginPlayerButton.Target(callback) is { } target)
        {
            Confirmed?.Invoke(target);
        }
    }
}

public sealed class PluginPlayerList : PluginItem
{
    internal PluginPlayerList(ItemNode node)
        : base(node)
    {
    }

    /// <summary>The current selection. Shared across every player the menu shows, since the same rows
    /// serve them all.</summary>
    public int SelectedIndex
    {
        get => Node.SelectedIndex ?? 0;
        set
        {
            Node.SelectedIndex = value;
            Emit(new UpdateOp { Op = UpdateOps.SetSelectedIndex, ItemId = Id, Index = value });
        }
    }

    public void SetOptions(IEnumerable<Text> options, int? selectedIndex = null)
    {
        Node.Options = PluginList.ToRefs(options);

        if (selectedIndex is { } index)
        {
            Node.SelectedIndex = index;
        }

        Emit(new UpdateOp
        {
            Op = UpdateOps.SetOptions,
            ItemId = Id,
            Options = Node.Options,
            Index = selectedIndex,
        });
    }

    /// <summary>Raised when the action is used on a player, with the target and the chosen index.</summary>
    public event Action<PlayerTarget, int>? Selected;

    internal override void Handle(PluginCallback callback)
    {
        base.Handle(callback);

        if (callback.Type == CallbackTypes.PlayerActionListSelected
            && PluginPlayerButton.Target(callback) is { } target)
        {
            if (callback.SelectedIndex is { } selected)
            {
                Node.SelectedIndex = selected;
            }

            Selected?.Invoke(target, callback.SelectedIndex ?? 0);
        }
    }
}

/// <summary>Actions vMenu injects into the "Plugin Actions" submenu of every player's entry in its
/// Online Players menu. The same rows serve every player, and the target is handed to your callback
/// when one fires. This is the one place a plugin reaches outside its own menu tree.</summary>
public sealed class PluginPlayerActions
{
    private readonly VMenuPlugin _plugin;

    private readonly List<PluginItem> _items = new();

    internal PluginPlayerActions(VMenuPlugin plugin) => _plugin = plugin;

    internal List<ItemNode> Nodes { get; } = new();

    public IReadOnlyList<PluginItem> Items => _items;

    public PluginPlayerButton AddButton(Text text, string? id = null) =>
        Attach(new PluginPlayerButton(NewNode(EntryTypes.Button, text, id)));

    public PluginPlayerConfirmButton AddConfirmButton(Text text, string? id = null) =>
        Attach(new PluginPlayerConfirmButton(NewNode(EntryTypes.ConfirmButton, text, id)));

    public PluginPlayerList AddList(Text text, IEnumerable<Text> options, int selectedIndex = 0, string? id = null)
    {
        var node = NewNode(EntryTypes.List, text, id);
        node.Options = PluginList.ToRefs(options);
        node.SelectedIndex = selectedIndex;

        return Attach(new PluginPlayerList(node));
    }

    public PluginSeparator AddSeparator(Text text, string? id = null) =>
        Attach(new PluginSeparator(NewNode(EntryTypes.Separator, text, id)));

    /// <summary>Removes one action from every player's entry.</summary>
    public void Remove(PluginItem item)
    {
        for (var index = _items.Count - 1; index >= 0; index--)
        {
            if (!ReferenceEquals(_items[index], item))
            {
                continue;
            }

            _items.RemoveAt(index);

            for (var nodeIndex = Nodes.Count - 1; nodeIndex >= 0; nodeIndex--)
            {
                if (ReferenceEquals(Nodes[nodeIndex], item.Node))
                {
                    Nodes.RemoveAt(nodeIndex);
                    break;
                }
            }

            _plugin.UnregisterItem(item);

            _plugin.EmitOp(new UpdateOp { Op = UpdateOps.RemoveItems, ItemIds = new List<string> { item.Id } });

            return;
        }
    }

    private ItemNode NewNode(string type, Text text, string? id) => new()
    {
        Id = id ?? _plugin.NextItemId(),
        Type = type,
        Text = text.ToRef(),
    };

    private T Attach<T>(T item)
        where T : PluginItem
    {
        item.Plugin = _plugin;

        _items.Add(item);
        Nodes.Add(item.Node);

        _plugin.RegisterItem(item);

        _plugin.EmitOp(new UpdateOp
        {
            Op = UpdateOps.AddPlayerActions,
            Items = new List<ItemNode> { item.Node },
        });

        return item;
    }
}

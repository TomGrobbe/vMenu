using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.ClientAPI;

/// <summary>One of your plugin's menus: the root under your row in vMenu's Plugins menu, or a
/// submenu. Rows added before connecting ride along with the registration, rows added later appear
/// live.</summary>
public sealed class PluginMenu
{
    private readonly VMenuPlugin _plugin;

    private readonly List<PluginItem> _items = new();

    private Text _title;

    private Text _subtitle;

    private Action? _opened;

    private Action? _closed;

    private Action<int, int>? _indexChanged;

    internal PluginMenu(VMenuPlugin plugin, MenuNode node)
    {
        _plugin = plugin;
        Node = node;
    }

    internal MenuNode Node { get; }

    public string Id => Node.Id;

    public IReadOnlyList<PluginItem> Items => _items;

    public Text Title
    {
        get => _title;
        set
        {
            _title = value;
            Node.Title = value.ToRef();
            _plugin.EmitOp(new UpdateOp { Op = UpdateOps.SetMenuTitle, MenuId = Id, TextValue = Node.Title });
        }
    }

    public Text Subtitle
    {
        get => _subtitle;
        set
        {
            _subtitle = value;
            Node.Subtitle = value.ToRef();
            _plugin.EmitOp(new UpdateOp { Op = UpdateOps.SetMenuSubtitle, MenuId = Id, TextValue = Node.Subtitle });
        }
    }

    /// <summary>Raised when the player opens this menu.</summary>
    public event Action? Opened
    {
        add
        {
            _opened += value;
            SubscribeMenuEvent(NodeEvents.Opened);
        }
        remove => _opened -= value;
    }

    /// <summary>Raised when the player leaves this menu, including into a submenu.</summary>
    public event Action? Closed
    {
        add
        {
            _closed += value;
            SubscribeMenuEvent(NodeEvents.Closed);
        }
        remove => _closed -= value;
    }

    /// <summary>Raised when the cursor moves, with the old and new row index. Chatty.</summary>
    public event Action<int, int>? IndexChanged
    {
        add
        {
            _indexChanged += value;
            SubscribeMenuEvent(NodeEvents.IndexChanged);
        }
        remove => _indexChanged -= value;
    }

    public PluginButton AddButton(Text text, string? id = null) =>
        Attach(new PluginButton(NewNode(EntryTypes.Button, text, id)));

    public PluginConfirmButton AddConfirmButton(Text text, string? id = null) =>
        Attach(new PluginConfirmButton(NewNode(EntryTypes.ConfirmButton, text, id)));

    /// <summary>Adds a checkbox. With persist on, the player's choice is saved in this resource's key
    /// value store and restored on the next start. Pass a stable id along with persist: the automatic
    /// ids follow creation order, so reordering your code would hand a saved value to the wrong box.</summary>
    public PluginCheckbox AddCheckbox(Text text, bool initiallyChecked = false, string? id = null, bool persist = false)
    {
        var node = NewNode(EntryTypes.Checkbox, text, id);
        node.Checked = initiallyChecked;

        var checkbox = new PluginCheckbox(node);

        if (persist)
        {
            checkbox.Persisted = true;

            if (PluginPreferences.ReadBool(node.Id) is { } stored)
            {
                node.Checked = stored;
            }
        }

        return Attach(checkbox);
    }

    public PluginList AddList(Text text, IEnumerable<Text> options, int selectedIndex = 0, string? id = null)
    {
        var node = NewNode(EntryTypes.List, text, id);
        node.Options = PluginList.ToRefs(options);
        node.SelectedIndex = selectedIndex;

        return Attach(new PluginList(node));
    }

    public PluginConfirmList AddConfirmList(Text text, IEnumerable<Text> options, int selectedIndex = 0, string? id = null)
    {
        var node = NewNode(EntryTypes.ConfirmList, text, id);
        node.Options = PluginList.ToRefs(options);
        node.SelectedIndex = selectedIndex;

        return Attach(new PluginConfirmList(node));
    }

    public PluginSlider AddSlider(Text text, int min, int max, int position, bool showDivider = false, string? id = null)
    {
        var node = NewNode(EntryTypes.Slider, text, id);
        node.Min = min;
        node.Max = max;
        node.Position = position;
        node.ShowDivider = showDivider;

        return Attach(new PluginSlider(node));
    }

    public PluginDynamicList AddDynamicList(Text text, string initialValue, string? id = null)
    {
        var node = NewNode(EntryTypes.DynamicList, text, id);
        node.Value = initialValue;

        return Attach(new PluginDynamicList(node));
    }

    public PluginSeparator AddSeparator(Text text, string? id = null) =>
        Attach(new PluginSeparator(NewNode(EntryTypes.Separator, text, id)));

    /// <summary>Adds a row that opens a new menu, returned through the item's
    /// <see cref="PluginSubmenu.Menu"/>. The title falls back to the row's text when left empty.</summary>
    public PluginSubmenu AddSubmenu(Text text, Text title = default, Text subtitle = default, string? id = null)
    {
        var node = NewNode(EntryTypes.Submenu, text, id);

        node.Menu = new MenuNode
        {
            Id = _plugin.NextMenuId(),
            Title = (title.IsEmpty ? text : title).ToRef(),
            Subtitle = subtitle.ToRef(),
        };

        var menu = new PluginMenu(_plugin, node.Menu);

        _plugin.RegisterMenu(menu);

        return Attach(new PluginSubmenu(node, menu));
    }

    /// <summary>Removes one row. For a submenu row, everything beneath it goes too.</summary>
    public void Remove(PluginItem item)
    {
        if (!RemoveLocal(item))
        {
            return;
        }

        _plugin.EmitOp(new UpdateOp { Op = UpdateOps.RemoveItems, ItemIds = new List<string> { item.Id } });
    }

    /// <summary>Removes every row.</summary>
    public void Clear()
    {
        foreach (var item in _items)
        {
            _plugin.UnregisterItem(item);
        }

        _items.Clear();
        Node.Items.Clear();

        _plugin.EmitOp(new UpdateOp { Op = UpdateOps.ClearMenu, MenuId = Id });
    }

    /// <summary>Opens this menu on screen, closing whatever vMenu menu was open.</summary>
    public void Open() => _plugin.EmitOp(new UpdateOp { Op = UpdateOps.OpenMenu, MenuId = Id });

    /// <summary>Closes this plugin's menu if one is open.</summary>
    public void Close() => _plugin.EmitOp(new UpdateOp { Op = UpdateOps.CloseMenu, MenuId = Id });

    internal void HandleMenu(PluginCallback callback)
    {
        switch (callback.Type)
        {
            case CallbackTypes.MenuOpened:
                _opened?.Invoke();
                break;

            case CallbackTypes.MenuClosed:
                _closed?.Invoke();
                break;

            case CallbackTypes.MenuIndexChanged when callback.NewIndex is { } newIndex:
                _indexChanged?.Invoke(callback.OldIndex ?? 0, newIndex);
                break;
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
        Node.Items.Add(item.Node);

        _plugin.RegisterItem(item);

        _plugin.EmitOp(new UpdateOp
        {
            Op = UpdateOps.AddItems,
            MenuId = Id,
            Items = new List<ItemNode> { item.Node },
        });

        return item;
    }

    private bool RemoveLocal(PluginItem item)
    {
        for (var index = _items.Count - 1; index >= 0; index--)
        {
            if (!ReferenceEquals(_items[index], item))
            {
                continue;
            }

            _items.RemoveAt(index);

            for (var nodeIndex = Node.Items.Count - 1; nodeIndex >= 0; nodeIndex--)
            {
                if (ReferenceEquals(Node.Items[nodeIndex], item.Node))
                {
                    Node.Items.RemoveAt(nodeIndex);
                    break;
                }
            }

            _plugin.UnregisterItem(item);

            return true;
        }

        return false;
    }

    private void SubscribeMenuEvent(string name)
    {
        Node.Events ??= new List<string>();

        if (!Node.Events.Contains(name))
        {
            Node.Events.Add(name);
            _plugin.ReRegisterIfConnected();
        }
    }
}

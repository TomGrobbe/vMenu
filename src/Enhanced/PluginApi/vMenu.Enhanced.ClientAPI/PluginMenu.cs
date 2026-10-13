using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.ClientAPI;

/// <summary>One of your plugin's menus: the root under your row in vMenu's Plugins menu, or a
/// submenu. Rows added before connecting ride along with the registration, rows added later appear
/// live.</summary>
public sealed class PluginMenu
{
    private readonly VMenuPlugin _plugin;

    private readonly List<PluginItem> _items = [];

    private readonly List<PluginKey> _keys = [];

    private Text _title;

    private Text _subtitle;

    private Action? _opened;

    private Action? _closed;

    private Action<int, int>? _indexChanged;

    private int? _insertAt;

    private Func<PluginItem, bool>? _filter;

    internal PluginMenu(VMenuPlugin plugin, MenuNode node)
    {
        _plugin = plugin;
        Node = node;
    }

    internal MenuNode Node { get; }

    public string Id => Node.Id;

    public IReadOnlyList<PluginItem> Items => _items;

    public IReadOnlyList<PluginKey> Keys => _keys;

    /// <summary>Whether <see cref="Filter"/> is hiding rows right now.</summary>
    public bool IsFiltered => _filter is not null;

    public Text Title
    {
        get => _title;
        set
        {
            _title = value;

            var title = value.ToRef();

            if (PluginDiff.Same(Node.Title, title))
            {
                return;
            }

            Node.Title = title;
            _plugin.EmitOp(new UpdateOp { Op = UpdateOps.SetMenuTitle, MenuId = Id, TextValue = title });
        }
    }

    public Text Subtitle
    {
        get => _subtitle;
        set
        {
            _subtitle = value;

            var subtitle = value.ToRef();

            if (PluginDiff.Same(Node.Subtitle, subtitle))
            {
                return;
            }

            Node.Subtitle = subtitle;
            _plugin.EmitOp(new UpdateOp { Op = UpdateOps.SetMenuSubtitle, MenuId = Id, TextValue = subtitle });
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

    /// <summary>Adds a key that works while this menu is open, with an instructional button at the bottom
    /// of the screen. Keep the id stable: it names the binding in the player's key settings, so changing
    /// it loses a key they picked themselves.</summary>
    /// <param name="id">Letters, digits and underscores, unique within your plugin.</param>
    /// <param name="text">The instructional button's label.</param>
    /// <param name="defaultKey">A keyboard key name as the game knows it, for example "X" or "F5".</param>
    /// <param name="defaultButton">An optional controller button, for example "RUP_INDEX".</param>
    /// <param name="description">What the key settings list it as. Falls back to the text.</param>
    /// <param name="shadowedControl">A game control index to suppress while the menu is open, for a
    /// default key the game already uses.</param>
    public PluginKey AddKey(
        string id,
        Text text,
        string defaultKey,
        string? defaultButton = null,
        Text description = default,
        int? shadowedControl = null)
    {
        var node = new KeyNode
        {
            Id = id,
            Text = text.ToRef(),
            Description = description.ToRef(),
            DefaultKey = defaultKey,
            DefaultButton = defaultButton,
            ShadowedControl = shadowedControl,
        };

        Node.Keys ??= [];
        Node.Keys.Add(node);

        var key = new PluginKey(_plugin, node, text);

        _keys.Add(key);
        _plugin.RegisterKey(key);
        _plugin.EmitOp(new UpdateOp { Op = UpdateOps.AddKeys, MenuId = Id, Keys = [node] });

        return key;
    }

    /// <summary>Rows added inside the returned scope go in at <paramref name="index"/>, one after another,
    /// instead of at the bottom. Dispose it to go back to adding at the bottom.</summary>
    public IDisposable InsertAt(int index)
    {
        var previous = _insertAt;

        _insertAt = Math.Clamp(index, 0, _items.Count);

        return new InsertScope(this, previous);
    }

    /// <summary>Moves a row of this menu to <paramref name="index"/>. A submenu row keeps its menu, and the
    /// highlighted row stays highlighted.</summary>
    public void Move(PluginItem item, int index)
    {
        var from = IndexOf(item);

        if (from < 0)
        {
            return;
        }

        index = Math.Clamp(index, 0, _items.Count - 1);

        if (index == from)
        {
            return;
        }

        _items.RemoveAt(from);
        _items.Insert(index, item);

        Node.Items.RemoveAt(from);
        Node.Items.Insert(index, item.Node);

        var before = index + 1 < _items.Count ? _items[index + 1].Id : null;

        _plugin.EmitOp(new UpdateOp { Op = UpdateOps.MoveItem, ItemId = item.Id, BeforeItemId = before });
    }

    /// <summary>Shows only the rows <paramref name="keep"/> answers true for. Rows added later are checked too:
    /// inside a batch when the batch ends, so properties set right after adding count, otherwise as they come in.
    /// Call it again after changing what it looks at.</summary>
    public void Filter(Func<PluginItem, bool> keep)
    {
        _filter = keep;

        _plugin.FilterChanged(this);
    }

    /// <summary>Shows every row again after <see cref="Filter"/>.</summary>
    public void ClearFilter()
    {
        if (_filter is null)
        {
            return;
        }

        _filter = null;

        _plugin.FilterChanged(this);
    }

    internal bool HasFilter => _filter is not null;

    internal bool Hides(PluginItem item) => _filter is { } keep && !keep(item);

    internal UpdateOp FilterOp()
    {
        if (_filter is not { } keep)
        {
            return new UpdateOp { Op = UpdateOps.ClearFilter, MenuId = Id };
        }

        var hidden = new List<string>();

        foreach (var item in _items)
        {
            if (!keep(item))
            {
                hidden.Add(item.Id);
            }
        }

        return new UpdateOp { Op = UpdateOps.SetFilter, MenuId = Id, ItemIds = hidden };
    }

    /// <summary>Removes one row. For a submenu row, everything beneath it goes too.</summary>
    public void Remove(PluginItem item)
    {
        if (!RemoveLocal(item))
        {
            return;
        }

        _plugin.EmitOp(new UpdateOp { Op = UpdateOps.RemoveItems, ItemIds = [item.Id] });
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

    /// <summary>Moves the cursor to a row of this menu. Raises <c>Highlighted</c> and <c>IndexChanged</c> as if
    /// the player had moved there. Does nothing for a hidden row or one from another menu.</summary>
    public void Select(PluginItem item)
    {
        if (IndexOf(item) >= 0)
        {
            _plugin.EmitOp(new UpdateOp { Op = UpdateOps.SelectItem, MenuId = Id, ItemId = item.Id });
        }
    }

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

        string? before = null;

        if (_insertAt is { } index && index < _items.Count)
        {
            before = _items[index].Id;

            _items.Insert(index, item);
            Node.Items.Insert(index, item.Node);

            _insertAt = index + 1;
        }
        else
        {
            _items.Add(item);
            Node.Items.Add(item.Node);

            if (_insertAt is not null)
            {
                _insertAt = _items.Count;
            }
        }

        _plugin.RegisterItem(item);

        _plugin.EmitAdd(
            this,
            item,
            new UpdateOp
            {
                Op = UpdateOps.AddItems,
                MenuId = Id,
                Items = [item.Node],
                BeforeItemId = before,
            });

        return item;
    }

    private int IndexOf(PluginItem item)
    {
        for (var index = 0; index < _items.Count; index++)
        {
            if (ReferenceEquals(_items[index], item))
            {
                return index;
            }
        }

        return -1;
    }

    internal static ItemNode AsAdded(ItemNode node)
    {
        if (node.Menu is not { } menu)
        {
            return node;
        }

        var copy = node.CopyRow();

        copy.Menu = new MenuNode
        {
            Id = menu.Id,
            Title = menu.Title,
            Subtitle = menu.Subtitle,
            Events = menu.Events,
            Keys = menu.Keys,
        };

        return copy;
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
        Node.Events ??= [];

        if (Node.Events.Exists(existing => string.Equals(existing, name, StringComparison.Ordinal)))
        {
            return;
        }

        Node.Events.Add(name);
        _plugin.EmitOp(new UpdateOp { Op = UpdateOps.SetMenuEvents, MenuId = Id, Events = [.. Node.Events] });
    }

    private sealed class InsertScope(PluginMenu menu, int? previous) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            menu._insertAt = previous;
        }
    }
}

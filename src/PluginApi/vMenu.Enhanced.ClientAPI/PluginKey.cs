using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.ClientAPI;

/// <summary>A key that works while its menu is open, shown as an instructional button at the bottom of
/// the screen. The player can rebind it in the game's key settings, where it is listed under vMenu
/// with your plugin's name in front.</summary>
public sealed class PluginKey
{
    private readonly VMenuPlugin _plugin;

    private Text _text;

    private PluginGate? _gate;

    internal PluginKey(VMenuPlugin plugin, KeyNode node, Text text)
    {
        _plugin = plugin;
        _text = text;
        Node = node;
    }

    internal KeyNode Node { get; }

    public string Id => Node.Id;

    /// <summary>The instructional button's label. Empty hides the button, the key still works.</summary>
    public Text Text
    {
        get => _text;
        set
        {
            _text = value;

            var text = value.ToRef();

            if (PluginDiff.Same(Node.Text, text))
            {
                return;
            }

            Node.Text = text;
            _plugin.EmitOp(new UpdateOp { Op = UpdateOps.SetKeyText, KeyId = Id, TextValue = text });
        }
    }

    /// <summary>A disabled key does nothing and shows no button.</summary>
    public bool Enabled
    {
        get => Node.Enabled != false;
        set
        {
            if (Enabled == value)
            {
                return;
            }

            Node.Enabled = value;
            _plugin.EmitOp(new UpdateOp { Op = UpdateOps.SetKeyEnabled, KeyId = Id, Flag = value });
        }
    }

    /// <summary>While the gate fails the key does nothing and shows no button.</summary>
    public PluginGate? Gate
    {
        get => _gate;
        set
        {
            _gate = value;

            var gate = value?.ToNode();

            if (PluginDiff.Same(Node.Gate, gate))
            {
                return;
            }

            Node.Gate = gate;
            _plugin.EmitOp(new UpdateOp { Op = UpdateOps.SetKeyGate, KeyId = Id, Gate = gate });
        }
    }

    /// <summary>Raised when the player presses the key, with the row the cursor was on.</summary>
    public event Action<PluginKeyPress>? Pressed;

    internal void Handle(PluginKeyPress press) => Pressed?.Invoke(press);
}

/// <summary>Which row the cursor was on when a key was pressed. At most one of the two is set, and
/// neither when the menu is empty or the row is not one of yours.</summary>
public sealed class PluginKeyPress
{
    internal PluginKeyPress(PluginItem? item, PluginItem? disabledItem)
    {
        Item = item;
        DisabledItem = disabledItem;
    }

    /// <summary>The row under the cursor, when the player may use it.</summary>
    public PluginItem? Item { get; }

    /// <summary>The row under the cursor, when it is locked by its gate or disabled. Acting on it
    /// anyway is your call.</summary>
    public PluginItem? DisabledItem { get; }
}

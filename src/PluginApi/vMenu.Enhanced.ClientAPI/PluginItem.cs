using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.ClientAPI;

/// <summary>One row in your plugin's menus. Setting a property updates the row live once the plugin
/// is connected, and everything set before connecting rides along with the registration.</summary>
public abstract class PluginItem
{
    private Text _text;

    private Text _description;

    private Text _label;

    private Text _lockedDescription;

    private PluginGate? _gate;

    private Action? _highlighted;

    internal PluginItem(ItemNode node) => Node = node;

    internal ItemNode Node { get; }

    internal VMenuPlugin? Plugin { get; set; }

    public string Id => Node.Id;

    public Text Text
    {
        get => _text;
        set
        {
            _text = value;
            Node.Text = value.ToRef();
            EmitText(UpdateOps.SetText, Node.Text);
        }
    }

    public Text Description
    {
        get => _description;
        set
        {
            _description = value;
            Node.Description = value.ToRef();
            EmitText(UpdateOps.SetDescription, Node.Description);
        }
    }

    /// <summary>Right aligned text. Ignored by rows whose label the menu draws itself.</summary>
    public Text Label
    {
        get => _label;
        set
        {
            _label = value;
            Node.Label = value.ToRef();
            EmitText(UpdateOps.SetLabel, Node.Label);
        }
    }

    /// <summary>What the row says while its gate locks it. Empty uses vMenu's own wording.</summary>
    public Text LockedDescription
    {
        get => _lockedDescription;
        set
        {
            _lockedDescription = value;
            Node.LockedDescription = value.ToRef();
            EmitText(UpdateOps.SetLockedDescription, Node.LockedDescription);
        }
    }

    public PluginGate? Gate
    {
        get => _gate;
        set
        {
            _gate = value;
            Node.Gate = value?.ToNode();
            Emit(new UpdateOp { Op = UpdateOps.SetGate, ItemId = Id, Gate = Node.Gate });
        }
    }

    /// <summary>What a failing gate does to the row: greyed out with a lock, or gone entirely.</summary>
    public bool HideWhenLocked
    {
        get => string.Equals(Node.Behaviour, "hide", StringComparison.OrdinalIgnoreCase);
        set
        {
            Node.Behaviour = value ? "hide" : "lock";
            Plugin?.ReRegisterIfConnected();
        }
    }

    public bool Visible
    {
        get => Node.Visible != false;
        set
        {
            Node.Visible = value;
            Emit(new UpdateOp { Op = UpdateOps.SetVisible, ItemId = Id, Flag = value });
        }
    }

    /// <summary>A disabled row is greyed out but still visible. Independent of the gate.</summary>
    public bool Enabled
    {
        get => Node.Enabled != false;
        set
        {
            Node.Enabled = value;
            Emit(new UpdateOp { Op = UpdateOps.SetEnabled, ItemId = Id, Flag = value });
        }
    }

    /// <summary>Icon names from the vMenu icon set, for example "LOCK" or "STAR".</summary>
    public void SetIcons(string? leftIcon, string? rightIcon)
    {
        Node.LeftIcon = leftIcon;
        Node.RightIcon = rightIcon;

        Emit(new UpdateOp { Op = UpdateOps.SetIcons, ItemId = Id, LeftIcon = leftIcon, RightIcon = rightIcon });
    }

    /// <summary>Raised while the player's cursor sits on this row. Chatty, subscribe deliberately.</summary>
    public event Action? Highlighted
    {
        add
        {
            _highlighted += value;
            SubscribeNodeEvent(NodeEvents.Highlighted);
        }
        remove => _highlighted -= value;
    }

    internal virtual void Handle(PluginCallback callback)
    {
        if (callback.Type == CallbackTypes.ItemHighlighted)
        {
            _highlighted?.Invoke();
        }
    }

    private protected void SubscribeNodeEvent(string name)
    {
        Node.Events ??= new List<string>();

        if (!Node.Events.Contains(name))
        {
            Node.Events.Add(name);
            Plugin?.ReRegisterIfConnected();
        }
    }

    private protected void Emit(UpdateOp op) => Plugin?.EmitOp(op);

    private void EmitText(string opName, TextRef? value) =>
        Emit(new UpdateOp { Op = opName, ItemId = Id, TextValue = value });
}

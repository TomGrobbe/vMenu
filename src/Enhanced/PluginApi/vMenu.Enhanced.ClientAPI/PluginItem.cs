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
            SetText(Node.Text, value.ToRef(), reference => Node.Text = reference, UpdateOps.SetText);
        }
    }

    public Text Description
    {
        get => _description;
        set
        {
            _description = value;
            SetText(Node.Description, value.ToRef(), reference => Node.Description = reference, UpdateOps.SetDescription);
        }
    }

    /// <summary>Right aligned text. Ignored by rows whose label the menu draws itself.</summary>
    public Text Label
    {
        get => _label;
        set
        {
            _label = value;
            SetText(Node.Label, value.ToRef(), reference => Node.Label = reference, UpdateOps.SetLabel);
        }
    }

    /// <summary>What the row says while its gate locks it. Empty uses vMenu's own wording.</summary>
    public Text LockedDescription
    {
        get => _lockedDescription;
        set
        {
            _lockedDescription = value;
            SetText(Node.LockedDescription, value.ToRef(), reference => Node.LockedDescription = reference, UpdateOps.SetLockedDescription);
        }
    }

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
            Emit(new UpdateOp { Op = UpdateOps.SetGate, ItemId = Id, Gate = gate });
        }
    }

    /// <summary>What a failing gate does to the row: greyed out with a lock, or gone entirely.</summary>
    public bool HideWhenLocked
    {
        get => string.Equals(Node.Behaviour, "hide", StringComparison.OrdinalIgnoreCase);
        set
        {
            var behaviour = value ? "hide" : "lock";

            if (string.Equals(Node.Behaviour, behaviour, StringComparison.Ordinal))
            {
                return;
            }

            Node.Behaviour = behaviour;
            Emit(new UpdateOp { Op = UpdateOps.SetBehaviour, ItemId = Id, Value = behaviour });
        }
    }

    public bool Visible
    {
        get => Node.Visible != false;
        set
        {
            if (Visible == value)
            {
                return;
            }

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
            if (Enabled == value)
            {
                return;
            }

            Node.Enabled = value;
            Emit(new UpdateOp { Op = UpdateOps.SetEnabled, ItemId = Id, Flag = value });
        }
    }

    /// <summary>Ask vMenu to log use of this row to the server owner's webhook. Does nothing on its own:
    /// the plugin's server half has to declare the same id with <c>AddLoggedItem</c>.</summary>
    public bool Log
    {
        get => Node.Log == true;
        set
        {
            if (Log == value)
            {
                return;
            }

            Node.Log = value;
            Emit(new UpdateOp { Op = UpdateOps.SetLog, ItemId = Id, Flag = value });
        }
    }

    /// <summary>Icon names from the vMenu icon set, for example "LOCK" or "STAR".</summary>
    public void SetIcons(string? leftIcon, string? rightIcon)
    {
        if (string.Equals(Node.LeftIcon, leftIcon, StringComparison.Ordinal)
            && string.Equals(Node.RightIcon, rightIcon, StringComparison.Ordinal))
        {
            return;
        }

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
        Node.Events ??= [];

        if (Node.Events.Exists(existing => string.Equals(existing, name, StringComparison.Ordinal)))
        {
            return;
        }

        Node.Events.Add(name);
        Emit(new UpdateOp { Op = UpdateOps.SetItemEvents, ItemId = Id, Events = [.. Node.Events] });
    }

    private protected void Emit(UpdateOp op) => Plugin?.EmitOp(op);

    private protected void SetText(TextRef? current, TextRef? next, Action<TextRef?> store, string opName)
    {
        if (PluginDiff.Same(current, next))
        {
            return;
        }

        store(next);
        Emit(new UpdateOp { Op = opName, ItemId = Id, TextValue = next });
    }
}

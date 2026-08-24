using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.ClientAPI;

public sealed class PluginButton : PluginItem
{
    internal PluginButton(ItemNode node)
        : base(node)
    {
    }

    /// <summary>Raised when the player presses the row.</summary>
    public event Action? Selected;

    internal override void Handle(PluginCallback callback)
    {
        base.Handle(callback);

        if (callback.Type == CallbackTypes.ItemSelected)
        {
            Selected?.Invoke();
        }
    }
}

public sealed class PluginConfirmButton : PluginItem
{
    private Text _confirmationDescription;

    internal PluginConfirmButton(ItemNode node)
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

    /// <summary>Raised on the confirming second press, never on the first.</summary>
    public event Action? Confirmed;

    internal override void Handle(PluginCallback callback)
    {
        base.Handle(callback);

        if (callback.Type == CallbackTypes.Confirmed)
        {
            Confirmed?.Invoke();
        }
    }
}

public sealed class PluginCheckbox : PluginItem
{
    internal PluginCheckbox(ItemNode node)
        : base(node)
    {
    }

    /// <summary>Whether the state is saved in this resource's key value store and restored on start.</summary>
    public bool Persisted { get; internal set; }

    public bool Checked
    {
        get => Node.Checked == true;
        set
        {
            Node.Checked = value;
            Remember(value);
            Emit(new UpdateOp { Op = UpdateOps.SetChecked, ItemId = Id, Flag = value });
        }
    }

    /// <summary>Raised when the player toggles the box. The new state is already in <see cref="Checked"/>.</summary>
    public event Action<bool>? Changed;

    internal override void Handle(PluginCallback callback)
    {
        base.Handle(callback);

        if (callback.Type == CallbackTypes.CheckboxChanged && callback.Checked is { } state)
        {
            Node.Checked = state;
            Remember(state);
            Changed?.Invoke(state);
        }
    }

    private void Remember(bool state)
    {
        if (Persisted)
        {
            PluginPreferences.WriteBool(Id, state);
        }
    }
}

public class PluginList : PluginItem
{
    internal PluginList(ItemNode node)
        : base(node)
    {
    }

    public int SelectedIndex
    {
        get => Node.SelectedIndex ?? 0;
        set
        {
            Node.SelectedIndex = value;
            Emit(new UpdateOp { Op = UpdateOps.SetSelectedIndex, ItemId = Id, Index = value });
        }
    }

    /// <summary>Replaces the options, optionally moving the selection at the same time.</summary>
    public void SetOptions(IEnumerable<Text> options, int? selectedIndex = null)
    {
        Node.Options = ToRefs(options);

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

    /// <summary>Raised when the player scrolls the value. The new index is already in <see cref="SelectedIndex"/>.</summary>
    public event Action<int, int>? IndexChanged;

    /// <summary>Raised when the player presses the row, with the index they had selected.</summary>
    public event Action<int>? Selected;

    internal override void Handle(PluginCallback callback)
    {
        base.Handle(callback);

        switch (callback.Type)
        {
            case CallbackTypes.ListIndexChanged when callback.NewIndex is { } newIndex:
                Node.SelectedIndex = newIndex;
                IndexChanged?.Invoke(callback.OldIndex ?? 0, newIndex);
                break;

            case CallbackTypes.ListSelected when callback.SelectedIndex is { } selected:
                RaiseSelected(selected);
                break;

            case CallbackTypes.Confirmed when callback.SelectedIndex is { } confirmed:
                RaiseConfirmed(confirmed);
                break;
        }
    }

    private protected virtual void RaiseSelected(int index) => Selected?.Invoke(index);

    private protected virtual void RaiseConfirmed(int index)
    {
    }

    internal static List<TextRef> ToRefs(IEnumerable<Text> options)
    {
        var refs = new List<TextRef>();

        foreach (var option in options)
        {
            if (option.ToRef() is { } reference)
            {
                refs.Add(reference);
            }
        }

        return refs;
    }
}

public sealed class PluginConfirmList : PluginList
{
    private Text _confirmationDescription;

    internal PluginConfirmList(ItemNode node)
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

    /// <summary>Raised on the confirming second press, with the index that was confirmed.</summary>
    public event Action<int>? Confirmed;

    private protected override void RaiseConfirmed(int index) => Confirmed?.Invoke(index);
}

public sealed class PluginSlider : PluginItem
{
    internal PluginSlider(ItemNode node)
        : base(node)
    {
    }

    public int Min => Node.Min ?? 0;

    public int Max => Node.Max ?? 0;

    public int Position
    {
        get => Node.Position ?? Min;
        set
        {
            Node.Position = value;
            Emit(new UpdateOp { Op = UpdateOps.SetSliderPosition, ItemId = Id, Index = value });
        }
    }

    /// <summary>Raised while the player drags the bar. The new position is already in <see cref="Position"/>.</summary>
    public event Action<int, int>? Moved;

    /// <summary>Raised when the player presses the row, with the position it sat at.</summary>
    public event Action<int>? Selected;

    internal override void Handle(PluginCallback callback)
    {
        base.Handle(callback);

        switch (callback.Type)
        {
            case CallbackTypes.SliderMoved when callback.NewPosition is { } newPosition:
                Node.Position = newPosition;
                Moved?.Invoke(callback.OldPosition ?? 0, newPosition);
                break;

            case CallbackTypes.SliderSelected when callback.Position is { } position:
                Selected?.Invoke(position);
                break;
        }
    }
}

public sealed class PluginDynamicList : PluginItem
{
    internal PluginDynamicList(ItemNode node)
        : base(node)
    {
    }

    public string Value
    {
        get => Node.Value ?? string.Empty;
        set
        {
            Node.Value = value;
            Emit(new UpdateOp { Op = UpdateOps.SetValue, ItemId = Id, Value = value });
        }
    }

    /// <summary>Produces the next value when the player scrolls: current value and whether they went
    /// left. The answer crosses back to vMenu as an update, so it lands one beat after the press.</summary>
    public Func<string, bool, string>? ChangeRequested { get; set; }

    /// <summary>Raised when the player presses the row, with the value it showed.</summary>
    public event Action<string>? Selected;

    internal override void Handle(PluginCallback callback)
    {
        base.Handle(callback);

        switch (callback.Type)
        {
            case CallbackTypes.DynamicChanging when callback.Left is { } left:
                if (ChangeRequested?.Invoke(callback.CurrentValue ?? string.Empty, left) is { } next)
                {
                    Value = next;
                }

                break;

            case CallbackTypes.DynamicSelected:
                Selected?.Invoke(callback.Value ?? string.Empty);
                break;
        }
    }
}

public sealed class PluginSeparator : PluginItem
{
    internal PluginSeparator(ItemNode node)
        : base(node)
    {
    }
}

public sealed class PluginSubmenu : PluginItem
{
    internal PluginSubmenu(ItemNode node, PluginMenu menu)
        : base(node) => Menu = menu;

    /// <summary>The menu this row opens. Add its rows through this.</summary>
    public PluginMenu Menu { get; }
}

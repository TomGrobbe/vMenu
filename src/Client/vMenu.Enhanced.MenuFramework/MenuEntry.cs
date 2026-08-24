using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

// Non generic so a MenuHost can hold every entry in one list and dispatch without knowing the item
// type. An entry declares its item rather than holding a pre-made one, which is what lets the
// framework rewrite text on a language change and the description on a permission change.
public abstract class MenuEntry
{
    public required MenuText Text { get; init; }

    public MenuText Description { get; init; }

    // Right aligned text. Ignored for list, slider, checkbox and dynamic list items, whose label MenuAPI
    // rewrites on every frame it draws them.
    public MenuText Label { get; init; }

    public MenuGate Gate { get; init; } = MenuGate.Always;

    // What to say while this entry is locked. Empty falls back to the framework's own wording.
    public MenuText LockedDescription { get; init; }

    // Null inherits the menu's default, which inherits MenuFrameworkOptions.
    public GateBehaviour? Behaviour { get; init; }

    public MenuItem.Icon LeftIcon { get; init; } = MenuItem.Icon.NONE;

    public MenuItem.Icon RightIcon { get; init; } = MenuItem.Icon.NONE;

    public Func<MenuItem.Icon>? ReadLeftIcon { get; init; }

    public Func<MenuItem.Icon>? ReadRightIcon { get; init; }

    // Greys the item out without locking or hiding it. Independent of the gate: a gated item is a right
    // the player lacks, a disabled one is an action that currently makes no sense.
    public Func<bool>? ReadEnabled { get; init; }

    public Func<VehicleStats?>? VehicleStats { get; init; }

    // The lighter "upgraded" overlay on the vehicle panel. Zeroed when not supplied.
    public Func<VehicleStats?>? VehicleUpgradeStats { get; init; }

    public Func<WeaponStats?>? WeaponStats { get; init; }

    public Func<WeaponStats?>? WeaponComponentStats { get; init; }

    // Null until the owning menu has been materialised.
    public MenuItem? Item { get; private protected set; }

    // Result of the last gate evaluation, so a filter pass does not re-run predicates.
    public bool IsAllowed { get; internal set; } = true;

    // Used when Label is not set; lets a submenu default to an arrow.
    protected virtual MenuText DefaultLabel => MenuText.Empty;

    // A submenu widens this to include its definition's gate, so declaring Definition does not silently
    // drop that menu's permission.
    internal virtual MenuGate EffectiveGate => Gate;

    internal abstract MenuItem Materialise(ILocalizer localizer);

    internal virtual void RaiseHighlighted()
    {
    }

    // Deriving the description from the declaration each time, rather than snapshotting and restoring
    // it, is what makes repeated lock and unlock cycles idempotent and keeps a locked item translated.
    internal virtual void ApplyPresentation(ILocalizer localizer, GateBehaviour behaviour)
    {
        if (Item is not { } item)
        {
            return;
        }

        var locked = !IsAllowed && behaviour is GateBehaviour.Lock;

        item.Text = Text.Resolve(localizer);
        item.Enabled = IsAllowed && (ReadEnabled?.Invoke() ?? true);
        item.LeftIcon = locked ? MenuItem.Icon.LOCK : (ReadLeftIcon?.Invoke() ?? LeftIcon);
        item.RightIcon = ReadRightIcon?.Invoke() ?? RightIcon;

        item.Description = locked
            ? (LockedDescription.IsEmpty
                ? localizer.Get(Loc.Framework.RestrictedDescription)
                : LockedDescription.Resolve(localizer))
            : Description.Resolve(localizer);

        var label = Label.IsEmpty ? DefaultLabel : Label;

        if (!label.IsEmpty)
        {
            item.Label = label.Resolve(localizer);
        }
    }
}

public abstract class MenuEntry<TItem> : MenuEntry
    where TItem : MenuItem
{
    // Applied once, right after the item is created. The escape hatch for what the declaration does not
    // model, such as ItemData, colour panels and slider bar colours.
    public Action<TItem>? Configure { get; init; }

    public Action<TItem>? OnHighlighted { get; init; }

    public TItem? Typed => (TItem?)Item;

    protected abstract TItem Create(ILocalizer localizer);

    internal sealed override MenuItem Materialise(ILocalizer localizer)
    {
        var item = Create(localizer);

        Item = item;

        Configure?.Invoke(item);

        return item;
    }

    internal sealed override void RaiseHighlighted()
    {
        if (Typed is { } item)
        {
            OnHighlighted?.Invoke(item);
        }
    }
}

using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>One declared row in a menu.</summary>
// Non generic so a MenuHost can hold every entry in one list and dispatch without knowing the item
// type. An entry declares its item rather than holding a pre-made one, which is what lets the
// framework rewrite text on a language change and the description on a permission change without
// either losing track of the other.
public abstract class MenuEntry
{
    public required MenuText Text { get; init; }

    public MenuText Description { get; init; }

    /// <summary>
    /// Right aligned text. Ignored for list, slider, checkbox and dynamic list items, whose label
    /// MenuAPI rewrites on every frame it draws them.
    /// </summary>
    public MenuText Label { get; init; }

    public MenuGate Gate { get; init; } = MenuGate.Always;

    /// <summary>
    /// What to say while this entry is locked. Empty falls back to the framework's own wording
    /// </summary>
    public MenuText LockedDescription { get; init; }

    /// <summary>Null inherits the menu's default, which inherits <see cref="MenuFrameworkOptions"/>.</summary>
    public GateBehaviour? Behaviour { get; init; }

    public MenuItem.Icon LeftIcon { get; init; } = MenuItem.Icon.NONE;

    public MenuItem.Icon RightIcon { get; init; } = MenuItem.Icon.NONE;

    public Func<MenuItem.Icon>? ReadLeftIcon { get; init; }

    public Func<MenuItem.Icon>? ReadRightIcon { get; init; }

    /// <summary>
    /// Greys the item out without locking or hiding it while this reads false. Independent of the
    /// gate: a gated item is a right the player lacks, a disabled one is an action that currently
    /// makes no sense.
    /// </summary>
    public Func<bool>? ReadEnabled { get; init; }

    /// <summary>Shows MenuAPI's vehicle stats panel while this entry is highlighted.</summary>
    public Func<VehicleStats?>? VehicleStats { get; init; }

    /// <summary>The lighter "upgraded" overlay on the vehicle panel. Zeroed when not supplied.</summary>
    public Func<VehicleStats?>? VehicleUpgradeStats { get; init; }

    public Func<WeaponStats?>? WeaponStats { get; init; }

    public Func<WeaponStats?>? WeaponComponentStats { get; init; }

    /// <summary>The live item. Null until the owning menu has been materialised.</summary>
    public MenuItem? Item { get; private protected set; }

    /// <summary>Result of the last gate evaluation, so a filter pass does not re-run predicates.</summary>
    public bool IsAllowed { get; internal set; } = true;

    /// <summary>Used when <see cref="Label"/> is not set; lets a submenu default to an arrow.</summary>
    protected virtual MenuText DefaultLabel => MenuText.Empty;

    /// <summary>The gate actually evaluated.</summary>
    // A submenu widens this to include its definition's gate, so declaring Definition does not
    // silently drop that menu's permission.
    internal virtual MenuGate EffectiveGate => Gate;

    internal abstract MenuItem Materialise(ILocalizer localizer);

    /// <summary>Bridges the typed <c>OnHighlighted</c> callback to the non-generic dispatch path.</summary>
    internal virtual void RaiseHighlighted()
    {
    }

    /// <summary>Rewrites every visible property from the declaration.</summary>
    // Deriving the description from the declaration each time, rather than snapshotting and
    // restoring it, is what makes repeated lock and unlock cycles idempotent and keeps a locked
    // item's text translated when the language changes.
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

/// <summary>A <see cref="MenuEntry"/> that knows the MenuAPI type it produces.</summary>
public abstract class MenuEntry<TItem> : MenuEntry
    where TItem : MenuItem
{
    /// <summary>
    /// Applied once, right after the item is created. The escape hatch for what the declaration does
    /// not model, such as <c>ItemData</c>, colour panels and slider bar colours.
    /// </summary>
    public Action<TItem>? Configure { get; init; }

    /// <summary>Raised when this entry becomes the highlighted row.</summary>
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

using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// One declared row in a menu.
/// </summary>
/// <remarks>
/// Non-generic so a <see cref="MenuHost"/> can hold every entry of a menu in one list and dispatch
/// to it without knowing the item type; the callbacks that differ per item type live on the sealed
/// leaves.
/// <para>
/// An entry <em>declares</em> its item rather than holding a pre-made one. That is what lets the
/// framework rewrite text on a language change and swap the description on a permission change
/// without either one losing track of what the other did.
/// </para>
/// </remarks>
public abstract class MenuEntry
{
    public required MenuText Text { get; init; }

    public MenuText Description { get; init; }

    /// <summary>
    /// Right aligned text. Ignored for list, slider, checkbox and dynamic list items: MenuAPI
    /// rewrites their label on every frame it draws them.
    /// </summary>
    public MenuText Label { get; init; }

    public MenuGate Gate { get; init; } = MenuGate.Always;

    /// <summary>Null inherits the menu's default, which inherits <see cref="MenuFrameworkOptions"/>.</summary>
    public GateBehaviour? Behaviour { get; init; }

    public MenuItem.Icon LeftIcon { get; init; } = MenuItem.Icon.NONE;

    public MenuItem.Icon RightIcon { get; init; } = MenuItem.Icon.NONE;

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

    /// <summary>
    /// The gate actually evaluated. A submenu widens this to include its definition's own gate, so
    /// declaring <c>Definition = new SomeMenu()</c> does not silently drop that menu's permission.
    /// </summary>
    internal virtual MenuGate EffectiveGate => Gate;

    internal abstract MenuItem Materialise(ILocalizer localizer);

    /// <summary>Bridges the typed <c>OnHighlighted</c> callback to the non-generic dispatch path.</summary>
    internal virtual void RaiseHighlighted()
    {
    }

    /// <summary>
    /// Rewrites every visible property from the declaration.
    /// </summary>
    /// <remarks>
    /// Text and gate state are applied together, on purpose. Deriving the description from the
    /// declaration each time — rather than snapshotting the original and restoring it — is what
    /// makes repeated lock/unlock cycles idempotent and keeps a locked item's restricted text
    /// translated when the language changes.
    /// </remarks>
    internal virtual void ApplyPresentation(ILocalizer localizer, GateBehaviour behaviour)
    {
        if (Item is not { } item)
        {
            return;
        }

        var locked = !IsAllowed && behaviour is GateBehaviour.Lock;

        item.Text = Text.Resolve(localizer);
        item.Enabled = IsAllowed;
        item.LeftIcon = locked ? MenuItem.Icon.LOCK : LeftIcon;
        item.RightIcon = RightIcon;

        item.Description = locked
            ? localizer.Get(Loc.Framework.RestrictedDescription)
            : Description.Resolve(localizer);

        var label = Label.IsEmpty ? DefaultLabel : Label;

        if (!label.IsEmpty)
        {
            item.Label = label.Resolve(localizer);
        }
    }
}

/// <summary>
/// A <see cref="MenuEntry"/> that knows the MenuAPI type it produces.
/// </summary>
public abstract class MenuEntry<TItem> : MenuEntry
    where TItem : MenuItem
{
    /// <summary>
    /// Applied once, right after the item is created. The escape hatch for everything the
    /// declaration does not model: <c>ItemData</c>, colour panels, slider bar colours.
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

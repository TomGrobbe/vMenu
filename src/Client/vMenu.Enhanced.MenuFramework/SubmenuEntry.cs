using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// A row that opens a child menu.
/// </summary>
// The gate applies to the link item, which is enough to close the door: MenuAPI checks Enabled
// before consulting its bound submenu table, so a locked link cannot open anything.
public sealed class SubmenuEntry : MenuEntry<MenuItem>
{
    /// <summary>Defaults to <see cref="MenuEntry.Text"/>, so most declarations name the menu once.</summary>
    public MenuText MenuTitle { get; init; }

    public MenuText MenuSubtitle { get; init; }

    /// <summary>
    /// A child that is its own menu class. Mutually exclusive with <see cref="Build"/>.
    /// </summary>
    public MenuDefinition? Definition { get; init; }

    /// <summary>
    /// A child that only exists underneath this entry and is not worth its own class. Mutually
    /// exclusive with <see cref="Definition"/>.
    /// </summary>
    public Action<MenuBuilder>? Build { get; init; }

    public Action<MenuOpened>? OnOpened { get; init; }

    public Func<MenuOpened, Task>? OnOpenedAsync { get; init; }

    public Action<ItemSelected>? OnSelected { get; init; }

    /// <summary>The host built for this row, so dropping the row can drop the menu behind it too.</summary>
    internal MenuHost? Child { get; set; }

    protected override MenuText DefaultLabel => "→";

    /// <summary>
    /// Combined with the child definition's own gate, so a menu class that declares a permission
    /// keeps it even when the entry pointing at it declares none.
    /// </summary>
    internal override MenuGate EffectiveGate => _effectiveGate ??= Definition is { } definition
        ? Gate & definition.Gate
        : Gate;

    private MenuGate? _effectiveGate;

    protected override MenuItem Create(ILocalizer localizer) =>
        new(Text.Resolve(localizer), Description.Resolve(localizer));

    internal MenuText ResolveTitle() => MenuTitle.IsEmpty ? Text : MenuTitle;
}

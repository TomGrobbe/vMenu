using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

// The gate applies to the link item, which is enough to close the door: MenuAPI checks Enabled
// before consulting its bound submenu table, so a locked link cannot open anything.
public sealed class SubmenuEntry : MenuEntry<MenuItem>
{
    // Defaults to Text, so most declarations name the menu once.
    public MenuText MenuTitle { get; init; }

    public MenuText MenuSubtitle { get; init; }

    // A child that is its own menu class. Mutually exclusive with Build.
    public MenuDefinition? Definition { get; init; }

    // A child that only exists underneath this entry and is not worth its own class. Mutually exclusive
    // with Definition.
    public Action<MenuBuilder>? Build { get; init; }

    public Action<MenuOpened>? OnOpened { get; init; }

    public Func<MenuOpened, Task>? OnOpenedAsync { get; init; }

    public Action<ItemSelected>? OnSelected { get; init; }

    // The host built for this row, so dropping the row can drop the menu behind it too.
    internal MenuHost? Child { get; set; }

    // The row that opens a menu class, taking its text and gate from the definition. No Gate here, since
    // EffectiveGate folds the definition's in.
    public static SubmenuEntry For(MenuDefinition definition) => new()
    {
        Text = definition.LinkText,
        Description = definition.LinkDescription,
        Label = definition.LinkLabel,
        Behaviour = definition.LinkBehaviour,
        Definition = definition,
    };

    protected override MenuText DefaultLabel => "→";

    // Combined with the child definition's own gate, so a menu class that declares a permission keeps it
    // even when the entry pointing at it declares none.
    internal override MenuGate EffectiveGate => _effectiveGate ??= Definition is { } definition
        ? Gate & definition.Gate
        : Gate;

    private MenuGate? _effectiveGate;

    protected override MenuItem Create(ILocalizer localizer) =>
        new(Text.Resolve(localizer), Description.Resolve(localizer));

    internal MenuText ResolveTitle() => MenuTitle.IsEmpty ? Text : MenuTitle;
}

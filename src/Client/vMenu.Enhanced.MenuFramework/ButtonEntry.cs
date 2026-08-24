using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

public sealed class ButtonEntry : MenuEntry<MenuItem>
{
    public Action<ItemSelected>? OnSelected { get; init; }

    // Runs after OnSelected. Exceptions are logged, never left unobserved.
    public Func<ItemSelected, Task>? OnSelectedAsync { get; init; }

    // Drops a second selection while OnSelectedAsync is still running. Without it, holding enter through
    // a long await, a model load say, starts the work several times over.
    public bool SingleFlight { get; init; } = true;

    protected override MenuItem Create(ILocalizer localizer) =>
        new(Text.Resolve(localizer), Description.Resolve(localizer));
}

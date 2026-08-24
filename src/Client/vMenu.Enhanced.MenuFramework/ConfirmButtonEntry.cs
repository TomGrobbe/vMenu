using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

// A plain row that asks before it does anything. The first press only turns its description into a
// warning; the second one runs the handler.
public sealed class ConfirmButtonEntry : ConfirmEntry<MenuItem>
{
    public Action<ItemSelected>? OnConfirmed { get; init; }

    // Runs after OnConfirmed. Exceptions are logged, never left unobserved.
    public Func<ItemSelected, Task>? OnConfirmedAsync { get; init; }

    protected override MenuItem Create(ILocalizer localizer) =>
        new(Text.Resolve(localizer), Description.Resolve(localizer));
}

using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// A plain row that asks before it does anything. The first press only turns its description into a
/// warning; the second one runs the handler.
/// </summary>
public sealed class ConfirmButtonEntry : ConfirmEntry<MenuItem>
{
    public Action<ItemSelected>? OnConfirmed { get; init; }

    /// <summary>Runs after <see cref="OnConfirmed"/>. Exceptions are logged, never left unobserved.</summary>
    public Func<ItemSelected, Task>? OnConfirmedAsync { get; init; }

    protected override MenuItem Create(ILocalizer localizer) =>
        new(Text.Resolve(localizer), Description.Resolve(localizer));
}

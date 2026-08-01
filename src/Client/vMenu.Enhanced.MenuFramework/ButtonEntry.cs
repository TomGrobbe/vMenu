using MenuAPI;

using vMenu.Enhanced.Localization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// A plain row that does something when selected.
/// </summary>
public sealed class ButtonEntry : MenuEntry<MenuItem>
{
    public Action<ItemSelected>? OnSelected { get; init; }

    /// <summary>Runs after <see cref="OnSelected"/>. Exceptions are logged, never left unobserved.</summary>
    public Func<ItemSelected, Task>? OnSelectedAsync { get; init; }

    /// <summary>
    /// Drops a second selection while <see cref="OnSelectedAsync"/> is still running. Without it,
    /// holding enter through a long await — a model load, say — starts the work several times over.
    /// </summary>
    public bool SingleFlight { get; init; } = true;

    protected override MenuItem Create(ILocalizer localizer) =>
        new(Text.Resolve(localizer), Description.Resolve(localizer));
}

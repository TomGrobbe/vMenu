using MenuAPI;

using vMenu.Enhanced.Localization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// A row whose value is a position on a bar.
/// </summary>
/// <remarks>
/// <see cref="Min"/> and <see cref="Max"/> are fixed once the item exists — MenuAPI exposes them
/// read-only — so a range that changes at runtime needs the menu rebuilt rather than refreshed.
/// </remarks>
public sealed class SliderEntry : MenuEntry<MenuSliderItem>
{
    public required int Min { get; init; }

    public required int Max { get; init; }

    /// <summary>The starting position. Ignored when <see cref="ReadPosition"/> is set.</summary>
    public int Position { get; init; }

    public Func<int>? ReadPosition { get; init; }

    public bool ShowDivider { get; init; }

    public MenuItem.Icon SliderLeftIcon { get; init; } = MenuItem.Icon.NONE;

    public MenuItem.Icon SliderRightIcon { get; init; } = MenuItem.Icon.NONE;

    public Action<SliderMoved>? OnMoved { get; init; }

    public Action<SliderSelected>? OnSelected { get; init; }

    public Func<SliderSelected, Task>? OnSelectedAsync { get; init; }

    protected override MenuSliderItem Create(ILocalizer localizer) =>
        new(
            Text.Resolve(localizer),
            Description.Resolve(localizer),
            Min,
            Max,
            Math.Clamp(ReadPosition?.Invoke() ?? Position, Min, Max),
            ShowDivider)
        {
            SliderLeftIcon = SliderLeftIcon,
            SliderRightIcon = SliderRightIcon,
        };

    internal override void ApplyPresentation(ILocalizer localizer, GateBehaviour behaviour)
    {
        base.ApplyPresentation(localizer, behaviour);

        if (ReadPosition is not null && Typed is { } item)
        {
            item.Position = Math.Clamp(ReadPosition(), item.Min, item.Max);
        }
    }
}

using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

public sealed class SeparatorEntry : MenuEntry<SeparatorMenuItem>
{
    /// <summary>Draws the text as <c>↓ Text ↓</c>.</summary>
    public bool ShowArrows { get; init; } = true;

    protected override SeparatorMenuItem Create(ILocalizer localizer) =>
        new(Text.Resolve(localizer), Description.Resolve(localizer), ShowArrows);

    internal override void ApplyPresentation(ILocalizer localizer, GateBehaviour behaviour)
    {
        if (Typed is { } item)
        {
            item.Text = Text.Resolve(localizer);
            item.Description = Description.Resolve(localizer);
        }
    }
}

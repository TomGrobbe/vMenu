using System.Diagnostics.CodeAnalysis;

using MenuAPI;

using vMenu.Enhanced.Localization;

namespace vMenu.Enhanced.MenuFramework;

public sealed class SeparatorEntry : MenuEntry<SeparatorMenuItem>
{
    /// <summary>For text that is not a plain translation key: set <see cref="MenuEntry.Text"/> in the initializer.</summary>
    public SeparatorEntry()
    {
    }

    /// <summary>Takes the text from <paramref name="textKey"/>, and the description from <c>textKey + ".desc"</c> when
    /// English has that key. Setting Description in the initializer still wins.</summary>
    [SetsRequiredMembers]
    public SeparatorEntry(string textKey)
        : base(textKey)
    {
    }

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

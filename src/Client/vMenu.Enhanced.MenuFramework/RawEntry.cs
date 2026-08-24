using System.Diagnostics.CodeAnalysis;

using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

// Wraps an item the caller built by hand, so it keeps its place in declaration order and still takes
// part in gating. Without the dispatch registration a locked raw list or slider would still move
// under the arrow keys. Text is left alone, there being no declaration to re-derive it from, so a
// raw item does not translate: use a real entry type for anything the player reads.
public sealed class RawEntry : MenuEntry<MenuItem>
{
    private readonly MenuItem _item;

    // The values the item arrived with, standing in for a declaration when unlocking.
    private readonly string? _description;

    private readonly MenuItem.Icon _leftIcon;

    [SetsRequiredMembers]
    public RawEntry(MenuItem item)
    {
        _item = item;
        _description = item.Description;
        _leftIcon = item.LeftIcon;

        Text = MenuText.Empty;
    }

    protected override MenuItem Create(ILocalizer localizer) => _item;

    internal override void ApplyPresentation(ILocalizer localizer, GateBehaviour behaviour)
    {
        if (Item is not { } item)
        {
            return;
        }

        var locked = !IsAllowed && behaviour is GateBehaviour.Lock;

        item.Enabled = IsAllowed;
        item.LeftIcon = locked ? MenuItem.Icon.LOCK : _leftIcon;
        item.Description = locked ? localizer.Get(Loc.Framework.RestrictedDescription) : _description!;
    }
}

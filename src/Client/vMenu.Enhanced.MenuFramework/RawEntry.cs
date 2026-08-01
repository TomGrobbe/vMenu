using System.Diagnostics.CodeAnalysis;

using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// Wraps an item the caller built by hand.
/// </summary>
/// <remarks>
/// Exists so a hand-written item keeps its place in declaration order and still takes part in
/// gating. It is registered for dispatch like any other entry, which matters: without it a locked
/// raw list or slider would still be changeable with the arrow keys.
/// <para>
/// Text is left alone — there is no declaration to re-derive it from, so a raw item does not
/// translate. Use a real entry type for anything the player reads.
/// </para>
/// </remarks>
public sealed class RawEntry : MenuEntry<MenuItem>
{
    private readonly MenuItem _item;

    /// <summary>The values the item arrived with, standing in for a declaration when unlocking.</summary>
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

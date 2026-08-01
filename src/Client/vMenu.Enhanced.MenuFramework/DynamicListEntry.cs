using MenuAPI;

using vMenu.Enhanced.Localization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// A row whose value is produced on demand rather than picked from a list.
/// </summary>
public sealed class DynamicListEntry : MenuEntry<MenuDynamicListItem>
{
    /// <summary>The value to show. Re-read on every refresh.</summary>
    public required Func<string> ReadValue { get; init; }

    /// <summary>
    /// Produces the next value. The framework wraps this before handing it to MenuAPI, because
    /// MenuAPI invokes it directly from <c>GoLeft</c>/<c>GoRight</c> without checking whether the
    /// item is enabled — an unwrapped callback would run on a locked row.
    /// </summary>
    public required Func<DynamicListChanging, string> Change { get; init; }

    public Action<DynamicListChanged>? OnChanged { get; init; }

    public Action<DynamicListSelected>? OnSelected { get; init; }

    public Func<DynamicListSelected, Task>? OnSelectedAsync { get; init; }

    protected override MenuDynamicListItem Create(ILocalizer localizer) =>
        new(Text.Resolve(localizer), ReadValue(), Guarded, Description.Resolve(localizer));

    internal override void ApplyPresentation(ILocalizer localizer, GateBehaviour behaviour)
    {
        base.ApplyPresentation(localizer, behaviour);

        if (Typed is { } item)
        {
            item.CurrentItem = ReadValue();
        }
    }

    private string Guarded(MenuDynamicListItem item, bool left) =>
        item.Enabled ? Change(new DynamicListChanging(item, item.CurrentItem, left)) : item.CurrentItem;
}

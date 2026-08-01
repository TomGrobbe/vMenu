using MenuAPI;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// Payloads handed to entry callbacks.
/// </summary>
/// <remarks>
/// One struct per event rather than a parameter list, so a handler stays a single parameter lambda
/// and a payload can gain a field later without breaking every call site.
/// <para>
/// <c>ItemIndex</c> is MenuAPI's index and is relative to the active filter, so it is fine to show
/// but must never be used to identify an item. The framework itself dispatches on object identity.
/// </para>
/// <para>
/// Item references are declared nullable where MenuAPI can genuinely hand over null. MenuAPI is not
/// nullable annotated, so the compiler will not warn about any of this.
/// </para>
/// <para>
/// Do not compare two payloads. The generated equality members route through
/// <c>EqualityComparer&lt;T&gt;.Default</c>, and the FiveM sandbox throws rather than load the
/// internal comparers behind it. They exist only because the type is a record; nothing calls them.
/// </para>
/// </remarks>
public readonly record struct ItemSelected(Menu Menu, MenuItem Item, int ItemIndex);

public readonly record struct CheckboxChanged(Menu Menu, MenuCheckboxItem Item, int ItemIndex, bool Checked);

public readonly record struct ListSelected(Menu Menu, MenuListItem Item, int ItemIndex, int SelectedIndex)
{
    public string? Value => Item.GetCurrentSelection();
}

public readonly record struct ListIndexChanged(Menu Menu, MenuListItem Item, int ItemIndex, int OldIndex, int NewIndex);

public readonly record struct SliderMoved(Menu Menu, MenuSliderItem Item, int ItemIndex, int OldPosition, int NewPosition);

public readonly record struct SliderSelected(Menu Menu, MenuSliderItem Item, int ItemIndex, int Position);

/// <summary>Raised before the value moves, so a handler can decide what the next one is.</summary>
public readonly record struct DynamicListChanging(MenuDynamicListItem Item, string CurrentValue, bool Left);

public readonly record struct DynamicListChanged(Menu Menu, MenuDynamicListItem Item, string OldValue, string NewValue);

public readonly record struct DynamicListSelected(Menu Menu, MenuDynamicListItem Item, string Value);

public readonly record struct MenuOpened(Menu Menu, MenuItem? CurrentItem);

public readonly record struct MenuIndexChanged(Menu Menu, MenuItem? OldItem, MenuItem? NewItem, int OldIndex, int NewIndex);

using MenuAPI;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// Payloads handed to entry callbacks.
/// </summary>
// ItemIndex is MenuAPI's index and is relative to the active filter, so it is fine to show but must
// never identify an item. Item references are nullable where MenuAPI can genuinely hand over null,
// which it is not annotated for, so the compiler will not warn.
// Never compare two payloads. The generated equality routes through EqualityComparer<T>.Default, and
// the sandbox throws rather than load the comparers behind it.
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

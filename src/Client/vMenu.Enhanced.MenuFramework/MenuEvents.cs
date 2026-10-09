using MenuAPI;

namespace vMenu.Enhanced.MenuFramework;

// Payloads handed to entry callbacks. ItemIndex is MenuAPI's index and is relative to the active
// filter, so it is fine to show but must never identify an item. Never compare two payloads: the
// generated equality routes through EqualityComparer<T>.Default, and the sandbox throws rather than
// load the comparers behind it.

/// <summary>A button, confirm button or submenu row was pressed.</summary>
public readonly record struct ItemSelected(Menu Menu, MenuItem Item, int ItemIndex);

/// <summary>A checkbox was toggled. Checked is the new state.</summary>
public readonly record struct CheckboxChanged(Menu Menu, MenuCheckboxItem Item, int ItemIndex, bool Checked);

/// <summary>A list row was pressed. SelectedIndex and Value are what it showed at the time.</summary>
public readonly record struct ListSelected(Menu Menu, MenuListItem Item, int ItemIndex, int SelectedIndex)
{
    public string? Value => Item.GetCurrentSelection();
}

/// <summary>A list row was scrolled from OldIndex to NewIndex.</summary>
public readonly record struct ListIndexChanged(Menu Menu, MenuListItem Item, int ItemIndex, int OldIndex, int NewIndex);

/// <summary>A slider moved from OldPosition to NewPosition.</summary>
public readonly record struct SliderMoved(Menu Menu, MenuSliderItem Item, int ItemIndex, int OldPosition, int NewPosition);

/// <summary>A slider row was pressed at Position.</summary>
public readonly record struct SliderSelected(Menu Menu, MenuSliderItem Item, int ItemIndex, int Position);

/// <summary>Raised before the value moves, so a handler can decide what the next one is.</summary>
public readonly record struct DynamicListChanging(MenuDynamicListItem Item, string CurrentValue, bool Left);

/// <summary>A dynamic list row changed from OldValue to NewValue.</summary>
public readonly record struct DynamicListChanged(Menu Menu, MenuDynamicListItem Item, string? OldValue, string NewValue);

/// <summary>A dynamic list row was pressed while showing Value.</summary>
public readonly record struct DynamicListSelected(Menu Menu, MenuDynamicListItem Item, string? Value);

/// <summary>A menu was opened. CurrentItem is the row the cursor starts on.</summary>
public readonly record struct MenuOpened(Menu Menu, MenuItem? CurrentItem);

/// <summary>The cursor moved from OldItem to NewItem.</summary>
public readonly record struct MenuIndexChanged(Menu Menu, MenuItem? OldItem, MenuItem? NewItem, int OldIndex, int NewIndex);

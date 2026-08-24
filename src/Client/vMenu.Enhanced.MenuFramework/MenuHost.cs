using MenuAPI;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

// All eleven MenuAPI events are menu level rather than per item, which is what forces a hand written
// menu into one long if/else over item identity. Subscribing once here and routing on object identity
// is the whole point of the framework.
internal sealed class MenuHost : IDisposable
{
    private readonly Dictionary<MenuItem, MenuEntry> _byItem = new(ReferenceComparer<MenuItem>.Instance);

    private readonly HashSet<MenuItem> _hidden = new(ReferenceComparer<MenuItem>.Instance);

    private readonly HashSet<MenuEntry> _inFlight = new(ReferenceComparer<MenuEntry>.Instance);

    // Closing and reopening while the previous refresh is still awaiting would otherwise run two handlers
    // over the same menu, and both would append their rows to it.
    private bool _openInFlight;

    // Remembered rather than looked for, so putting a row back to asking never walks the entry list. A
    // menu can hold thousands of rows and this happens on every arrow key.
    private IConfirmable? _awaitingConfirmation;

    private Func<MenuItem, bool>? _userFilter;

    private bool _filterDirty;

    private bool _attached;

    private MenuItem? _emptyNotice;

    private bool _noticeShown;

    internal MenuHost(Menu menu, MenuHost? parent, MenuGate gate, MenuText title, MenuText subtitle, GateBehaviour? defaultBehaviour)
    {
        Menu = menu;
        Parent = parent;
        Gate = gate;
        Title = title;
        Subtitle = subtitle;

        Builder = new MenuBuilder(this) { DefaultGateBehaviour = defaultBehaviour };
    }

    internal Menu Menu { get; }

    internal MenuHost? Parent { get; }

    internal MenuGate Gate { get; }

    internal MenuText Title { get; }

    internal MenuText Subtitle { get; }

    internal MenuBuilder Builder { get; }

    internal List<MenuHost> Children { get; } = [];

    internal bool IsReachable()
    {
        for (var host = this; host is not null; host = host.Parent)
        {
            if (!host.Gate.Evaluate())
            {
                return false;
            }
        }

        return true;
    }

    // Gating does not happen here: the filter needs the complete item list, so it runs afterwards.
    internal MenuItem Materialise(MenuEntry entry, ILocalizer localizer)
    {
        var item = entry.Materialise(localizer);

        Menu.AddMenuItem(item);
        _byItem[item] = entry;

        return item;
    }

    internal bool IsLive => _attached;

    // Everything that remembers an item has to be emptied together, or a later refresh would gate rows
    // the menu no longer has.
    internal void ClearEntries()
    {
        // MenuAPI drops the menu a bound row opened along with the row, so the host for it is untracked to
        // match. Before the entries, which is what ClearMenuItems reads.
        foreach (var entry in Builder.Entries)
        {
            if (entry is SubmenuEntry { Child: { } child })
            {
                MenuRegistry.Untrack(child);
            }
        }

        Menu.ClearMenuItems();

        Builder.Entries.Clear();
        _byItem.Clear();
        _hidden.Clear();
        _inFlight.Clear();

        _awaitingConfirmation = null;
        _filterDirty = false;

        // ClearMenuItems took the notice with everything else, so the record of it has to go too or it would
        // never be added back.
        _noticeShown = false;
    }

    internal void Attach()
    {
        if (_attached)
        {
            return;
        }

        _attached = true;

        Menu.OnItemSelect += HandleItemSelect;
        Menu.OnCheckboxChange += HandleCheckboxChange;
        Menu.OnListItemSelect += HandleListSelect;
        Menu.OnListIndexChange += HandleListIndexChange;
        Menu.OnSliderItemSelect += HandleSliderSelect;
        Menu.OnSliderPositionChange += HandleSliderMoved;
        Menu.OnDynamicListItemSelect += HandleDynamicSelect;
        Menu.OnDynamicListItemCurrentItemChange += HandleDynamicChanged;
        Menu.OnMenuOpen += HandleMenuOpen;
        Menu.OnMenuClose += HandleMenuClose;
        Menu.OnIndexChange += HandleIndexChange;
    }

    public void Dispose()
    {
        if (!_attached)
        {
            return;
        }

        _attached = false;

        Menu.OnItemSelect -= HandleItemSelect;
        Menu.OnCheckboxChange -= HandleCheckboxChange;
        Menu.OnListItemSelect -= HandleListSelect;
        Menu.OnListIndexChange -= HandleListIndexChange;
        Menu.OnSliderItemSelect -= HandleSliderSelect;
        Menu.OnSliderPositionChange -= HandleSliderMoved;
        Menu.OnDynamicListItemSelect -= HandleDynamicSelect;
        Menu.OnDynamicListItemCurrentItemChange -= HandleDynamicChanged;
        Menu.OnMenuOpen -= HandleMenuOpen;
        Menu.OnMenuClose -= HandleMenuClose;
        Menu.OnIndexChange -= HandleIndexChange;
    }

    // Used for both a permission resync and a language change, which cannot be allowed to disagree about
    // what an item says. Synchronous, or menus would show stale state after the notifier already returned.
    internal void Refresh(ILocalizer localizer)
    {
        RefreshHeader(localizer);

        foreach (var hint in Builder.InstructionalButtons)
        {
            if (hint.Gate.Evaluate())
            {
                Menu.InstructionalButtons[hint.Control] = hint.Text.Resolve(localizer);
            }
            else
            {
                Menu.InstructionalButtons.Remove(hint.Control);
            }
        }

        var fallback = Builder.DefaultGateBehaviour ?? MenuFrameworkOptions.DefaultGateBehaviour;
        var visibilityChanged = false;

        foreach (var entry in Builder.Entries)
        {
            entry.IsAllowed = entry.EffectiveGate.Evaluate();

            var behaviour = entry.Behaviour ?? fallback;

            entry.ApplyPresentation(localizer, behaviour);

            if (entry.Item is not { } item)
            {
                continue;
            }

            var hide = !entry.IsAllowed && behaviour is GateBehaviour.Hide;

            visibilityChanged |= hide ? _hidden.Add(item) : _hidden.Remove(item);
        }

        UpdateNoticeText(localizer);

        if (!visibilityChanged && _noticeShown == NoticeWanted())
        {
            // Leave the filter alone so the player's cursor does not jump.
            return;
        }

        if (Menu.Visible)
        {
            // Re-filtering under an open menu would shuffle rows beneath the cursor. Hidden entries are already
            // disabled above, so deferring is cosmetic only.
            _filterDirty = true;
            return;
        }

        ApplyFilter();
    }

    internal void RefreshHeader(ILocalizer localizer)
    {
        Menu.MenuTitle = Title.Resolve(localizer);
        Menu.MenuSubtitle = ResolveSubtitle(Title, Subtitle, localizer);
    }

    // MenuAPI draws the bar under the banner whether or not there is a subtitle in it, but only moves the
    // rows down when there is one, so an empty subtitle leaves the first row drawn on top of the bar.
    internal static string ResolveSubtitle(MenuText title, MenuText subtitle, ILocalizer localizer)
    {
        var resolved = subtitle.Resolve(localizer);

        return resolved.Length > 0 ? resolved : title.Resolve(localizer);
    }

    internal void SetUserFilter(Func<MenuItem, bool>? predicate)
    {
        _userFilter = predicate;

        ApplyFilter();
    }

    internal void RefreshFilter() => ApplyFilter();

    internal void SortItems(Comparison<MenuItem> comparison)
    {
        ShowNotice(false);

        Menu.SortMenuItems(comparison);

        // MenuAPI's sort clears any active filter without saying so, so it has to be re-applied.
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        _filterDirty = false;

        var restore = Menu.GetCurrentMenuItem();
        var empty = NoticeWanted();

        ShowNotice(empty);

        if (_hidden.Count == 0 && _userFilter is null && !empty)
        {
            Menu.ResetFilter();
        }
        else
        {
            Menu.FilterMenuItems(item => ReferenceEquals(item, _emptyNotice)
                ? empty
                : !_hidden.Contains(item) && (_userFilter?.Invoke(item) ?? true));
        }

        if (restore is null)
        {
            return;
        }

        // Filtering always resets to the top, so put the player back on their row if it survived.
        var index = Menu.GetMenuItems().IndexOf(restore);

        Menu.RefreshIndex(index < 0 ? 0 : index);
    }

    private bool NoticeWanted()
    {
        var any = false;

        foreach (var entry in Builder.Entries)
        {
            if (entry.Item is not { } item)
            {
                continue;
            }

            if (!_hidden.Contains(item))
            {
                return false;
            }

            any = true;
        }

        return any;
    }

    private void ShowNotice(bool wanted)
    {
        if (wanted == _noticeShown)
        {
            return;
        }

        _noticeShown = wanted;

        if (!wanted)
        {
            Menu.RemoveMenuItem(_emptyNotice!);

            return;
        }

        _emptyNotice ??= new MenuItem(string.Empty) { Enabled = false };

        UpdateNoticeText(Localizer.Current);

        Menu.AddMenuItem(_emptyNotice);
    }

    private void UpdateNoticeText(ILocalizer localizer)
    {
        if (_emptyNotice is not { } notice)
        {
            return;
        }

        notice.Text = localizer.Get(Loc.Framework.EmptyMenu);
        notice.Description = localizer.Get(Loc.Framework.EmptyMenuDescription);
    }

    private async void HandleItemSelect(Menu menu, MenuItem item, int itemIndex)
    {
        if (!_byItem.TryGetValue(item, out var entry) || !item.Enabled)
        {
            return;
        }

        var arguments = new ItemSelected(menu, item, itemIndex);

        if (entry is SubmenuEntry submenu)
        {
            Guard(() => submenu.OnSelected?.Invoke(arguments), item);
            return;
        }

        if (entry is ConfirmButtonEntry confirm)
        {
            if (!Arm(confirm))
            {
                return;
            }

            Guard(() => confirm.OnConfirmed?.Invoke(arguments), item);

            if (confirm.OnConfirmedAsync is { } confirmed)
            {
                await RunAsync(entry, confirm.SingleFlight, () => confirmed(arguments), item);
            }

            return;
        }

        if (entry is not ButtonEntry button)
        {
            return;
        }

        Guard(() => button.OnSelected?.Invoke(arguments), item);

        if (button.OnSelectedAsync is not { } handler)
        {
            return;
        }

        await RunAsync(entry, button.SingleFlight, () => handler(arguments), item);
    }

    private bool Arm<TItem>(ConfirmEntry<TItem> entry)
        where TItem : MenuItem
    {
        if (entry.Press())
        {
            _awaitingConfirmation = null;

            return true;
        }

        _awaitingConfirmation = entry;

        return false;
    }

    private void ClearConfirmation()
    {
        if (_awaitingConfirmation is not { } entry)
        {
            return;
        }

        _awaitingConfirmation = null;

        entry.ResetConfirmation();
    }

    private async Task RunAsync(MenuEntry entry, bool singleFlight, Func<Task> handler, MenuItem item)
    {
        if (singleFlight && !_inFlight.Add(entry))
        {
            return;
        }

        try
        {
            await handler();
        }
        catch (Exception exception)
        {
            // MenuAPI's events are multicast and return void, so an unobserved throw would take the rest of the
            // invocation list with it.
            Log.Error($"[Menu] '{item.Text}' select handler threw: {exception}");
        }
        finally
        {
            if (singleFlight)
            {
                _inFlight.Remove(entry);
            }
        }
    }

    private async void HandleCheckboxChange(Menu menu, MenuCheckboxItem item, int itemIndex, bool newState)
    {
        if (!_byItem.TryGetValue(item, out var entry))
        {
            return;
        }

        // Belt and braces: checkboxes reach this through SelectItem, which honours Enabled.
        if (!item.Enabled)
        {
            item.Checked = !newState;
            return;
        }

        if (entry is not CheckboxEntry checkbox)
        {
            return;
        }

        var arguments = new CheckboxChanged(menu, item, itemIndex, newState);

        Guard(() => checkbox.OnChanged?.Invoke(arguments), item);

        if (checkbox.OnChangedAsync is not { } handler)
        {
            return;
        }

        try
        {
            await handler(arguments);
        }
        catch (Exception exception)
        {
            Log.Error($"[Menu] '{item.Text}' change handler threw: {exception}");
        }
    }

    private void HandleListIndexChange(Menu menu, MenuListItem item, int oldIndex, int newIndex, int itemIndex)
    {
        if (!_byItem.TryGetValue(item, out var entry))
        {
            return;
        }

        // GoLeft/GoRight do not check Enabled and the value has already moved, so the change has to be undone
        // rather than the callback suppressed. Before the entry type check, so a raw list item is covered too.
        if (!item.Enabled)
        {
            item.ListIndex = oldIndex;
            return;
        }

        // Scrolling to another value is leaving whatever was confirmed, so the row asks again.
        ClearConfirmation();

        var changed = new ListIndexChanged(menu, item, itemIndex, oldIndex, newIndex);

        if (entry is ConfirmListEntry confirmList)
        {
            Guard(() => confirmList.OnIndexChanged?.Invoke(changed), item);

            return;
        }

        if (entry is not ListEntry list)
        {
            return;
        }

        Guard(() => list.OnIndexChanged?.Invoke(changed), item);
    }

    private async void HandleListSelect(Menu menu, MenuListItem item, int selectedIndex, int itemIndex)
    {
        if (!_byItem.TryGetValue(item, out var entry) || !item.Enabled)
        {
            return;
        }

        var arguments = new ListSelected(menu, item, itemIndex, selectedIndex);

        if (entry is ConfirmListEntry confirm)
        {
            if (!Arm(confirm))
            {
                return;
            }

            Guard(() => confirm.OnConfirmed?.Invoke(arguments), item);

            if (confirm.OnConfirmedAsync is { } confirmed)
            {
                await RunAsync(entry, confirm.SingleFlight, () => confirmed(arguments), item);
            }

            return;
        }

        if (entry is not ListEntry list)
        {
            return;
        }

        Guard(() => list.OnSelected?.Invoke(arguments), item);

        if (list.OnSelectedAsync is not { } handler)
        {
            return;
        }

        try
        {
            await handler(arguments);
        }
        catch (Exception exception)
        {
            Log.Error($"[Menu] '{item.Text}' select handler threw: {exception}");
        }
    }

    private void HandleSliderMoved(Menu menu, MenuSliderItem item, int oldPosition, int newPosition, int itemIndex)
    {
        if (!_byItem.TryGetValue(item, out var entry))
        {
            return;
        }

        if (!item.Enabled)
        {
            item.Position = oldPosition;
            return;
        }

        if (entry is not SliderEntry slider)
        {
            return;
        }

        Guard(() => slider.OnMoved?.Invoke(new SliderMoved(menu, item, itemIndex, oldPosition, newPosition)), item);
    }

    private async void HandleSliderSelect(Menu menu, MenuSliderItem item, int position, int itemIndex)
    {
        if (!_byItem.TryGetValue(item, out var entry) || !item.Enabled || entry is not SliderEntry slider)
        {
            return;
        }

        var arguments = new SliderSelected(menu, item, itemIndex, position);

        Guard(() => slider.OnSelected?.Invoke(arguments), item);

        if (slider.OnSelectedAsync is not { } handler)
        {
            return;
        }

        try
        {
            await handler(arguments);
        }
        catch (Exception exception)
        {
            Log.Error($"[Menu] '{item.Text}' select handler threw: {exception}");
        }
    }

    private void HandleDynamicChanged(Menu menu, MenuDynamicListItem item, string? oldValue, string newValue)
    {
        if (!_byItem.TryGetValue(item, out var entry))
        {
            return;
        }

        if (!item.Enabled)
        {
            item.CurrentItem = oldValue;
            return;
        }

        if (entry is not DynamicListEntry dynamicList)
        {
            return;
        }

        Guard(() => dynamicList.OnChanged?.Invoke(new DynamicListChanged(menu, item, oldValue, newValue)), item);
    }

    private async void HandleDynamicSelect(Menu menu, MenuDynamicListItem item, string? value)
    {
        if (!_byItem.TryGetValue(item, out var entry) || !item.Enabled || entry is not DynamicListEntry dynamicList)
        {
            return;
        }

        var arguments = new DynamicListSelected(menu, item, value);

        Guard(() => dynamicList.OnSelected?.Invoke(arguments), item);

        if (dynamicList.OnSelectedAsync is not { } handler)
        {
            return;
        }

        try
        {
            await handler(arguments);
        }
        catch (Exception exception)
        {
            Log.Error($"[Menu] '{item.Text}' select handler threw: {exception}");
        }
    }

    private async void HandleMenuOpen(Menu menu)
    {
        ClearConfirmation();

        // The bound item table is static and rebindable, and any code can call OpenMenu directly, so the menu
        // re-checks its own gate rather than trusting the door.
        if (!IsReachable())
        {
            menu.CloseMenu();
            Parent?.Menu.OpenMenu();

            return;
        }

        // A row bound to a submenu is opened by MenuAPI itself, so a title that depends on what the player
        // just picked has had no refresh pass since they picked it, and MenuAPI draws no banner for an empty title.
        RefreshHeader(Localizer.Current);

        if (_filterDirty)
        {
            ApplyFilter();
        }

        var current = menu.GetCurrentMenuItem();

        ApplyHighlight(current);

        var opened = new MenuOpened(menu, current);

        Guard(() => Builder.OnOpened?.Invoke(opened), current);

        if (Builder.OnOpenedAsync is not { } handler || _openInFlight)
        {
            return;
        }

        _openInFlight = true;

        try
        {
            // Awaiting the delegate itself would only await the last one it was combined from.
            foreach (var subscriber in handler.GetInvocationList())
            {
                await ((Func<MenuOpened, Task>)subscriber)(opened);
            }
        }
        catch (Exception exception)
        {
            Log.Error($"[Menu] '{Menu.MenuTitle}' open handler threw: {exception}");
        }
        finally
        {
            _openInFlight = false;
        }
    }

    // Opening a submenu closes this one, so this covers walking away from the menu as well as shutting it.
    private void HandleMenuClose(Menu menu)
    {
        ClearConfirmation();

        Guard(() => Builder.OnClosed?.Invoke(menu), null);
    }

    private void HandleIndexChange(Menu menu, MenuItem oldItem, MenuItem? newItem, int oldIndex, int newIndex)
    {
        ClearConfirmation();

        ApplyHighlight(newItem);

        Guard(() => Builder.OnIndexChanged?.Invoke(new MenuIndexChanged(menu, oldItem, newItem, oldIndex, newIndex)), newItem);
    }

    // Drives the stats panels from the highlighted entry, which is why entries model them and ItemData
    // stays free for callers.
    private void ApplyHighlight(MenuItem? item)
    {
        if (item is null || !_byItem.TryGetValue(item, out var entry))
        {
            Menu.ShowVehicleStatsPanel = false;
            Menu.ShowWeaponStatsPanel = false;

            return;
        }

        if (entry.VehicleStats?.Invoke() is { } vehicle)
        {
            var upgrades = entry.VehicleUpgradeStats?.Invoke() ?? VehicleStats.None;

            Menu.ShowVehicleStatsPanel = true;
            Menu.SetVehicleStats(vehicle.TopSpeed, vehicle.Acceleration, vehicle.Braking, vehicle.Traction);
            Menu.SetVehicleUpgradeStats(upgrades.TopSpeed, upgrades.Acceleration, upgrades.Braking, upgrades.Traction);
        }
        else
        {
            Menu.ShowVehicleStatsPanel = false;
        }

        if (entry.WeaponStats?.Invoke() is { } weapon)
        {
            var components = entry.WeaponComponentStats?.Invoke() ?? WeaponStats.None;

            Menu.ShowWeaponStatsPanel = true;
            Menu.SetWeaponStats(weapon.Damage, weapon.FireRate, weapon.Accuracy, weapon.Range);
            Menu.SetWeaponComponentStats(components.Damage, components.FireRate, components.Accuracy, components.Range);
        }
        else
        {
            Menu.ShowWeaponStatsPanel = false;
        }

        Guard(entry.RaiseHighlighted, item);
    }

    private static void Guard(Action action, MenuItem? item)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            Log.Error($"[Menu] '{item?.Text ?? "<menu>"}' handler threw: {exception}");
        }
    }
}

using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>Owns one MenuAPI <see cref="Menu"/>, its entries, subscriptions, filter and gate state.</summary>
// All eleven MenuAPI events are menu level rather than per item, which is what forces a hand written
// menu into one long if/else over item identity. Subscribing once here and routing on object
// identity is the whole point of the framework.
internal sealed class MenuHost : IDisposable
{
    private readonly Dictionary<MenuItem, MenuEntry> _byItem = new(ReferenceComparer<MenuItem>.Instance);

    private readonly HashSet<MenuItem> _hidden = new(ReferenceComparer<MenuItem>.Instance);

    private readonly HashSet<MenuEntry> _inFlight = new(ReferenceComparer<MenuEntry>.Instance);

    private Func<MenuItem, bool>? _userFilter;

    private bool _filterDirty;

    private bool _attached;

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

    /// <summary>The gate of whatever opens this menu, so the menu can re-check its own door.</summary>
    internal MenuGate Gate { get; }

    internal MenuText Title { get; }

    internal MenuText Subtitle { get; }

    internal MenuBuilder Builder { get; }

    internal List<MenuHost> Children { get; } = [];

    /// <summary>Whether this menu and every menu above it currently pass their gate.</summary>
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

    /// <summary>Creates the MenuAPI item for an entry and registers it for dispatch.</summary>
    // Gating does not happen here: the filter needs the complete item list, so it runs afterwards.
    internal MenuItem Materialise(MenuEntry entry, ILocalizer localizer)
    {
        var item = entry.Materialise(localizer);

        Menu.AddMenuItem(item);
        _byItem[item] = entry;

        return item;
    }

    /// <summary>Whether the menu is already built, so a late entry has to be materialised on the spot.</summary>
    internal bool IsLive => _attached;

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

    /// <summary>Re-evaluates every gate and rewrites every visible property.</summary>
    // Used for both a permission resync and a language change, which cannot be allowed to disagree
    // about what an item says. Synchronous, or menus would show stale state after the notifier
    // already returned.
    internal void Refresh(ILocalizer localizer)
    {
        Menu.MenuTitle = Title.Resolve(localizer);
        Menu.MenuSubtitle = Subtitle.Resolve(localizer);

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

        if (!visibilityChanged)
        {
            // Leave the filter alone so the player's cursor does not jump.
            return;
        }

        if (Menu.Visible)
        {
            // Re-filtering under an open menu would shuffle rows beneath the cursor. Hidden entries
            // are already disabled above, so deferring is cosmetic only.
            _filterDirty = true;
            return;
        }

        ApplyFilter();
    }

    internal void SetUserFilter(Func<MenuItem, bool>? predicate)
    {
        _userFilter = predicate;

        ApplyFilter();
    }

    internal void SortItems(Comparison<MenuItem> comparison)
    {
        Menu.SortMenuItems(comparison);

        // MenuAPI's sort clears any active filter without saying so, so it has to be re-applied.
        ApplyFilter();
    }

    // The only place that touches MenuAPI's filter for a managed menu. Gate hiding and a caller's
    // predicate are combined here because MenuAPI supports exactly one.
    private void ApplyFilter()
    {
        _filterDirty = false;

        var restore = Menu.GetCurrentMenuItem();

        if (_hidden.Count == 0 && _userFilter is null)
        {
            Menu.ResetFilter();
        }
        else
        {
            Menu.FilterMenuItems(item => !_hidden.Contains(item) && (_userFilter?.Invoke(item) ?? true));
        }

        if (restore is null)
        {
            return;
        }

        // Filtering always resets to the top, so put the player back on their row if it survived.
        var index = Menu.GetMenuItems().IndexOf(restore);

        Menu.RefreshIndex(index < 0 ? 0 : index);
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

        if (entry is not ButtonEntry button)
        {
            return;
        }

        Guard(() => button.OnSelected?.Invoke(arguments), item);

        if (button.OnSelectedAsync is not { } handler)
        {
            return;
        }

        var tracked = button.SingleFlight;

        if (tracked && !_inFlight.Add(entry))
        {
            return;
        }

        try
        {
            await handler(arguments);
        }
        catch (Exception exception)
        {
            // MenuAPI's events are multicast and return void, so an unobserved throw would take the
            // rest of the invocation list with it.
            API.Log.Error($"[Menu] '{item.Text}' select handler threw: {exception}");
        }
        finally
        {
            if (tracked)
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
            API.Log.Error($"[Menu] '{item.Text}' change handler threw: {exception}");
        }
    }

    private void HandleListIndexChange(Menu menu, MenuListItem item, int oldIndex, int newIndex, int itemIndex)
    {
        if (!_byItem.TryGetValue(item, out var entry))
        {
            return;
        }

        // GoLeft/GoRight do not check Enabled and the value has already moved, so the change has to
        // be undone rather than the callback suppressed. Before the entry type check, so a hand
        // added raw list item is covered too.
        if (!item.Enabled)
        {
            item.ListIndex = oldIndex;
            return;
        }

        if (entry is not ListEntry list)
        {
            return;
        }

        Guard(() => list.OnIndexChanged?.Invoke(new ListIndexChanged(menu, item, itemIndex, oldIndex, newIndex)), item);
    }

    private async void HandleListSelect(Menu menu, MenuListItem item, int selectedIndex, int itemIndex)
    {
        if (!_byItem.TryGetValue(item, out var entry) || !item.Enabled || entry is not ListEntry list)
        {
            return;
        }

        var arguments = new ListSelected(menu, item, itemIndex, selectedIndex);

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
            API.Log.Error($"[Menu] '{item.Text}' select handler threw: {exception}");
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
            API.Log.Error($"[Menu] '{item.Text}' select handler threw: {exception}");
        }
    }

    private void HandleDynamicChanged(Menu menu, MenuDynamicListItem item, string oldValue, string newValue)
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

    private async void HandleDynamicSelect(Menu menu, MenuDynamicListItem item, string value)
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
            API.Log.Error($"[Menu] '{item.Text}' select handler threw: {exception}");
        }
    }

    private void HandleMenuOpen(Menu menu)
    {
        // The bound item table is static and rebindable, and any code can call OpenMenu directly, so
        // the menu re-checks its own gate rather than trusting the door.
        if (!IsReachable())
        {
            menu.CloseMenu();
            Parent?.Menu.OpenMenu();

            return;
        }

        if (_filterDirty)
        {
            ApplyFilter();
        }

        var current = menu.GetCurrentMenuItem();

        ApplyHighlight(current);

        Guard(() => Builder.OnOpened?.Invoke(new MenuOpened(menu, current)), current);
    }

    private void HandleMenuClose(Menu menu) => Guard(() => Builder.OnClosed?.Invoke(menu), null);

    private void HandleIndexChange(Menu menu, MenuItem oldItem, MenuItem newItem, int oldIndex, int newIndex)
    {
        ApplyHighlight(newItem);

        Guard(() => Builder.OnIndexChanged?.Invoke(new MenuIndexChanged(menu, oldItem, newItem, oldIndex, newIndex)), newItem);
    }

    // Drives the stats panels from the highlighted entry, which is why entries model them and
    // ItemData stays free for callers.
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
            API.Log.Error($"[Menu] '{item?.Text ?? "<menu>"}' handler threw: {exception}");
        }
    }
}

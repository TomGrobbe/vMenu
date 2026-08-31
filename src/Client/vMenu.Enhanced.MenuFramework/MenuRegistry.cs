using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Ticks;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Permissions;

using KeyBindingSettings = vMenu.Enhanced.Data.Configuration.Settings.KeyBindings;
using LocalizationSettings = vMenu.Enhanced.Data.Configuration.Settings.Localization;

namespace vMenu.Enhanced.MenuFramework;

// Subscribes to the three change events once and fans out from here rather than one subscription per
// menu, for one place to unsubscribe and a deterministic order.
public static class MenuRegistry
{
    // Settings that provably cannot change what any menu shows, so a refresh pass over every gate and
    // every label is not worth running for them.
    private static readonly Setting[] Ignored =
    [
        // Read once by LanguageLoader before the menus are built. The translation files would not be streamed
        // to the client if changed without a resource restart.
        LocalizationSettings.Languages,

        // Keybinds get registered with the game at startup, so changing one takes a restart either way.
        KeyBindingSettings.MenuToggleKey,
        KeyBindingSettings.NoClipToggleKey,
        KeyBindingSettings.TeleportKey,
    ];

    private static readonly List<MenuHost> Hosts = [];

    private static readonly Dictionary<Menu, MenuHost> HostsByMenu = new(ReferenceComparer<Menu>.Instance);

    private static MenuHost? _root;

    private static bool _built;

    public static Menu? MainMenu => _root?.Menu;

    // MenuController has no way to remove a menu, its tables being static and append only, so a second
    // call would leave the first tree in place and duplicate every row.
    public static async Task BuildAsync(IReadOnlyList<MenuDefinition> definitions)
    {
        if (_built)
        {
            Log.Error("[Menu] BuildAsync was called twice. MenuAPI cannot drop the menus already registered, so this call is being ignored.");
            return;
        }

        _built = true;

        LocalizationSelfCheck.Run();

        var localizer = Localizer.Current;

        MenuText title = MenuText.Key(Loc.MainMenu.Title);
        MenuText subtitle = MenuText.Key(Loc.MainMenu.Subtitle);

        var mainMenu = new Menu(title.Resolve(localizer), MenuHost.ResolveSubtitle(title, subtitle, localizer));

        MenuController.AddMenu(mainMenu);

        _root = Track(new MenuHost(mainMenu, null, MenuGate.Always, title, subtitle, null, "MainMenu"));

        foreach (var definition in definitions)
        {
            if (definition.Title.IsEmpty)
            {
                Log.Error($"[Menu] {definition.GetType().Name} has no title: add a [VMenu(TitleKey = ...)] attribute or override Title.");
            }

            // A submenu entry so the link, its gate and its child menu go through the same path as every nested
            // submenu.
            _root.Builder.Entries.Add(SubmenuEntry.For(definition));
        }

        await MaterialiseAsync(_root, localizer);

        ClientPermissions.PermissionsChanged += RefreshAll;
        ClientConfig.AddEventListenerExcept(Ignored, RefreshAll);
        Localizer.Changed += RefreshAll;

        TickRegistry.Register(
            "Menu.ShadowedControls",
            DisableShadowedControls,
            TickRate.PerFrame,
            MenuController.IsAnyMenuOpen);

        // Items are created enabled, so without this everything looks unlocked until the first permission set
        // lands.
        RefreshAll();

        Log.Debug($"[Menu] Built {Hosts.Count} menu(s).");
    }

    public static void RefreshAll()
    {
        var localizer = Localizer.Current;

        foreach (var host in Hosts)
        {
            host.Refresh(localizer);
        }

        BackOutOfUnreachableMenu();
    }

    // Only the skin's banner changes here, so this deliberately does not go through Refresh: nothing
    // about the rows or the gates is affected.
    internal static void ApplyBanner(string? image)
    {
        foreach (var host in Hosts)
        {
            host.Menu.HeaderImage = image;
        }
    }

    public static void Refresh(Menu menu)
    {
        if (HostsByMenu.TryGetValue(menu, out var host))
        {
            host.Refresh(Localizer.Current);
        }
    }

    public static void Dispose()
    {
        if (!_built)
        {
            return;
        }

        ClientPermissions.PermissionsChanged -= RefreshAll;
        ClientConfig.RemoveEventListenerExcept(RefreshAll);
        Localizer.Changed -= RefreshAll;

        foreach (var host in Hosts)
        {
            host.Dispose();
        }

        Hosts.Clear();
        HostsByMenu.Clear();

        // Leaves MenuAPI holding nothing either, so a rebuild starts from an empty menu list.
        MenuController.RemoveAllMenus();

        _root = null;
        _built = false;
    }

    internal static void MaterialiseLate(MenuHost host, MenuEntry entry)
    {
        MaterialiseOne(host, entry, Localizer.Current);

        // Whole tree, so a submenu created here is gated too.
        RefreshAll();
    }

    // RefreshAll walks every menu in the resource, so doing it per row turns adding a thousand rows into
    // a thousand full tree passes.
    internal static void MaterialiseLateBatch(MenuHost host, IEnumerable<MenuEntry> entries)
    {
        var localizer = Localizer.Current;

        foreach (var entry in entries)
        {
            MaterialiseOne(host, entry, localizer);
        }

        RefreshAll();

        host.RefreshFilter();
    }

    internal static DetachedMenu CreateDetached(
        MenuHost parent,
        MenuText title,
        MenuText subtitle,
        MenuGate gate,
        Action<MenuBuilder> build,
        GateBehaviour? defaultBehaviour)
    {
        var localizer = Localizer.Current;

        var menu = new Menu(title.Resolve(localizer), MenuHost.ResolveSubtitle(title, subtitle, localizer));

        MenuController.AddSubmenu(parent.Menu, menu);

        var child = Track(new MenuHost(menu, parent, gate, title, subtitle, defaultBehaviour, parent.AuditName));

        parent.Children.Add(child);

        build(child.Builder);

        // Nothing walks this menu on the parent's behalf, the parent having no entry for it, so it
        // materialises itself here.
        MaterialiseSync(child, localizer);

        return new DetachedMenu(child);
    }

    private static void MaterialiseOne(MenuHost host, MenuEntry entry, ILocalizer localizer)
    {
        var item = host.Materialise(entry, localizer);

        if (entry is SubmenuEntry submenu && Prepared(submenu) && CreateChild(host, submenu, item, localizer) is { } child)
        {
            MaterialiseSync(child, localizer);
        }
    }

    // For entries added once the menu is live. There is nowhere to await from there, so a definition
    // needing asynchronous preparation is refused rather than built from empty state.
    private static void MaterialiseSync(MenuHost host, ILocalizer localizer)
    {
        for (var index = 0; index < host.Builder.Entries.Count; index++)
        {
            var entry = host.Builder.Entries[index];
            var item = host.Materialise(entry, localizer);

            if (entry is SubmenuEntry submenu && Prepared(submenu) && CreateChild(host, submenu, item, localizer) is { } child)
            {
                MaterialiseSync(child, localizer);
            }
        }

        host.Attach();
    }

    private static bool Prepared(SubmenuEntry submenu)
    {
        if (submenu.Definition is not { } definition)
        {
            return true;
        }

        if (definition.PrepareAsync().IsCompleted)
        {
            return true;
        }

        Log.Error($"[Menu] '{definition.GetType().Name}' needs asynchronous preparation, so it cannot be added after its parent menu was built. Declare it during Build instead.");

        return false;
    }

    private static async Task MaterialiseAsync(MenuHost host, ILocalizer localizer)
    {
        // Indexed, because a definition's Build may append to its own list while the child menus underneath
        // it are still being created.
        for (var index = 0; index < host.Builder.Entries.Count; index++)
        {
            var entry = host.Builder.Entries[index];
            var item = host.Materialise(entry, localizer);

            if (entry is not SubmenuEntry submenu)
            {
                continue;
            }

            if (submenu.Definition is { } definition)
            {
                await definition.PrepareAsync();
            }

            var child = CreateChild(host, submenu, item, localizer);

            if (child is not null)
            {
                await MaterialiseAsync(child, localizer);
            }
        }

        host.Attach();
    }

    private static MenuHost? CreateChild(MenuHost parent, SubmenuEntry submenu, MenuItem linkItem, ILocalizer localizer)
    {
        var definition = submenu.Definition;

        if (definition is null && submenu.Build is null)
        {
            Log.Error($"[Menu] Submenu '{submenu.Text.Resolve(localizer)}' declares neither a Definition nor a Build, so it opens nothing.");
            return null;
        }

        var title = definition?.Title ?? submenu.ResolveTitle();
        var subtitle = definition?.Subtitle ?? submenu.MenuSubtitle;

        var childMenu = new Menu(title.Resolve(localizer), MenuHost.ResolveSubtitle(title, subtitle, localizer));

        // BindMenuItem calls AddSubmenu itself, so a separate call would be redundant.
        MenuController.BindMenuItem(parent.Menu, childMenu, linkItem);

        var child = Track(new MenuHost(
            childMenu,
            parent,
            submenu.EffectiveGate,
            title,
            subtitle,
            definition?.DefaultGateBehaviour,
            definition?.GetType().Name ?? parent.AuditName));

        parent.Children.Add(child);

        if (definition is not null)
        {
            definition.BuildInto(child.Builder);
        }
        else
        {
            submenu.Build!.Invoke(child.Builder);
        }

        // Combined rather than assigned, so declaring OnOpened on the entry does not silently replace
        // whatever the menu itself set during Build.
        child.Builder.OnOpened += submenu.OnOpened;
        child.Builder.OnOpenedAsync += submenu.OnOpenedAsync;

        submenu.Child = child;

        return child;
    }

    // MenuAPI removes the menu behind a row when that row is dropped, so without this the framework would
    // keep walking a host whose menu no longer exists on every gate refresh.
    internal static void Untrack(MenuHost host)
    {
        if (host.Parent is { } parent)
        {
            RemoveByReference(parent.Children, host);
        }

        Drop(host);
    }

    private static void Drop(MenuHost host)
    {
        // Over a copy, because dropping a child takes it out of this list.
        foreach (var child in host.Children.ToArray())
        {
            Drop(child);
        }

        host.Children.Clear();
        host.Dispose();

        // A detached child has no row pointing at it, so MenuAPI cannot work out on its own that it went
        // with its parent. Harmless for a bound one, which MenuAPI has already dropped.
        MenuController.RemoveMenu(host.Menu);

        RemoveByReference(Hosts, host);

        HostsByMenu.Remove(host.Menu);
    }

    // By reference rather than List.Remove, which would reach for EqualityComparer<MenuHost>.Default.
    private static void RemoveByReference(List<MenuHost> hosts, MenuHost host)
    {
        for (var index = hosts.Count - 1; index >= 0; index--)
        {
            if (ReferenceEquals(hosts[index], host))
            {
                hosts.RemoveAt(index);

                return;
            }
        }
    }

    // Walks the framework's own parent links, not Menu.ParentMenu: MenuAPI re-parents a child to whatever
    // menu was open when its link was selected, so that chain describes how the player got here.
    private static void BackOutOfUnreachableMenu()
    {
        if (MenuController.GetCurrentMenu() is not { } open
            || !HostsByMenu.TryGetValue(open, out var host)
            || host.IsReachable())
        {
            return;
        }

        var target = host.Parent;

        while (target is not null && !target.IsReachable())
        {
            target = target.Parent;
        }

        MenuController.CloseAllMenus();

        target?.Menu.OpenMenu();
    }

    private static void DisableShadowedControls()
    {
        if (MenuController.GetCurrentMenu() is not { } open
            || !HostsByMenu.TryGetValue(open, out var host))
        {
            return;
        }

        foreach (var key in host.Builder.Keys)
        {
            if (key.ShadowedControl is { } control)
            {
                Native.DisableControlAction(0, (int)control, true);
            }
        }
    }

    private static MenuHost Track(MenuHost host)
    {
        Hosts.Add(host);
        HostsByMenu[host.Menu] = host;

        return host;
    }
}

using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Permissions;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// Builds the whole menu tree and keeps it in step with permissions and language.
/// </summary>
/// <remarks>
/// Subscribes to <see cref="ClientPermissions.PermissionsChanged"/> and
/// <see cref="Localizer.Changed"/> exactly once and fans out from here, rather than one
/// subscription per menu: fewer delegates, one place to unsubscribe, and a deterministic order.
/// </remarks>
public static class MenuRegistry
{
    private static readonly List<MenuHost> Hosts = [];

    private static readonly Dictionary<Menu, MenuHost> HostsByMenu = new(ReferenceComparer<Menu>.Instance);

    private static MenuHost? _root;

    private static bool _built;

    /// <summary>The main menu, once built.</summary>
    public static Menu? MainMenu => _root?.Menu;

    /// <summary>
    /// Builds the main menu and everything under it, in the order given.
    /// </summary>
    /// <remarks>
    /// Not repeatable. MenuAPI's <see cref="MenuController"/> has no way to remove a menu — its
    /// <c>Menus</c> and bound-item tables are static and append-only — so a second call would leave
    /// the first tree in place and duplicate every row.
    /// </remarks>
    public static async Task BuildAsync(IReadOnlyList<MenuDefinition> definitions)
    {
        if (_built)
        {
            API.Log.Error("[Menu] BuildAsync was called twice. MenuAPI cannot drop the menus already registered, so this call is being ignored.");
            return;
        }

        _built = true;

        LocalizationSelfCheck.Run();

        var localizer = Localizer.Current;

        MenuText title = MenuText.Key(Loc.MainMenu.Title);
        MenuText subtitle = MenuText.Key(Loc.MainMenu.Subtitle);

        var mainMenu = new Menu(title.Resolve(localizer), subtitle.Resolve(localizer));

        MenuController.AddMenu(mainMenu);

        _root = Track(new MenuHost(mainMenu, null, MenuGate.Always, title, subtitle, null));

        foreach (var definition in definitions)
        {
            if (definition.Title.IsEmpty)
            {
                API.Log.Error($"[Menu] {definition.GetType().Name} has no title: add a [VMenu(TitleKey = ...)] attribute or override Title.");
            }

            // Modelled as a submenu entry so the link item, its gate and its child menu all go
            // through the same code path as every nested submenu. No Gate is set here: a submenu
            // entry already folds its definition's gate into the one it evaluates.
            _root.Builder.Entries.Add(new SubmenuEntry
            {
                Text = definition.LinkText,
                Description = definition.LinkDescription,
                Label = definition.LinkLabel,
                Definition = definition,
            });
        }

        await MaterialiseAsync(_root, localizer);

        ClientPermissions.PermissionsChanged += RefreshAll;
        Localizer.Changed += RefreshAll;

        // Items are created enabled, so without this pass everything looks unlocked until the first
        // permission set lands.
        RefreshAll();

        API.Log.Debug($"[Menu] Built {Hosts.Count} menu(s).");
    }

    /// <summary>Re-evaluates every gate and rewrites every label.</summary>
    public static void RefreshAll()
    {
        var localizer = Localizer.Current;

        foreach (var host in Hosts)
        {
            host.Refresh(localizer);
        }

        BackOutOfUnreachableMenu();
    }

    /// <summary>Detaches every subscription. Call when the resource is shutting down or reloading.</summary>
    public static void Dispose()
    {
        if (!_built)
        {
            return;
        }

        ClientPermissions.PermissionsChanged -= RefreshAll;
        Localizer.Changed -= RefreshAll;

        foreach (var host in Hosts)
        {
            host.Dispose();
        }

        Hosts.Clear();
        HostsByMenu.Clear();

        _root = null;
        _built = false;
    }

    /// <summary>Materialises an entry added after its menu was already built.</summary>
    internal static void MaterialiseLate(MenuHost host, MenuEntry entry)
    {
        var localizer = Localizer.Current;
        var item = host.Materialise(entry, localizer);

        if (entry is SubmenuEntry submenu && Prepared(submenu) && CreateChild(host, submenu, item, localizer) is { } child)
        {
            MaterialiseSync(child, localizer);
        }

        // Whole tree rather than just this host, so a submenu created here is gated too.
        RefreshAll();
    }

    /// <summary>
    /// The synchronous counterpart of <see cref="MaterialiseAsync"/>, for entries added once the
    /// menu is already live. There is nowhere to await from there, so a definition that genuinely
    /// needs asynchronous preparation is refused rather than silently built from empty state.
    /// </summary>
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

        API.Log.Error($"[Menu] '{definition.GetType().Name}' needs asynchronous preparation, so it cannot be added after its parent menu was built. Declare it during Build instead.");

        return false;
    }

    private static async Task MaterialiseAsync(MenuHost host, ILocalizer localizer)
    {
        // Indexed rather than foreach: a definition's Build may append to its own list while the
        // child menus underneath it are still being created.
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
            API.Log.Error($"[Menu] Submenu '{submenu.Text.Resolve(localizer)}' declares neither a Definition nor a Build, so it opens nothing.");
            return null;
        }

        var title = definition?.Title ?? submenu.ResolveTitle();
        var subtitle = definition?.Subtitle ?? submenu.MenuSubtitle;

        var childMenu = new Menu(title.Resolve(localizer), subtitle.Resolve(localizer));

        // BindMenuItem calls AddSubmenu itself, so a separate call would be redundant.
        MenuController.BindMenuItem(parent.Menu, childMenu, linkItem);

        var child = Track(new MenuHost(
            childMenu,
            parent,
            submenu.EffectiveGate,
            title,
            subtitle,
            definition?.DefaultGateBehaviour));

        parent.Children.Add(child);

        if (definition is not null)
        {
            definition.BuildInto(child.Builder);
        }
        else
        {
            submenu.Build!.Invoke(child.Builder);
        }

        // Combined rather than assigned, so declaring OnOpened on the entry does not silently
        // replace whatever the menu itself set during Build.
        child.Builder.OnOpened += submenu.OnOpened;

        return child;
    }

    /// <summary>
    /// Returns the player to the nearest menu they can still reach.
    /// </summary>
    /// <remarks>
    /// Walks the framework's own parent links rather than <c>Menu.ParentMenu</c>: MenuAPI re-parents
    /// a child to whatever menu was open when its link was selected, so that chain describes how the
    /// player got here, not where the menu belongs.
    /// </remarks>
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

    private static MenuHost Track(MenuHost host)
    {
        Hosts.Add(host);
        HostsByMenu[host.Menu] = host;

        return host;
    }
}

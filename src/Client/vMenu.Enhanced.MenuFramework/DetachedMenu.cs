using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

// A child menu with no row pointing at it, opened from code instead. For a detail menu behind a list
// of rows that is rebuilt at runtime: one SubmenuEntry per row would mean one whole child menu per
// row, which is fine for six rows and ruinous for two thousand.
public sealed class DetachedMenu
{
    private readonly MenuHost _host;

    internal DetachedMenu(MenuHost host) => _host = host;

    public Menu Menu => _host.Menu;

    public MenuBuilder Builder => _host.Builder;

    // Closes whatever menu is open and opens this one, so the back button returns there. Re-parenting on
    // the way in is what MenuAPI does for a bound submenu item.
    public void Open()
    {
        // Re-resolved here rather than trusted from the last gating pass. A detached menu exists because
        // what it says depends on whatever the caller just picked, and that pass ran long before they did.
        Refresh();

        if (MenuController.GetCurrentMenu() is { } current && !ReferenceEquals(current, Menu))
        {
            MenuController.AddSubmenu(current, Menu);
            current.CloseMenu();
        }

        Menu.OpenMenu();
    }

    // Called by Open, so most menus never need this. It is here for a title that changes while the menu
    // is already on screen.
    public void Refresh() => _host.Refresh(Localizer.Current);

    // A detached menu is the one kind nothing can clean up by itself: no row points at it, so dropping
    // rows can never make it unreachable. Whoever asked for it has to say when it is done.
    public void Remove()
    {
        // Through the host, so the framework untracks the hosts behind any submenu rows it had.
        _host.ClearEntries();

        MenuRegistry.Untrack(_host);
    }
}

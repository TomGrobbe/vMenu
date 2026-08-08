using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// A child menu with no row pointing at it, opened from code instead.
/// </summary>
// For a detail menu behind a list of rows that is rebuilt at runtime. One SubmenuEntry per row would
// mean one whole child menu per row, which is fine for six rows and ruinous for two thousand.
public sealed class DetachedMenu
{
    private readonly MenuHost _host;

    internal DetachedMenu(MenuHost host) => _host = host;

    public Menu Menu => _host.Menu;

    /// <summary>Its contents, for adding rows after it was declared.</summary>
    public MenuBuilder Builder => _host.Builder;

    /// <summary>
    /// Closes whatever menu is open and opens this one, so the back button returns there.
    /// </summary>
    // Re-parenting on the way in is what MenuAPI does for a bound submenu item, and it is what makes
    // back work no matter which menu opened this one.
    public void Open()
    {
        // Re-resolved here rather than trusted from the last gating pass. A detached menu exists
        // because what it says depends on whatever the caller just picked, and that pass ran long
        // before they picked it.
        Refresh();

        if (MenuController.GetCurrentMenu() is { } current && !ReferenceEquals(current, Menu))
        {
            MenuController.AddSubmenu(current, Menu);
            current.CloseMenu();
        }

        Menu.OpenMenu();
    }

    /// <summary>
    /// Rewrites the title, the subtitle and every row from their declarations.
    /// </summary>
    // Called by Open, so most menus never need this. It is here for a title that changes while the
    // menu is already on screen.
    public void Refresh() => _host.Refresh(Localizer.Current);
}

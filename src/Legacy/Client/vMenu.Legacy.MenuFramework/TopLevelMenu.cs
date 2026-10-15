using MenuAPI;

namespace vMenu.Enhanced.MenuFramework;

// No Open on purpose: DetachedMenu.Open would give it a parent, and back would stop closing it.
public sealed class TopLevelMenu
{
    private readonly MenuHost _host;

    internal TopLevelMenu(MenuHost host) => _host = host;

    public Menu Menu => _host.Menu;

    public MenuBuilder Builder => _host.Builder;

    public void Remove() => MenuRegistry.Remove(_host);
}

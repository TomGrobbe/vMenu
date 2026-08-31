using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

public sealed class MenuBuilder
{
    private readonly MenuHost _host;

    internal MenuBuilder(MenuHost host) => _host = host;

    // Mutable so a menu can append entries generated from runtime data before anything is materialised.
    public List<MenuEntry> Entries { get; } = [];

    public Menu Menu => _host.Menu;

    public List<MenuKey> Keys { get; } = [];

    internal List<Menu.KeyBindingHandler> Registered { get; } = [];

    // Null inherits MenuFrameworkOptions.DefaultGateBehaviour.
    public GateBehaviour? DefaultGateBehaviour { get; set; }

    public Action<MenuOpened>? OnOpened { get; set; }

    public Func<MenuOpened, Task>? OnOpenedAsync { get; set; }

    public Action<Menu>? OnClosed { get; set; }

    public Action<MenuIndexChanged>? OnIndexChanged { get; set; }

    // Safe to call after the menu has been built, in which case the entry is materialised and gated at once.
    public T Add<T>(T entry)
        where T : MenuEntry
    {
        Entries.Add(entry);

        if (_host.IsLive)
        {
            MenuRegistry.MaterialiseLate(_host, entry);
        }

        return entry;
    }

    // Much cheaper than Add in a loop once the menu is live, because the gating pass runs once at the end
    // rather than once per entry.
    public void AddRange(IEnumerable<MenuEntry> entries)
    {
        if (!_host.IsLive)
        {
            Entries.AddRange(entries);
            return;
        }

        // Materialised out of a copy, since the batch is walked twice and the caller may well have handed us
        // a lazy query over the list we are about to append to.
        var batch = entries.ToList();

        Entries.AddRange(batch);

        MenuRegistry.MaterialiseLateBatch(_host, batch);
    }

    // A submenu row takes its child menu with it, since nothing could reach that menu once the row
    // opening it is gone. Declare the row again to get a fresh one.
    public void ClearEntries() => _host.ClearEntries();

    // Worth reaching for over a SubmenuEntry when a long list of rows shares one detail menu: a submenu
    // entry builds one child menu per row, which is the wrong shape when there could be thousands of
    // them. Title and subtitle resolve on every refresh, so pass MenuText.From to follow the selection.
    public DetachedMenu AddDetachedMenu(
        MenuText title,
        MenuText subtitle,
        Action<MenuBuilder> build,
        MenuGate? gate = null) =>
        MenuRegistry.CreateDetached(_host, title, subtitle, gate ?? MenuGate.Always, build, DefaultGateBehaviour);

    // Its text is never rewritten, so it does not translate. Registering it rather than ignoring it keeps
    // the arrow keys from changing a raw list or slider that has been locked.
    public RawEntry AddRaw(MenuItem item) => Add(new RawEntry(item));

    // MenuAPI's sort drops the filter, so it is put back afterwards.
    public void SortItems(Comparison<MenuItem> comparison) => _host.SortItems(comparison);

    // An extra visibility predicate, combined with gate hiding. Pass null to clear it.
    public void SetUserFilter(Func<MenuItem, bool>? predicate) => _host.SetUserFilter(predicate);
}

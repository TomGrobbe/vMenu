using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

public sealed class MenuBuilder
{
    private readonly MenuHost _host;

    internal MenuBuilder(MenuHost host) => _host = host;

    /// <summary>Mutable so a menu can append entries generated from runtime data before anything is materialised.</summary>
    public List<MenuEntry> Entries { get; } = [];

    public Menu Menu => _host.Menu;

    public List<MenuKey> Keys { get; } = [];

    internal List<Menu.KeyBindingHandler> Registered { get; } = [];

    /// <summary>Null inherits MenuFrameworkOptions.DefaultGateBehaviour.</summary>
    public GateBehaviour? DefaultGateBehaviour { get; set; }

    public Action<MenuOpened>? OnOpened { get; set; }

    public Func<MenuOpened, Task>? OnOpenedAsync { get; set; }

    public Action<Menu>? OnClosed { get; set; }

    public Action<MenuIndexChanged>? OnIndexChanged { get; set; }

    /// <summary>Safe to call after the menu has been built, in which case the entry is materialised and gated at once.</summary>
    public T Add<T>(T entry)
        where T : MenuEntry =>
        Insert(Entries.Count, entry);

    /// <summary>Inserts an entry at <paramref name="index"/>. Once the menu is live it is materialised and gated at
    /// once, and the highlighted item stays highlighted.</summary>
    public T Insert<T>(int index, T entry)
        where T : MenuEntry
    {
        _host.InsertEntry(index, entry);

        return entry;
    }

    /// <summary>Much cheaper than Add in a loop once the menu is live, because the gating pass runs once at the end
    /// rather than once per entry.</summary>
    public void AddRange(IEnumerable<MenuEntry> entries) => InsertRange(Entries.Count, entries);

    /// <summary>Inserts entries in order starting at <paramref name="index"/>, with one refresh at the end.</summary>
    public void InsertRange(int index, IEnumerable<MenuEntry> entries)
    {
        // Materialised out of a copy, since the caller may well have handed us a lazy query over the list we
        // are about to insert into.
        var batch = entries.ToList();

        index = Math.Clamp(index, 0, Entries.Count);

        using (BeginUpdate())
        {
            foreach (var entry in batch)
            {
                _host.InsertEntry(index++, entry);
            }
        }
    }

    /// <summary>Removes one entry. A submenu entry takes its child menu with it. False when the entry is not in this menu.</summary>
    public bool Remove(MenuEntry entry) => _host.RemoveEntry(entry);

    /// <summary>Moves an entry to <paramref name="index"/>. A submenu entry keeps its child menu, and the highlighted
    /// item stays highlighted. False when the entry is not in this menu.</summary>
    public bool Move(MenuEntry entry, int index) => _host.MoveEntry(entry, index);

    /// <summary>Moves the cursor to an entry, raising the same highlight and index change events as the player moving
    /// there. False when the entry is not shown.</summary>
    public bool Select(MenuEntry entry) => _host.SelectEntry(entry);

    /// <summary>Holds back the refresh and filter pass of Insert, Remove and Move until the returned handle is disposed,
    /// so many changes cost one pass. Nesting is fine.</summary>
    public IDisposable BeginUpdate() => _host.BeginUpdate();

    /// <summary>Re-reads text, gates and state of every entry in this menu only.</summary>
    public void Refresh() => _host.Refresh(Localizer.Current);

    /// <summary>Applies the gate and user filter again, keeping the cursor on its item.</summary>
    public void RefreshFilter() => _host.RefreshFilter();

    /// <summary>Asks for a <see cref="Refresh"/>, run when the current <see cref="BeginUpdate"/> ends or right away
    /// outside one.</summary>
    public void Invalidate() => _host.Invalidate(refresh: true, filter: false);

    /// <summary>Asks for a <see cref="RefreshFilter"/>, run when the current <see cref="BeginUpdate"/> ends or right
    /// away outside one.</summary>
    public void InvalidateFilter() => _host.Invalidate(refresh: false, filter: true);

    /// <summary>A submenu row takes its child menu with it, since nothing could reach that menu once the row
    /// opening it is gone. Declare the row again to get a fresh one.</summary>
    public void ClearEntries() => _host.ClearEntries();

    /// <summary>Worth reaching for over a SubmenuEntry when a long list of rows shares one detail menu: a submenu
    /// entry builds one child menu per row, which is the wrong shape when there could be thousands of
    /// them. Title and subtitle resolve on every refresh, so pass MenuText.From to follow the selection.</summary>
    public DetachedMenu AddDetachedMenu(
        MenuText title,
        MenuText subtitle,
        Action<MenuBuilder> build,
        MenuGate? gate = null) =>
        MenuRegistry.CreateDetached(_host, title, subtitle, gate ?? MenuGate.Always, build, DefaultGateBehaviour);

    /// <summary>Its text is never rewritten, so it does not translate. Registering it rather than ignoring it keeps
    /// the arrow keys from changing a raw list or slider that has been locked.</summary>
    public RawEntry AddRaw(MenuItem item) => Add(new RawEntry(item));

    /// <summary>MenuAPI's sort drops the filter, so it is put back afterwards.</summary>
    public void SortItems(Comparison<MenuItem> comparison) => _host.SortItems(comparison);

    /// <summary>An extra visibility predicate, combined with gate hiding. Pass null to clear it.</summary>
    public void SetUserFilter(Func<MenuItem, bool>? predicate) => _host.SetUserFilter(predicate);
}

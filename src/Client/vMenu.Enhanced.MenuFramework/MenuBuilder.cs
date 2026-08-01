using MenuAPI;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// The surface a <see cref="MenuDefinition"/> uses to declare its contents.
/// </summary>
public sealed class MenuBuilder
{
    private readonly MenuHost _host;

    internal MenuBuilder(MenuHost host) => _host = host;

    /// <summary>
    /// The declared rows, in display order.
    /// </summary>
    /// <remarks>
    /// Mutable on purpose: a menu can write its fixed rows as one collection expression and then
    /// append whatever runtime data produces, in the same method, before anything is materialised.
    /// </remarks>
    public List<MenuEntry> Entries { get; } = [];

    /// <summary>The MenuAPI menu, for the occasional thing the framework does not model.</summary>
    public Menu Menu => _host.Menu;

    /// <summary>Null inherits <see cref="MenuFrameworkOptions.DefaultGateBehaviour"/>.</summary>
    public GateBehaviour? DefaultGateBehaviour { get; set; }

    public Action<MenuOpened>? OnOpened { get; set; }

    public Action<Menu>? OnClosed { get; set; }

    public Action<MenuIndexChanged>? OnIndexChanged { get; set; }

    /// <summary>
    /// Appends an entry and returns it, so it can be captured. Safe to call after the menu has been
    /// built: the entry is materialised and gated immediately in that case.
    /// </summary>
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

    /// <summary>
    /// Adds an item built by hand.
    /// </summary>
    /// <remarks>
    /// Registering it rather than ignoring it is what keeps the arrow keys from changing a raw list
    /// or slider that has been locked. Its text is never rewritten, so it does not translate.
    /// </remarks>
    public RawEntry AddRaw(MenuItem item) => Add(new RawEntry(item));

    /// <summary>Sorts the visible items, then restores the filter that MenuAPI's sort silently drops.</summary>
    public void SortItems(Comparison<MenuItem> comparison) => _host.SortItems(comparison);

    /// <summary>
    /// An extra visibility predicate, combined with gate-hiding. The seam for something like a
    /// search box; pass null to clear it.
    /// </summary>
    public void SetUserFilter(Func<MenuItem, bool>? predicate) => _host.SetUserFilter(predicate);
}

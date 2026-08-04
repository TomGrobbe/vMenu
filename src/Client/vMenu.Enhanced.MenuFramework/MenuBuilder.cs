using MenuAPI;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>The surface a <see cref="MenuDefinition"/> uses to declare its contents.</summary>
public sealed class MenuBuilder
{
    private readonly MenuHost _host;

    internal MenuBuilder(MenuHost host) => _host = host;

    /// <summary>The declared rows, in display order.</summary>
    // Mutable so a menu can append entries generated from runtime data before anything is
    // materialised.
    public List<MenuEntry> Entries { get; } = [];

    /// <summary>The MenuAPI menu, for the occasional thing the framework does not model.</summary>
    public Menu Menu => _host.Menu;

    /// <summary>Null inherits <see cref="MenuFrameworkOptions.DefaultGateBehaviour"/>.</summary>
    public GateBehaviour? DefaultGateBehaviour { get; set; }

    public Action<MenuOpened>? OnOpened { get; set; }

    public Action<Menu>? OnClosed { get; set; }

    public Action<MenuIndexChanged>? OnIndexChanged { get; set; }

    /// <summary>
    /// Appends an entry and returns it. Safe to call after the menu has been built, in which case
    /// the entry is materialised and gated immediately.
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

    /// <summary>Adds an item built by hand. Its text is never rewritten, so it does not translate.</summary>
    // Registering it rather than ignoring it keeps the arrow keys from changing a raw list or slider
    // that has been locked.
    public RawEntry AddRaw(MenuItem item) => Add(new RawEntry(item));

    /// <summary>Sorts the visible items, then restores the filter that MenuAPI's sort drops.</summary>
    public void SortItems(Comparison<MenuItem> comparison) => _host.SortItems(comparison);

    /// <summary>An extra visibility predicate, combined with gate hiding. Pass null to clear it.</summary>
    public void SetUserFilter(Func<MenuItem, bool>? predicate) => _host.SetUserFilter(predicate);
}

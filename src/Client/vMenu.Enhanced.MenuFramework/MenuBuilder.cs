using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

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

    /// <summary>
    /// Extra button hints along the bottom of the screen, on top of the select and back MenuAPI draws
    /// itself. Resolved on every refresh, so they follow a language change like everything else.
    /// </summary>
    public List<ButtonHint> InstructionalButtons { get; } = [];

    /// <summary>Null inherits <see cref="MenuFrameworkOptions.DefaultGateBehaviour"/>.</summary>
    public GateBehaviour? DefaultGateBehaviour { get; set; }

    public Action<MenuOpened>? OnOpened { get; set; }

    public Func<MenuOpened, Task>? OnOpenedAsync { get; set; }

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

    /// <summary>
    /// Appends several entries at once. Much cheaper than <see cref="Add"/> in a loop once the menu
    /// is live, because the gating pass runs once at the end rather than once per entry.
    /// </summary>
    public void AddRange(IEnumerable<MenuEntry> entries)
    {
        if (!_host.IsLive)
        {
            Entries.AddRange(entries);
            return;
        }

        // Materialised out of a copy, since the batch is walked twice and the caller may well have
        // handed us a lazy query over the list we are about to append to.
        var batch = entries.ToList();

        Entries.AddRange(batch);

        MenuRegistry.MaterialiseLateBatch(_host, batch);
    }

    /// <summary>
    /// Removes every row, so the menu can be filled with a fresh set. For a menu whose contents are
    /// runtime data rather than a fixed declaration.
    /// </summary>
    // A submenu row takes its child menu with it, since nothing could reach that menu once the row
    // opening it is gone. Declare the row again to get a fresh one.
    public void ClearEntries() => _host.ClearEntries();

    /// <summary>
    /// Declares a child menu that no row points at, returned as a handle you open from code.
    /// </summary>
    /// <remarks>
    /// The reason to reach for this over a <see cref="SubmenuEntry"/> is a detail menu shared by a
    /// long list of rows. A submenu entry builds one child menu per row, which is the wrong shape
    /// when the rows are runtime data and there could be thousands of them. Point every row at the
    /// same detached menu instead, and have the row record what it was before opening it.
    ///
    /// <para>
    /// Its title and subtitle are resolved on every refresh, so pass
    /// <see cref="MenuText.From(Func{string})"/> to have them follow whatever the rows selected.
    /// </para>
    /// </remarks>
    /// <param name="title">The banner text.</param>
    /// <param name="subtitle">The text in the bar below the banner.</param>
    /// <param name="build">Declares the menu's rows, exactly like a definition's own build.</param>
    /// <param name="gate">Who may open it. Combined with the gates of every menu above it.</param>
    public DetachedMenu AddDetachedMenu(
        MenuText title,
        MenuText subtitle,
        Action<MenuBuilder> build,
        MenuGate? gate = null) =>
        MenuRegistry.CreateDetached(_host, title, subtitle, gate ?? MenuGate.Always, build, DefaultGateBehaviour);

    /// <summary>Adds an item built by hand. Its text is never rewritten, so it does not translate.</summary>
    // Registering it rather than ignoring it keeps the arrow keys from changing a raw list or slider
    // that has been locked.
    public RawEntry AddRaw(MenuItem item) => Add(new RawEntry(item));

    /// <summary>Sorts the visible items, then restores the filter that MenuAPI's sort drops.</summary>
    public void SortItems(Comparison<MenuItem> comparison) => _host.SortItems(comparison);

    /// <summary>An extra visibility predicate, combined with gate hiding. Pass null to clear it.</summary>
    public void SetUserFilter(Func<MenuItem, bool>? predicate) => _host.SetUserFilter(predicate);
}

using vMenu.Enhanced.Localization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// One menu, declared. Subclassed by every concrete menu.
/// </summary>
/// <remarks>
/// The framework owns the MenuAPI <c>Menu</c>, its event subscriptions and its lifetime; a
/// definition only says what should be in it.
/// </remarks>
public abstract class MenuDefinition
{
    private VMenuMetadata? _metadata;

    internal VMenuMetadata Metadata => _metadata ??= VMenuMetadata.For(GetType());

    public virtual MenuText Title => Metadata.Title;

    public virtual MenuText Subtitle => Metadata.Subtitle;

    /// <summary>Text of the item that opens this menu from its parent.</summary>
    public virtual MenuText LinkText => Title;

    public virtual MenuText LinkDescription => Metadata.LinkDescription;

    public virtual MenuText LinkLabel => Metadata.LinkLabel;

    /// <summary>
    /// Gates the item that opens this menu. That is enough to gate the menu as a whole — MenuAPI
    /// will not open a submenu from a disabled item.
    /// </summary>
    public virtual MenuGate Gate => Metadata.Gate;

    /// <summary>Null inherits <see cref="MenuFrameworkOptions.DefaultGateBehaviour"/>.</summary>
    public virtual GateBehaviour? DefaultGateBehaviour => null;

    /// <summary>
    /// Anything that has to be fetched or computed before <see cref="Build"/> can declare entries.
    /// Runs once, before the menu is materialised.
    /// </summary>
    public virtual Task PrepareAsync() => Task.CompletedTask;

    /// <summary>
    /// Declares the menu's contents. Append to <see cref="MenuBuilder.Entries"/>; the list is
    /// mutable so a menu can mix a static block with entries generated from runtime data.
    /// </summary>
    protected abstract void Build(MenuBuilder menu);

    /// <summary>
    /// Lets the registry drive <see cref="Build"/>, which stays protected: a menu overriding it from
    /// another assembly could not widen a <c>protected internal</c> member anyway.
    /// </summary>
    internal void BuildInto(MenuBuilder builder) => Build(builder);
}

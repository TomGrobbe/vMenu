using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>One menu, declared. Subclassed by every concrete menu.</summary>
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

    /// <summary>Gates the item that opens this menu, which gates the menu as a whole.</summary>
    public virtual MenuGate Gate => Metadata.Gate;

    /// <summary>Null inherits <see cref="MenuFrameworkOptions.DefaultGateBehaviour"/>.</summary>
    public virtual GateBehaviour? DefaultGateBehaviour => null;

    /// <summary>What the item that opens this menu looks like when <see cref="Gate"/> denies.</summary>
    public virtual GateBehaviour? LinkBehaviour => null;

    /// <summary>Anything to fetch or compute before <see cref="Build"/> can declare entries.</summary>
    public virtual Task PrepareAsync() => Task.CompletedTask;

    /// <summary>Declares the menu's contents by appending to <see cref="MenuBuilder.Entries"/>.</summary>
    protected abstract void Build(MenuBuilder menu);

    // Lets the registry drive Build, which stays protected.
    internal void BuildInto(MenuBuilder builder) => Build(builder);
}

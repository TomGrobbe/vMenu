using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

public abstract class MenuDefinition
{
    private VMenuMetadata? _metadata;

    internal VMenuMetadata Metadata => _metadata ??= VMenuMetadata.For(GetType());

    public virtual MenuText Title => Metadata.Title;

    public virtual MenuText Subtitle => Metadata.Subtitle;

    // Text of the item that opens this menu from its parent.
    public virtual MenuText LinkText => Title;

    public virtual MenuText LinkDescription => Metadata.LinkDescription;

    public virtual MenuText LinkLabel => Metadata.LinkLabel;

    // Gates the item that opens this menu, which gates the menu as a whole.
    public virtual MenuGate Gate => Metadata.Gate;

    // Null inherits MenuFrameworkOptions.DefaultGateBehaviour.
    public virtual GateBehaviour? DefaultGateBehaviour => null;

    // What the item that opens this menu looks like when Gate denies.
    public virtual GateBehaviour? LinkBehaviour => null;

    // Anything to fetch or compute before Build can declare entries.
    public virtual Task PrepareAsync() => Task.CompletedTask;

    // Declares the menu's contents by appending to MenuBuilder.Entries.
    protected abstract void Build(MenuBuilder menu);

    // Lets the registry drive Build, which stays protected.
    internal void BuildInto(MenuBuilder builder) => Build(builder);
}

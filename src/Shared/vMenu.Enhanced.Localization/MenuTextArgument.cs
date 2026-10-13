namespace vMenu.Enhanced.Localization;

internal sealed class MenuTextArgument(string name, MenuText value)
{
    public string Name { get; } = name;

    public MenuText Value { get; } = value;
}

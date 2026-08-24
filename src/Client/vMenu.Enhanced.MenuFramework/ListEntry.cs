using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

// A row whose value is chosen from a fixed list. Options are MenuText so they translate. Values that
// are data rather than prose, such as model names, must use MenuText.Literal or a language change
// reports them as missing keys.
public sealed class ListEntry : MenuEntry<MenuListItem>
{
    public required IReadOnlyList<MenuText> Options { get; init; }

    // The starting selection. Ignored when ReadSelectedIndex is set.
    public int SelectedIndex { get; init; }

    public Func<int>? ReadSelectedIndex { get; init; }

    public Action<ListSelected>? OnSelected { get; init; }

    public Func<ListSelected, Task>? OnSelectedAsync { get; init; }

    public Action<ListIndexChanged>? OnIndexChanged { get; init; }

    protected override MenuListItem Create(ILocalizer localizer)
    {
        var options = ListOptions.Resolve(Options, localizer);

        return new MenuListItem(
            Text.Resolve(localizer),
            options,
            ListOptions.Clamp(ReadSelectedIndex?.Invoke() ?? SelectedIndex, options.Count),
            Description.Resolve(localizer));
    }

    internal override void ApplyPresentation(ILocalizer localizer, GateBehaviour behaviour)
    {
        base.ApplyPresentation(localizer, behaviour);

        if (Typed is { } item)
        {
            ListOptions.Rewrite(item, Options, ReadSelectedIndex?.Invoke() ?? item.ListIndex, localizer);
        }
    }
}

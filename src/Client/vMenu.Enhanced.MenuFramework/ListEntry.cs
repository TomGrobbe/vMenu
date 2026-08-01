using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// A row whose value is chosen from a fixed list.
/// </summary>
/// <remarks>
/// Options are <see cref="MenuText"/> so they translate with everything else. Values that are data
/// rather than prose — model names, scenario names — must be declared with
/// <see cref="MenuText.Literal"/>, otherwise a language change will report them as missing keys.
/// </remarks>
public sealed class ListEntry : MenuEntry<MenuListItem>
{
    public required IReadOnlyList<MenuText> Options { get; init; }

    /// <summary>The starting selection. Ignored when <see cref="ReadSelectedIndex"/> is set.</summary>
    public int SelectedIndex { get; init; }

    public Func<int>? ReadSelectedIndex { get; init; }

    public Action<ListSelected>? OnSelected { get; init; }

    public Func<ListSelected, Task>? OnSelectedAsync { get; init; }

    public Action<ListIndexChanged>? OnIndexChanged { get; init; }

    protected override MenuListItem Create(ILocalizer localizer)
    {
        var options = Resolve(localizer);

        return new MenuListItem(
            Text.Resolve(localizer),
            options,
            Clamp(ReadSelectedIndex?.Invoke() ?? SelectedIndex, options.Count),
            Description.Resolve(localizer));
    }

    internal override void ApplyPresentation(ILocalizer localizer, GateBehaviour behaviour)
    {
        base.ApplyPresentation(localizer, behaviour);

        if (Typed is not { } item)
        {
            return;
        }

        // Rewritten in place rather than replaced, so the current selection survives a language
        // change: list values are addressed by index, never by their text.
        var selected = ReadSelectedIndex?.Invoke() ?? item.ListIndex;

        item.ListItems.Clear();

        foreach (var option in Options)
        {
            item.ListItems.Add(option.Resolve(localizer));
        }

        item.ListIndex = Clamp(selected, item.ListItems.Count);
    }

    private List<string> Resolve(ILocalizer localizer)
    {
        var options = new List<string>(Options.Count);

        foreach (var option in Options)
        {
            options.Add(option.Resolve(localizer));
        }

        return options;
    }

    /// <summary>MenuAPI appends an "N/A" entry to an empty list while drawing, so zero is still valid.</summary>
    private static int Clamp(int index, int count) => Math.Clamp(index, 0, Math.Max(0, count - 1));
}

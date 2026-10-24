using System.Diagnostics.CodeAnalysis;

using MenuAPI;

using vMenu.Enhanced.Localization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>A row whose value is chosen from a fixed list. Options are MenuText so they translate. Values that
/// are data rather than prose, such as model names, must use MenuText.Literal or a language change
/// reports them as missing keys.</summary>
public sealed class ListEntry : MenuEntry<MenuListItem>
{
    /// <summary>For text that is not a plain translation key: set <see cref="MenuEntry.Text"/> in the initializer.</summary>
    public ListEntry()
    {
    }

    /// <summary>Takes the text from <paramref name="textKey"/>, and the description from <c>textKey + ".desc"</c> when
    /// English has that key. Setting Description in the initializer still wins.</summary>
    [SetsRequiredMembers]
    public ListEntry(string textKey, IReadOnlyList<MenuText> options)
        : base(textKey)
    {
        Options = options;
    }

    public required IReadOnlyList<MenuText> Options { get; init; }

    /// <summary>The starting selection. Ignored when ReadSelectedIndex is set.</summary>
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

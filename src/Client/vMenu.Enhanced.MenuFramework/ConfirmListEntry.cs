using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// A row that picks a value from a list and asks before it acts on it. The first press only turns
/// its description into a warning; the second one runs the handler. Scrolling the value away puts
/// the row back to asking, so nobody confirms one thing and deletes another.
/// </summary>
public sealed class ConfirmListEntry : ConfirmEntry<MenuListItem>
{
    public required IReadOnlyList<MenuText> Options { get; init; }

    /// <summary>The starting selection. Ignored when <see cref="ReadSelectedIndex"/> is set.</summary>
    public int SelectedIndex { get; init; }

    public Func<int>? ReadSelectedIndex { get; init; }

    public Action<ListSelected>? OnConfirmed { get; init; }

    /// <summary>Runs after <see cref="OnConfirmed"/>. Exceptions are logged, never left unobserved.</summary>
    public Func<ListSelected, Task>? OnConfirmedAsync { get; init; }

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

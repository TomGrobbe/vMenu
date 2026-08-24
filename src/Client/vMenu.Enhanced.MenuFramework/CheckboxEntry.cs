using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

public sealed class CheckboxEntry : MenuEntry<MenuCheckboxItem>
{
    // The starting state. Ignored when ReadState is set.
    public bool Checked { get; init; }

    // A live source for the state, re-read on every refresh. Use it when something other than this
    // checkbox can change the value, so the tick cannot drift out of sync with reality.
    public Func<bool>? ReadState { get; init; }

    public MenuCheckboxItem.CheckboxStyle Style { get; init; } = MenuCheckboxItem.CheckboxStyle.Tick;

    public Action<CheckboxChanged>? OnChanged { get; init; }

    public Func<CheckboxChanged, Task>? OnChangedAsync { get; init; }

    protected override MenuCheckboxItem Create(ILocalizer localizer) =>
        new(Text.Resolve(localizer), Description.Resolve(localizer), ReadState?.Invoke() ?? Checked)
        {
            Style = Style,
        };

    internal override void ApplyPresentation(ILocalizer localizer, GateBehaviour behaviour)
    {
        base.ApplyPresentation(localizer, behaviour);

        if (ReadState is not null && Typed is { } item)
        {
            item.Checked = ReadState();
        }
    }
}

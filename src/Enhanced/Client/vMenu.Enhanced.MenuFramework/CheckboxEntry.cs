using System.Diagnostics.CodeAnalysis;

using MenuAPI;

using vMenu.Enhanced.Localization;

namespace vMenu.Enhanced.MenuFramework;

public sealed class CheckboxEntry : MenuEntry<MenuCheckboxItem>
{
    /// <summary>For text that is not a plain translation key: set <see cref="MenuEntry.Text"/> in the initializer.</summary>
    public CheckboxEntry()
    {
    }

    /// <summary>Takes the text from <paramref name="textKey"/>, and the description from <c>textKey + ".desc"</c> when
    /// English has that key. Setting Description in the initializer still wins.</summary>
    [SetsRequiredMembers]
    public CheckboxEntry(string textKey)
        : base(textKey)
    {
    }

    /// <summary>The starting state. Ignored when ReadState is set.</summary>
    public bool Checked { get; init; }

    /// <summary>A live source for the state, re-read on every refresh. Use it when something other than this
    /// checkbox can change the value, so the tick cannot drift out of sync with reality.</summary>
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

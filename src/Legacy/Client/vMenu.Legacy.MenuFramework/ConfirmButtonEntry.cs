using System.Diagnostics.CodeAnalysis;

using MenuAPI;

using vMenu.Enhanced.Localization;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>A plain row that asks before it does anything. The first press only turns its description into a
/// warning; the second one runs the handler.</summary>
public sealed class ConfirmButtonEntry : ConfirmEntry<MenuItem>
{
    /// <summary>For text that is not a plain translation key: set <see cref="MenuEntry.Text"/> in the initializer.</summary>
    public ConfirmButtonEntry()
    {
    }

    /// <summary>Takes the text from <paramref name="textKey"/>, and the description from <c>textKey + ".desc"</c> when
    /// English has that key. Setting Description in the initializer still wins.</summary>
    [SetsRequiredMembers]
    public ConfirmButtonEntry(string textKey)
        : base(textKey)
    {
    }

    public Action<ItemSelected>? OnConfirmed { get; init; }

    /// <summary>Runs after OnConfirmed. Exceptions are logged, never left unobserved.</summary>
    public Func<ItemSelected, Task>? OnConfirmedAsync { get; init; }

    protected override MenuItem Create(ILocalizer localizer) =>
        new(Text.Resolve(localizer), Description.Resolve(localizer));
}

using System.Diagnostics.CodeAnalysis;

using MenuAPI;

using vMenu.Enhanced.Localization;

namespace vMenu.Enhanced.MenuFramework;

public sealed class ButtonEntry : MenuEntry<MenuItem>
{
    /// <summary>For text that is not a plain translation key: set <see cref="MenuEntry.Text"/> in the initializer.</summary>
    public ButtonEntry()
    {
    }

    /// <summary>Takes the text from <paramref name="textKey"/>, and the description from <c>textKey + ".desc"</c> when
    /// English has that key. Setting Description in the initializer still wins.</summary>
    [SetsRequiredMembers]
    public ButtonEntry(string textKey)
        : base(textKey)
    {
    }

    public Action<ItemSelected>? OnSelected { get; init; }

    /// <summary>Runs after OnSelected. Exceptions are logged, never left unobserved.</summary>
    public Func<ItemSelected, Task>? OnSelectedAsync { get; init; }

    /// <summary>Drops a second selection while OnSelectedAsync is still running. Without it, holding enter through
    /// a long await, a model load say, starts the work several times over.</summary>
    public bool SingleFlight { get; init; } = true;

    protected override MenuItem Create(ILocalizer localizer) =>
        new(Text.Resolve(localizer), Description.Resolve(localizer));
}

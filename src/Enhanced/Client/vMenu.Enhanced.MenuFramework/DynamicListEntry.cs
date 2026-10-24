using System.Diagnostics.CodeAnalysis;

using MenuAPI;

using vMenu.Enhanced.Localization;

namespace vMenu.Enhanced.MenuFramework;

public sealed class DynamicListEntry : MenuEntry<MenuDynamicListItem>
{
    /// <summary>For text that is not a plain translation key: set <see cref="MenuEntry.Text"/> in the initializer.</summary>
    public DynamicListEntry()
    {
    }

    /// <summary>Takes the text from <paramref name="textKey"/>, and the description from <c>textKey + ".desc"</c> when
    /// English has that key. Setting Description in the initializer still wins.</summary>
    [SetsRequiredMembers]
    public DynamicListEntry(string textKey, Func<string> readValue, Func<DynamicListChanging, string> change)
        : base(textKey)
    {
        ReadValue = readValue;
        Change = change;
    }

    /// <summary>The value to show. Re-read on every refresh.</summary>
    public required Func<string> ReadValue { get; init; }

    /// <summary>Produces the next value. The framework wraps this before handing it to MenuAPI, because MenuAPI
    /// invokes it directly from GoLeft/GoRight without checking whether the item is enabled, so an
    /// unwrapped callback would run on a locked row.</summary>
    public required Func<DynamicListChanging, string> Change { get; init; }

    public Action<DynamicListChanged>? OnChanged { get; init; }

    public Action<DynamicListSelected>? OnSelected { get; init; }

    public Func<DynamicListSelected, Task>? OnSelectedAsync { get; init; }

    protected override MenuDynamicListItem Create(ILocalizer localizer) =>
        new(Text.Resolve(localizer), ReadValue(), Guarded, Description.Resolve(localizer));

    internal override void ApplyPresentation(ILocalizer localizer, GateBehaviour behaviour)
    {
        base.ApplyPresentation(localizer, behaviour);

        if (Typed is { } item)
        {
            item.CurrentItem = ReadValue();
        }
    }

    private string Guarded(MenuDynamicListItem item, bool left)
    {
        // MenuAPI allows CurrentItem to be null. Every entry the framework builds seeds it from ReadValue,
        // which cannot be, so this only covers a row something else re-seeded.
        var current = item.CurrentItem ?? string.Empty;

        return item.Enabled ? Change(new DynamicListChanging(item, current, left)) : current;
    }
}

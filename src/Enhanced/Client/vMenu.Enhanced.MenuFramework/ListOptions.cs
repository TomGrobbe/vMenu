using MenuAPI;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.MenuFramework;

internal static class ListOptions
{
    internal static List<string> Resolve(IReadOnlyList<MenuText> options, ILocalizer localizer)
    {
        var resolved = new List<string>(options.Count);

        foreach (var option in options)
        {
            resolved.Add(option.Resolve(localizer));
        }

        return resolved;
    }

    // Rewrites the values in place, so the current selection survives a language change. In place rather
    // than replaced because list values are addressed by index, never by their text.
    internal static void Rewrite(MenuListItem item, IReadOnlyList<MenuText> options, int selected, ILocalizer localizer)
    {
        item.ListItems.Clear();

        foreach (var option in options)
        {
            item.ListItems.Add(option.Resolve(localizer));
        }

        item.ListIndex = Clamp(selected, item.ListItems.Count);
    }

    // MenuAPI appends an "N/A" entry to an empty list while drawing, so zero is still valid.
    internal static int Clamp(int index, int count) => Math.Clamp(index, 0, Math.Max(0, count - 1));
}

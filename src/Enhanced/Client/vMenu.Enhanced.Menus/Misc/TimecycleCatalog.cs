using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Misc;

internal static class TimecycleCatalog
{
    private static string[]? _names;

    internal static IReadOnlyList<string> Names => Load();

    internal static string? Find(string name) =>
        Array.Find(Load(), known => string.Equals(known, name, StringComparison.OrdinalIgnoreCase));

    private static string[] Load() =>
        _names ??= [.. Enumerable.Range(0, Native.GetTimecycleModifierCount())
            .Select(Native.GetTimecycleModifierNameByIndex)
            .Order(StringComparer.OrdinalIgnoreCase)];
}

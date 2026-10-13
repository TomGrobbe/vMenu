using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Misc;

internal static class TimecycleCatalog
{
    private static string[]? _names;

    internal static IReadOnlyList<string> Names => Load();

    internal static string? Find(string name) =>
        Array.Find(Load(), known => string.Equals(known, name, StringComparison.OrdinalIgnoreCase));

    private static string[] Load() =>
        _names ??= [.. Enumerable.Range(0, API.GetTimecycleModifierCount())
            .Select(API.GetTimecycleModifierNameByIndex)
            .Order(StringComparer.OrdinalIgnoreCase)];
}

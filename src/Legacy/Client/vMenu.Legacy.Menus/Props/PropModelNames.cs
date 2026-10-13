using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Props;

internal static class PropModelNames
{
    private static readonly Dictionary<uint, string> Seen = [];

    internal static uint Remember(string model)
    {
        var hash = Hashing.Joaat(model);

        Seen[hash] = model;

        return hash;
    }

    internal static string Of(uint hash) => Seen.TryGetValue(hash, out var model) ? model : string.Empty;
}

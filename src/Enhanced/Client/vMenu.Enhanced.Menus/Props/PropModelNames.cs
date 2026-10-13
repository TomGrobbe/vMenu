using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Props;

internal static class PropModelNames
{
    private static readonly Dictionary<uint, string> Seen = [];

    internal static uint Remember(string model)
    {
        var hash = API.Hash(model);

        Seen[hash] = model;

        return hash;
    }

    internal static string Of(uint hash) => Seen.TryGetValue(hash, out var model) ? model : string.Empty;
}

using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Players.Appearance;

/// <summary>One game update's worth of clothes for a ped.</summary>
internal sealed class PedCollection(int index, string name)
{
    internal int Index { get; } = index;

    /// <summary>
    /// The name the natives take. An empty string is the base game, and that is not a placeholder,
    /// it is genuinely what the game calls it.
    /// </summary>
    // Legacy showed the base collection as the literal text "Base Collection" and then tested for
    // that same text to turn it back into an empty string. A DLC named that would have broken it.
    internal string Name { get; } = name;

    internal bool IsBaseGame => Name.Length == 0;
}

internal static class PedCollections
{
    /// <summary>Every collection this ped has clothes in, newest first.</summary>
    // Newest first because that is what people are usually looking for, and because the base game
    // sits at index zero and would otherwise bury four hundred rows of newer clothes under it.
    internal static List<PedCollection> All(int ped)
    {
        var collections = new List<PedCollection>();

        for (var index = Native.GetPedCollectionsCount(ped) - 1; index >= 0; index--)
        {
            collections.Add(new PedCollection(index, Native.GetPedCollectionName(ped, index) ?? string.Empty));
        }

        return collections;
    }

    internal static bool Has(int ped, string name)
    {
        if (name.Length == 0)
        {
            return true;
        }

        for (var index = Native.GetPedCollectionsCount(ped) - 1; index >= 0; index--)
        {
            if (string.Equals(Native.GetPedCollectionName(ped, index), name, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}

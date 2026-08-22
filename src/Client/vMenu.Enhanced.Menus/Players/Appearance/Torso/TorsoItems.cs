using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Players.Appearance.Torso;

internal static class TorsoItems
{
    private const uint NoItem = 1849449579;

    private const int LookupTexture = 0;

    private static readonly Dictionary<int, Dictionary<uint, int>> DrawableOfHashPerSlot = [];

    private static readonly Dictionary<int, int> IndexedCountPerSlot = [];

    private static uint _indexedModel;

    private static readonly Dictionary<string, uint> HashByName = new(StringComparer.Ordinal);

    internal static bool IsRealItem(uint item) => item is not (0 or NoItem);

    internal static uint HashOf(int ped, int slot, int drawable, int texture) =>
        (uint)Native.GetHashNameForComponent(ped, slot, drawable, texture);

    internal static uint HashOfFirstTexture(int ped, int slot, int drawable) =>
        HashOf(ped, slot, drawable, LookupTexture);

    internal static uint HashOfName(string itemName)
    {
        if (HashByName.TryGetValue(itemName, out var known))
        {
            return known;
        }

        var hash = (uint)Native.GetHashKey(itemName);

        HashByName[itemName] = hash;

        return hash;
    }

    internal static bool IsAnyNamed(uint item, string[] itemNames)
    {
        if (!IsRealItem(item))
        {
            return false;
        }

        foreach (var name in itemNames)
        {
            if (item == HashOfName(name))
            {
                return true;
            }
        }

        return false;
    }

    internal static int? DrawableOf(int ped, int slot, uint item)
    {
        if (!IsRealItem(item))
        {
            return null;
        }

        return Index(ped, slot).TryGetValue(item, out var drawable) ? drawable : null;
    }

    internal static int? DrawableOfNamed(int ped, int slot, string itemName) =>
        DrawableOf(ped, slot, (uint)Native.GetHashKey(itemName));

    internal static void Forget()
    {
        DrawableOfHashPerSlot.Clear();
        IndexedCountPerSlot.Clear();
        _indexedModel = 0;
    }

    private static Dictionary<uint, int> Index(int ped, int slot)
    {
        var model = (uint)Native.GetEntityModel(ped);

        if (model != _indexedModel)
        {
            DrawableOfHashPerSlot.Clear();
            IndexedCountPerSlot.Clear();
            _indexedModel = model;
        }

        var count = Native.GetNumberOfPedDrawableVariations(ped, slot);

        if (DrawableOfHashPerSlot.TryGetValue(slot, out var known)
            && IndexedCountPerSlot.TryGetValue(slot, out var indexedCount)
            && indexedCount == count)
        {
            return known;
        }

        var map = new Dictionary<uint, int>(count);

        for (var drawable = 0; drawable < count; drawable++)
        {
            var hash = HashOfFirstTexture(ped, slot, drawable);

            if (IsRealItem(hash))
            {
                map[hash] = drawable;
            }
        }

        DrawableOfHashPerSlot[slot] = map;
        IndexedCountPerSlot[slot] = count;

        return map;
    }
}

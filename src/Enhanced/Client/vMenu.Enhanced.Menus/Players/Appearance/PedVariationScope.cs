using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Players.Appearance;

// The two answer different questions. "Give me the next hat" wants everything the ped owns, and only
// the global list can walk that. "Show me what the summer update added" wants one collection. Legacy
// built a whole second menu for the second question; here one row builder asks a scope, and the
// scope knows which family of natives to call.
//
// A class rather than a record: generated equality reaches for EqualityComparer<T>.Default, which
// the client sandbox refuses to load.
internal sealed class PedVariationScope
{
    // Null is the whole wardrobe. A string is one collection, where empty is the base game.
    private readonly string? _collection;

    private PedVariationScope(string? collection) => _collection = collection;

    internal static PedVariationScope Global { get; } = new(null);

    internal static PedVariationScope ForCollection(string name) => new(name);

    internal bool IsCollection => _collection is not null;

    // The collection's real name, which is an empty string for the base game.
    internal string CollectionName => _collection ?? string.Empty;

    #region Components

    internal int DrawableCount(int ped, int slot) => _collection is { } collection
        ? Native.GetNumberOfPedCollectionDrawableVariations(ped, slot, collection)
        : Native.GetNumberOfPedDrawableVariations(ped, slot);

    internal int TextureCount(int ped, int slot, int drawable) => _collection is { } collection
        ? Native.GetNumberOfPedCollectionTextureVariations(ped, slot, collection, drawable)
        : Native.GetNumberOfPedTextureVariations(ped, slot, drawable);

    // Null when the ped is wearing something that did not come from this collection, which is a real
    // state rather than a missing one: there is nothing here to point at.
    internal int? CurrentDrawable(int ped, int slot)
    {
        if (_collection is not { } collection)
        {
            return Native.GetPedDrawableVariation(ped, slot);
        }

        return string.Equals(Native.GetPedDrawableVariationCollectionName(ped, slot), collection, StringComparison.Ordinal)
            ? Native.GetPedDrawableVariationCollectionLocalIndex(ped, slot)
            : null;
    }

    internal static int CurrentTexture(int ped, int slot) => Math.Max(0, Native.GetPedTextureVariation(ped, slot));

    internal static int CurrentPalette(int ped, int slot) => Math.Max(0, Native.GetPedPaletteVariation(ped, slot));

    // A few pieces are listed but have no model behind them on this build, and wearing one leaves the
    // ped with a hole in it.
    internal bool IsUsable(int ped, int slot, int drawable, int texture) => _collection is { } collection
        ? Native.IsPedCollectionComponentVariationValid(ped, slot, collection, drawable, texture)
        : Native.IsPedComponentVariationValid(ped, slot, drawable, texture);

    internal void SetComponent(int ped, int slot, int drawable, int texture, int palette)
    {
        if (_collection is { } collection)
        {
            Native.SetPedCollectionComponentVariation(ped, slot, collection, drawable, texture, palette);

            return;
        }

        Native.SetPedComponentVariation(ped, slot, drawable, texture, palette);
    }

    #endregion

    #region Props

    internal int PropCount(int ped, int slot) => _collection is { } collection
        ? Native.GetNumberOfPedCollectionPropDrawableVariations(ped, slot, collection)
        : Native.GetNumberOfPedPropDrawableVariations(ped, slot);

    internal int PropTextureCount(int ped, int slot, int drawable) => _collection is { } collection
        ? Native.GetNumberOfPedCollectionPropTextureVariations(ped, slot, collection, drawable)
        : Native.GetNumberOfPedPropTextureVariations(ped, slot, drawable);

    // Null covers both "nothing worn" and "worn, but not from this collection", which the row shows the
    // same way either way.
    internal int? CurrentProp(int ped, int slot)
    {
        // The false is the dead check the enhanced natives added, off because a ped wears what it wears
        // whether or not it happens to be down.
        if (Native.GetPedPropIndex(ped, slot, false) is var global && global < 0)
        {
            return null;
        }

        if (_collection is not { } collection)
        {
            return global;
        }

        return string.Equals(Native.GetPedPropCollectionName(ped, slot), collection, StringComparison.Ordinal)
            ? Native.GetPedPropCollectionLocalIndex(ped, slot)
            : null;
    }

    // Only asked to tell "nothing worn" apart from "worn, but from another collection", which look the
    // same to CurrentProp and read very differently to a player.
    internal static int GlobalProp(int ped, int slot) => Native.GetPedPropIndex(ped, slot, false);

    internal static int CurrentPropTexture(int ped, int slot) => Math.Max(0, Native.GetPedPropTextureIndex(ped, slot));

    internal void SetProp(int ped, int slot, int drawable, int texture)
    {
        // True attaches the prop, which is what makes it survive the ped being redrawn.
        if (_collection is { } collection)
        {
            Native.SetPedCollectionPropIndex(ped, slot, collection, drawable, texture, true);

            return;
        }

        Native.SetPedPropIndex(ped, slot, drawable, texture, true, false);
    }

    internal static void ClearProp(int ped, int slot) => Native.ClearPedProp(ped, slot, false);

    #endregion
}

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Appearance;
using vMenu.Enhanced.Menus.Appearance;

namespace vMenu.Enhanced.Menus.Players.Appearance;

/// <summary>
/// Puts a saved appearance back onto a ped, and checks that it took.
/// </summary>
/// <remarks>
/// One pass is enough, unlike the vehicle writer's three. Vehicle upgrades have to stream in before
/// the game will accept them, so applying them is a wait-and-retry affair. Ped components are not
/// streamed: the game either has the piece or it does not, and asking a second time will not conjure
/// it. Anything still different after one pass is a piece this client is missing, which is worth
/// reporting rather than retrying.
/// </remarks>
public static class PedAppearanceWriter
{
    /// <summary>Applies an appearance, and reports whatever would not stick.</summary>
    /// <returns>Empty when the ped now matches exactly.</returns>
    public static async Task<List<AppearanceDifference>> ApplyAsync(int ped, PedAppearance appearance)
    {
        Apply(ped, appearance);

        // One frame, so the game has drawn the ped with what it was given before it is read back.
        await API.Delay(0);

        return PedAppearanceDiff.Compare(appearance, PedAppearanceReader.Read(ped));
    }

    /// <summary>One pass. Every call here is idempotent, so repeating it is safe.</summary>
    public static void Apply(int ped, PedOutfit outfit)
    {
        // A clean base first, so a slot the save says nothing about comes back as the model's own
        // default rather than whatever the ped worn before this one left in it. It is also what lets
        // an absent prop mean "nothing worn" without the save having to spell that out.
        Native.SetPedDefaultComponentVariation(ped);
        Native.ClearAllPedProps(ped, false);

        foreach (var component in outfit.Components)
        {
            ApplyComponent(ped, component);
        }

        foreach (var prop in outfit.Props)
        {
            ApplyProp(ped, prop);
        }
    }

    // Checked against the live counts rather than clamped into range. A drawable this model does not
    // have is a piece the player is missing, and quietly dressing them in a different one would hide
    // that from both the player and the diff.
    private static void ApplyComponent(int ped, PedComponentValue component)
    {
        if (ApplyComponentFromCollection(ped, component))
        {
            return;
        }

        if (component.LocalDrawable >= 0 && !PedCollections.Has(ped, component.Collection))
        {
            return;
        }

        if (component.Drawable < 0
            || component.Drawable >= Native.GetNumberOfPedDrawableVariations(ped, component.Slot))
        {
            return;
        }

        var textures = Native.GetNumberOfPedTextureVariations(ped, component.Slot, component.Drawable);

        if (component.Texture < 0 || component.Texture >= textures)
        {
            return;
        }

        Native.SetPedComponentVariation(ped, component.Slot, component.Drawable, component.Texture, component.Palette);
    }

    private static bool ApplyComponentFromCollection(int ped, PedComponentValue component)
    {
        if (component.LocalDrawable < 0 || !PedCollections.Has(ped, component.Collection))
        {
            return false;
        }

        var drawables = Native.GetNumberOfPedCollectionDrawableVariations(ped, component.Slot, component.Collection);

        if (component.LocalDrawable >= drawables)
        {
            return false;
        }

        var textures = Native.GetNumberOfPedCollectionTextureVariations(
            ped, component.Slot, component.Collection, component.LocalDrawable);

        if (component.Texture < 0 || component.Texture >= textures)
        {
            return false;
        }

        Native.SetPedCollectionComponentVariation(
            ped, component.Slot, component.Collection, component.LocalDrawable, component.Texture, component.Palette);

        return true;
    }

    private static void ApplyProp(int ped, PedPropValue prop)
    {
        if (ApplyPropFromCollection(ped, prop))
        {
            return;
        }

        if (prop.LocalDrawable >= 0 && !PedCollections.Has(ped, prop.Collection))
        {
            return;
        }

        if (prop.Drawable < 0 || prop.Drawable >= Native.GetNumberOfPedPropDrawableVariations(ped, prop.Slot))
        {
            return;
        }

        var textures = Native.GetNumberOfPedPropTextureVariations(ped, prop.Slot, prop.Drawable);

        if (prop.Texture < 0 || prop.Texture >= textures)
        {
            return;
        }

        // True attaches the prop, which is what makes it survive the ped being redrawn. False is the
        // dead check the enhanced natives added, off so a restore works on a ped that is down.
        Native.SetPedPropIndex(ped, prop.Slot, prop.Drawable, prop.Texture, true, false);
    }

    private static bool ApplyPropFromCollection(int ped, PedPropValue prop)
    {
        if (prop.LocalDrawable < 0 || !PedCollections.Has(ped, prop.Collection))
        {
            return false;
        }

        var drawables = Native.GetNumberOfPedCollectionPropDrawableVariations(ped, prop.Slot, prop.Collection);

        if (prop.LocalDrawable >= drawables)
        {
            return false;
        }

        var textures = Native.GetNumberOfPedCollectionPropTextureVariations(
            ped, prop.Slot, prop.Collection, prop.LocalDrawable);

        if (prop.Texture < 0 || prop.Texture >= textures)
        {
            return false;
        }

        Native.SetPedCollectionPropIndex(
            ped, prop.Slot, prop.Collection, prop.LocalDrawable, prop.Texture, true);

        return true;
    }
}

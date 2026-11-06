using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Appearance;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Menus.Appearance;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Players.Appearance;

// One pass is enough, unlike the vehicle writer's three. Vehicle upgrades have to stream in before
// the game will accept them; ped components are not streamed, so the game either has the piece or it
// does not. Anything still different after one pass is a piece this client is missing, which is
// worth reporting rather than retrying.
public static class PedAppearanceWriter
{
    // Empty when the ped now matches exactly.
    public static async Task<List<AppearanceDifference>> ApplyAsync(int ped, PedAppearance appearance)
    {
        // Quiet here, the caller already reports every difference, these included.
        Wear(ped, appearance);

        // One frame, so the game has drawn the ped with what it was given before it is read back.
        await BaseScript.Delay(0);

        return PedAppearanceDiff.Compare(appearance, PedAppearanceReader.Read(ped));
    }

    // One pass. Every call here is idempotent, so repeating it is safe.
    public static void Apply(int ped, PedOutfit outfit, bool warnLeftOut = true)
    {
        var leftOut = Wear(ped, outfit);

        if (warnLeftOut && leftOut > 0)
        {
            Notifications.Warning(MenuText.Key(
                Loc.PlayerAppearance.Gen9LeftOut,
                ("count", MenuText.Literal(leftOut.ToString(CultureInfo.InvariantCulture)))));
        }
    }

    private static int Wear(int ped, PedOutfit outfit)
    {
        // A clean base first, so a slot the save says nothing about comes back as the model's own default
        // rather than whatever the ped worn before this one left in it. It is also what lets an absent prop
        // mean "nothing worn" without the save having to spell that out.
        API.SetPedDefaultComponentVariation(ped);
        NativeFixer.ClearAllPedProps(ped, false);

        var leftOut = 0;

        foreach (var component in outfit.Components)
        {
            if (!ApplyComponent(ped, component))
            {
                leftOut++;
            }
        }

        foreach (var prop in outfit.Props)
        {
            ApplyProp(ped, prop);
        }

        return leftOut;
    }

    // Checked against the live counts rather than clamped into range. A drawable this model does not
    // have is a piece the player is missing, and quietly dressing them in a different one would hide
    // that from both the player and the diff.
    private static bool ApplyComponent(int ped, PedComponentValue component)
    {
        if (ApplyComponentFromCollection(ped, component) is bool applied)
        {
            return applied;
        }

        if (component.LocalDrawable >= 0 && !PedCollections.Has(ped, component.Collection))
        {
            return true;
        }

        if (component.Drawable < 0
            || component.Drawable >= API.GetNumberOfPedDrawableVariations(ped, component.Slot))
        {
            return true;
        }

        var textures = API.GetNumberOfPedTextureVariations(ped, component.Slot, component.Drawable);

        if (component.Texture < 0 || component.Texture >= textures)
        {
            return true;
        }

        if (PedVariationScope.Global.IsGen9Exclusive(ped, component.Slot, component.Drawable))
        {
            return false;
        }

        API.SetPedComponentVariation(ped, component.Slot, component.Drawable, component.Texture, component.Palette);

        return true;
    }

    // Null when the collection cannot place it, so the global ids get a try instead.
    private static bool? ApplyComponentFromCollection(int ped, PedComponentValue component)
    {
        if (component.LocalDrawable < 0 || !PedCollections.Has(ped, component.Collection))
        {
            return null;
        }

        var drawables = API.GetNumberOfPedCollectionDrawableVariations(ped, component.Slot, component.Collection);

        if (component.LocalDrawable >= drawables)
        {
            return null;
        }

        var textures = API.GetNumberOfPedCollectionTextureVariations(
            ped, component.Slot, component.Collection, component.LocalDrawable);

        if (component.Texture < 0 || component.Texture >= textures)
        {
            return null;
        }

        if (PedVariationScope.ForCollection(component.Collection).IsGen9Exclusive(ped, component.Slot, component.LocalDrawable))
        {
            return false;
        }

        API.SetPedCollectionComponentVariation(
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

        if (prop.Drawable < 0 || prop.Drawable >= API.GetNumberOfPedPropDrawableVariations(ped, prop.Slot))
        {
            return;
        }

        var textures = API.GetNumberOfPedPropTextureVariations(ped, prop.Slot, prop.Drawable);

        if (prop.Texture < 0 || prop.Texture >= textures)
        {
            return;
        }

        // True attaches the prop, which is what makes it survive the ped being redrawn. False is the dead
        // check the enhanced natives added, off so a restore works on a ped that is down.
        NativeFixer.SetPedPropIndex(ped, prop.Slot, prop.Drawable, prop.Texture, true, false);
    }

    private static bool ApplyPropFromCollection(int ped, PedPropValue prop)
    {
        if (prop.LocalDrawable < 0 || !PedCollections.Has(ped, prop.Collection))
        {
            return false;
        }

        var drawables = API.GetNumberOfPedCollectionPropDrawableVariations(ped, prop.Slot, prop.Collection);

        if (prop.LocalDrawable >= drawables)
        {
            return false;
        }

        var textures = API.GetNumberOfPedCollectionPropTextureVariations(
            ped, prop.Slot, prop.Collection, prop.LocalDrawable);

        if (prop.Texture < 0 || prop.Texture >= textures)
        {
            return false;
        }

        API.SetPedCollectionPropIndex(
            ped, prop.Slot, prop.Collection, prop.LocalDrawable, prop.Texture, true);

        return true;
    }
}
